using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Drive.v3;
using log4net;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MMG_IIA.Common;
using MMG_IIA.Dto;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;
using Tesseract;
using TesseractOCR;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Point = OpenCvSharp.Point;
using Window = System.Windows.Window;

namespace MMG_IIA
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string[] scopes = { DriveService.Scope.Drive }; // スコープを設定
        private readonly string applicationName; // アプリケーション名
        private readonly string credentialsPath; // 認証情報ファイルのパス
        private readonly string mimeTypeDocument = "application/vnd.google-apps.document"; // GoogleドキュメントのMIMEタイプ
        private readonly string MATCH_TEMP_FILE_NAME = "_match.bmp";
        private readonly string trainingModelLanguage; // 使用する言語を指定
        private readonly string ocrLanguage; // OCRの言語
        private readonly string parentId; // アップロード先の親フォルダID
        private readonly double matchThreshold; // マッチング閾値
        private string trainingModelPath = AppDomain.CurrentDomain.BaseDirectory + "tessdata"; // 一般的tessdataのパス
        private string imageFilePath; // 画像ファイルパス
        private string characterMatchingFilePath; // キャラクター画像ファイルパス
        private string uploadFileId; // アップロードしたファイルのID
        private string documentId; // ドキュメントのID
        private string ocrText; // OCR結果のテキスト
        private bool isImageWindow; // 画像ウィンドウの表示フラグ
        private bool isCharacterMatchingView; // キャラクター画像マッチングの表示フラグ
        private List<string> formationCharacterNameList; // 編成キャラクター名リスト
        private Dictionary<string, Point> characterPointDic; // キャラクターのポイント辞書
        private readonly Dictionary<int, List<CharacterDto>> characterDic; // キャラクター辞書
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        /// <summary>
        /// OCRの種類を定義する列挙体
        /// </summary>
        /// <remarks>GoogleDriveAPI、Tesseract、TesseractOCR、None</remarks>
        public enum OCRType
        {
            GoogleDriveAPI,
            Tesseract,
            TesseractOCR,
            None,
        }

        /// <summary>
        /// MainWindowのコンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // 初期化処理
                isImageWindow = false;
                isCharacterMatchingView = false;
                characterPointDic = new Dictionary<string, Point>();
                characterDic = new Dictionary<int, List<CharacterDto>>();
                applicationName = Properties.Settings.Default["GoogleAPIApplicationName"].ToString();
                credentialsPath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default["GoogleAPICredentialsPath"].ToString();
                parentId = Properties.Settings.Default["GoogleAPIDriveParentId"].ToString();
                trainingModelLanguage = Properties.Settings.Default["TrainingModelLanguage"].ToString();
                ocrLanguage = Properties.Settings.Default["OCRLanguage"].ToString();
                matchThreshold = Properties.Settings.Default["MATCH_THRESHOLD"] == null ? 0.8 : double.Parse(Properties.Settings.Default["MATCH_THRESHOLD"].ToString());
                characterMatchingFilePath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default["MATCH_GUILDVS_CHARACTER_DIR"].ToString();
                TextBoxCharacterMatchingFilePath.Text = characterMatchingFilePath;
                TextBoxTrainingModelFilePath.Text = trainingModelPath;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// GoogleDriveAPIを使用してファイルをアップロードする
        /// </summary>
        /// <param name="driveService">Googleドライブサービス</param>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>完了タスク</returns>
        private Task UploadFileAsync(DriveService driveService, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                string mimeType = MimeMapping.GetMimeMapping(filePath);

                Google.Apis.Drive.v3.Data.File meta = new Google.Apis.Drive.v3.Data.File
                {
                    Name = Path.GetFileName(filePath),
                    MimeType = mimeType,
                    Parents = new List<string>() { parentId }
                };

                var request = driveService.Files.Create(meta, fs, mimeType);

                var response = request.Upload();

                if (response.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    uploadFileId = request.ResponseBody.Id;
                }
                else
                {
                    MessageBox.Show("アップロードに失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// GoogleDriveAPIを使用して画像をGoogleドキュメントに変換する
        /// </summary>
        /// <param name="driveService">Googleドライブサービス</param>
        /// <param name="imageId">グーグルドライブ画像ID</param>
        /// <returns>完了タスク</returns>
        private Task ConvertImageToGoogleDocumentAsync(DriveService driveService, string imageId)
        {
            var metadata = new Google.Apis.Drive.v3.Data.File
            {
                MimeType = mimeTypeDocument
            };

            var copyRequest = driveService.Files.Copy(metadata, imageId);
            copyRequest.OcrLanguage = ocrLanguage;
            documentId = copyRequest.Execute().Id;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Googleドキュメントの要素を読み取る
        /// </summary>
        /// <param name="element">要素</param>
        /// <returns>テキスト</returns>
        private string ReadParagraphElements(ParagraphElement element)
        {
            var textRun = element.TextRun;

            if (textRun == null || textRun.Content == null)
            {
                return "";
            }
            else
            {
                return textRun.Content;
            }
        }

        /// <summary>
        /// Googleドキュメントの構造要素を読み取る
        /// </summary>
        /// <param name="elements">構造要素</param>
        /// <returns>テキスト</returns>
        private string ReadStructuralElements(IList<StructuralElement> elements)
        {
            var text = "";

            foreach (var element in elements)
            {
                if (element.Paragraph != null)
                {
                    foreach (var paragraphElement in element.Paragraph.Elements)
                    {
                        text += ReadParagraphElements(paragraphElement);
                    }
                }
                else if (element.Table != null)
                {
                    foreach (var tableRow in element.Table.TableRows)
                    {
                        foreach (var tableCell in tableRow.TableCells)
                        {
                            text += ReadStructuralElements(tableCell.Content);
                        }
                    }
                }
                else if (element.TableOfContents != null)
                {
                    text += ReadStructuralElements(element.TableOfContents.Content);
                }
            }

            return text;
        }

        /// <summary>
        /// Googleドキュメントからテキストを取得する
        /// </summary>
        /// <param name="docsService">Googleドキュメントサービス</param>
        /// <param name="documentId">ドキュメントID</param>
        /// <returns>テキスト</returns>
        private string GetGoogleDocumentText(DocsService docsService, string documentId)
        {
            var getRequest = docsService.Documents.Get(documentId);
            var document = getRequest.Execute();
            var elements = document.Body.Content;

            return ReadStructuralElements(elements);
        }

        /// <summary>
        /// GoogleドキュメントOCRを実行する
        /// </summary>
        /// <param name="driveService">Googleドライブサービス</param>
        /// <param name="docsService">Googleドキュメントサービス</param>
        /// <param name="imageFilePath">画像ファイルパス</param>
        /// <returns>テキスト</returns>
        private string GoogleDocumentOcr(DriveService driveService, DocsService docsService, string imageFilePath)
        {
            UploadFileAsync(driveService, imageFilePath).Wait();

            ConvertImageToGoogleDocumentAsync(driveService, uploadFileId).Wait();

            return GetGoogleDocumentText(docsService, documentId);
        }

        /// <summary>
        /// GoogleTesseractで画像からOCRを実行し、テキストブロックに結果を表示する
        /// </summary>
        /// <param name="type">OCRの種類</param>
        private void GoogleTesseractOCR(OCRType type)
        {
            try
            {
                if (OCRType.Tesseract == type)
                {
                    var engine = new TesseractEngine(trainingModelPath, trainingModelLanguage);
                    using (var pix = Pix.LoadFromFile(imageFilePath))
                    {
                        var page = engine.Process(pix);
                        ocrText = page.GetText();
                    }
                }
                else if (OCRType.TesseractOCR == type)
                {
                    var engineOCR = new Engine(trainingModelPath, trainingModelLanguage);
                    using (var pix = TesseractOCR.Pix.Image.LoadFromFile(imageFilePath))
                    {
                        var pageOCR = engineOCR.Process(pix);
                        ocrText = pageOCR.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// GoogleDriveAPIアクセスして画像からOCRを実行し、テキストブロックに結果を表示する
        /// </summary>
        private void GoogleDriveDocumentOcr()
        {
            try
            {
                // 認証情報ファイルのパスを指定
                Google.Apis.Auth.OAuth2.GoogleCredential credential;

                using (var fs = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromStream(fs).CreateScoped(scopes);
                }

                // 認証情報を保存するためのストレージを指定
                Google.Apis.Services.BaseClientService.Initializer init = new Google.Apis.Services.BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = applicationName
                };

                DriveService driveService = new DriveService(init);
                DocsService docsService = new DocsService(init);

                ocrText = GoogleDocumentOcr(driveService, docsService, imageFilePath);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show("OCRに失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 入力チェックを行う
        /// </summary>
        /// <param name="type">OCRType</param>
        /// <exception cref="Exception">入力チェックエラーメッセージ</exception>
        private void InputCheck(OCRType type)
        {
            if (string.IsNullOrEmpty(imageFilePath))
            {
                throw new Exception("画像ファイルを選択してください。");
            }
            if (!File.Exists(imageFilePath))
            {
                throw new Exception("指定されたファイルが存在しません。");
            }

            if (OCRType.GoogleDriveAPI == type)
            {
                if (string.IsNullOrEmpty(credentialsPath))
                {
                    throw new Exception("設定ファイルで認証情報ファイル(GoogleAPICredentialsPath)のパスを指定してください。");
                }
                if (File.Exists(credentialsPath) == false)
                {
                    throw new Exception("指定された認証情報ファイルが存在しません。");
                }
                if (string.IsNullOrEmpty(parentId))
                {
                    throw new Exception("設定ファイルでGoogleDriveのアップロード先親フォルダID(GoogleAPIDriveParentId)を指定してください。");
                }
                if (string.IsNullOrEmpty(applicationName))
                {
                    throw new Exception("設定ファイルでGoogleDriveのアプリケーション名(GoogleAPIApplicationName)を指定してください。");
                }
            }
            else if (OCRType.Tesseract == type || OCRType.TesseractOCR == type)
            {
                if (string.IsNullOrEmpty(trainingModelPath))
                {
                    throw new Exception("tessdataのパスを指定してください。");
                }
                if (Directory.Exists(trainingModelPath) == false)
                {
                    throw new Exception("指定されたtessdataのパスが存在しません。");
                }
                if (string.IsNullOrEmpty(trainingModelLanguage))
                {
                    throw new Exception("設定ファイルで使用する言語(TrainingModelLanguage)を指定してください。");
                }
                if (string.IsNullOrEmpty(ocrLanguage))
                {
                    throw new Exception("設定ファイルでOCRの言語(OCRLanguage)を指定してください。");
                }
            }
        }

        /// <summary>
        /// 画像を4分割して、マッチング精度を上げるための画像ファイルパスを取得する
        /// </summary>
        /// <param name="baseFilePath">元ファイルパス</param>
        /// <returns>画像4分割ファイルパス</returns>
        private string[] GetDivImageFilePath(string baseFilePath)
        {
            List<string> resultList = new List<string>();

            try
            {
                string inFilePath = AppDomain.CurrentDomain.BaseDirectory
                                    + Properties.Settings.Default["MATCH_TEMP_DIR"].ToString()
                                    + Properties.Settings.Default["MATCH_TEMP_DIV_DIR"].ToString();
                inFilePath += Path.GetFileNameWithoutExtension(baseFilePath);

                resultList.Add(inFilePath + "_1.bmp");
                resultList.Add(inFilePath + "_2.bmp");
                resultList.Add(inFilePath + "_3.bmp");
                resultList.Add(inFilePath + "_4.bmp");

                Bitmap srcImage = new Bitmap(baseFilePath);

                // 画像を切り抜く範囲を指定(偵察情報画面を基準とする)
                int x = 280;
                int y = 210;
                int width = 1000;
                int height = 80;
                int heightCnt = 0;

                foreach (string filePath in resultList)
                {
                    Rectangle rect = new Rectangle(x, y, width, height);
                    Bitmap destImage = srcImage.Clone(rect, srcImage.PixelFormat);

                    destImage.Save(filePath, ImageFormat.Bmp);
                    destImage.Dispose();

                    y += 95 + heightCnt;
                    heightCnt += 5;
                }

                srcImage.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }

            return resultList.ToArray();
        }

        /// <summary>
        /// キャラクターの高さを範囲内の中間値で固定する
        /// </summary>
        private void FixedIntermediateValueWithInHeightRange()
        {
            try
            {
                Dictionary<string, Point> newDic = new Dictionary<string, Point>();

                // Pointから範囲内の高さの集合体から平均値を求めて、その値で上書きする。
                foreach (KeyValuePair<string, Point> keyValuePair in characterPointDic)
                {
                    string sd = keyValuePair.Value.Y.ToString().Substring(0, 1) + "." + keyValuePair.Value.Y.ToString().Substring(1);
                    double yd = double.Parse(sd);
                    int y = (int)Math.Ceiling(yd);

                    Point point = characterPointDic[keyValuePair.Key];
                    Point newP = new Point(point.X, y);

                    newDic.Add(keyValuePair.Key, newP);
                }

                characterPointDic = newDic;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 編成キャラクター名リストを取得する
        /// </summary>
        /// <param name="imageFilePath">画像ファイルパス</param>
        /// <exception cref="Exception">編成キャラクター名取得エラー</exception>
        private void LoadFormationCharacterNameList(string imageFilePath)
        {
            try
            {
                // マッチングのために一時ファイルとして出力
                string matchTempDirPath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default["MATCH_TEMP_DIR"].ToString();

                if (!Directory.Exists(matchTempDirPath))
                {
                    Directory.CreateDirectory(matchTempDirPath);
                }

                string tempFilePath = matchTempDirPath + DateTime.Now.ToString("yyyyMMddHHmmssfff") + MATCH_TEMP_FILE_NAME;
                Bitmap bitmap = new Bitmap(imageFilePath);
                bitmap.Save(tempFilePath, ImageFormat.Bmp);

                // 取り込んだ画像を4編成分の区切り加工して4画像ファイル分とする。(マッチング精度を上げる)
                if (string.IsNullOrEmpty(tempFilePath))
                {
                    return;
                }

                string matchTempDivDirPath = matchTempDirPath + Properties.Settings.Default["MATCH_TEMP_DIV_DIR"].ToString();

                if (!Directory.Exists(matchTempDivDirPath))
                {
                    Directory.CreateDirectory(matchTempDivDirPath);
                }

                string[] filePaths = GetDivImageFilePath(tempFilePath);
                formationCharacterNameList = new List<string>();
                BaseCharacterDto[] baseCharacterDtos = BaseCharacter.GetBaseCharacterDtos();
                int no = 0;
                characterDic.Clear();

                foreach (string filePath in filePaths)
                {
                    CharacterMatching(filePath);

                    // 高さがテンプレート画像によって±の誤差が発生するため、範囲内の高さを中間値で固定する。
                    FixedIntermediateValueWithInHeightRange();

                    // マッチング位置から、キャラクターのグルーピングを行う
                    IOrderedEnumerable<KeyValuePair<string, Point>> sorted = characterPointDic.OrderBy(y => y.Value.Y).ThenBy(x => x.Value.X);

                    no++;

                    foreach (KeyValuePair<string, Point> keyValuePair in sorted)
                    {
                        if (!characterDic.ContainsKey(no))
                        {
                            characterDic.Add(no, new List<CharacterDto>());
                        }

                        BaseCharacterDto baseCharacterDto = baseCharacterDtos.Where(x => x.Name.Contains(keyValuePair.Key)).FirstOrDefault();

                        CharacterDto characterDto = new CharacterDto()
                        {
                            Name = keyValuePair.Key,
                        };

                        characterDic[no].Add(characterDto);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("編成キャラクター名の取得に失敗しました", ex);
            }
        }

        /// <summary>
        ///　キャラクター画像マッチングを行う
        /// </summary>
        /// <param name="imageFilePath">画像ファイルパス</param>
        private void CharacterMatching(string imageFilePath)
        {
            var targetMat = new Mat(imageFilePath);

            // テンプレート画像で回してマッチするものを探す
            string matchGuildvsCharacterDir = Properties.Settings.Default["MATCH_GUILDVS_CHARACTER_DIR"].ToString();
            string pmDirPath = AppDomain.CurrentDomain.BaseDirectory + matchGuildvsCharacterDir;

            string[] templateFiles = Directory.GetFiles(pmDirPath, "*", SearchOption.TopDirectoryOnly);

            characterPointDic.Clear();

            foreach (var fileName in templateFiles)
            {
                var templateMat = new Mat(fileName);

                var match = Matching(targetMat, templateMat, out var maxPoint);

                if (match < 0)
                {
                    continue;
                }

                MMatching(targetMat, templateMat);

                string name = Path.GetFileNameWithoutExtension(fileName);
                string[] names = name.Split('_');
                string newFileName = Path.GetDirectoryName(fileName) + "/" + names[0] + Path.GetExtension(fileName);

                string charaFilePath = newFileName.Replace(matchGuildvsCharacterDir, Properties.Settings.Default["FormationCharaImagePath"].ToString());

                charaFilePath = charaFilePath.Replace(".bmp", ".jpg");

                string charaFileName = Path.GetFileNameWithoutExtension(charaFilePath);

                formationCharacterNameList.Add(charaFileName);

                if (characterPointDic.ContainsKey(names[0]))
                {
                    continue;
                }
                else
                {
                    characterPointDic.Add(names[0], maxPoint);
                }
            }

            if (isCharacterMatchingView)
            {
                Cv2.ImShow("マッチング結果", targetMat);
                Cv2.WaitKey(0);
            }
        }

        /// <summary>
        /// マッチングを行い、対象に赤枠を表示する。
        /// </summary>
        /// <param name="targetMat">対象Mat</param>
        /// <param name="templateMat">テンプレートMat</param>
        private void MMatching(Mat targetMat, Mat templateMat)
        {
            // 検索対象の画像とテンプレート画像
            Mat mat = targetMat;
            Mat temp = templateMat;

            using (Mat result = new Mat())
            {
                // テンプレートマッチ
                Cv2.MatchTemplate(mat, temp, result, TemplateMatchModes.CCoeffNormed);

                // しきい値の範囲に絞る
                Cv2.Threshold(result, result, matchThreshold, 1.0, ThresholdTypes.Tozero);

                while (true)
                {
                    // 類似度が最大/最小となる画素の位置を調べる
                    Cv2.MinMaxLoc(result, out double minval, out double maxval, out Point minloc, out Point maxloc);

                    if (maxval >= matchThreshold)
                    {
                        // 見つかった場所に赤枠を表示
                        OpenCvSharp.Rect rect = new OpenCvSharp.Rect(maxloc.X, maxloc.Y, temp.Width, temp.Height);
                        Cv2.Rectangle(mat, rect, new Scalar(0, 0, 255), 2);

                        // 見つかった箇所は塗りつぶす
                        Cv2.FloodFill(result, maxloc, new Scalar(0), out OpenCvSharp.Rect outRect, new Scalar(0.1),
                                    new Scalar(1.0), FloodFillFlags.Link4);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// マッチングを行い、閾値を返す。
        /// </summary>
        /// <param name="targetMat">対象Mat</param>
        /// <param name="templateMat">テンプレートMat</param>
        /// <param name="matchPoint">マッチングポイント</param>
        /// <returns>閾値</returns>
        private double Matching(Mat targetMat, Mat templateMat, out Point matchPoint)
        {
            // 探索画像を二値化
            var targetBinMat = new Mat();
            Cv2.CvtColor(targetMat, targetBinMat, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(targetBinMat, targetBinMat, 128, 255, ThresholdTypes.Binary);

            // テンプレ画像を二値化
            var templateBinMat = new Mat();
            Cv2.CvtColor(templateMat, templateBinMat, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(templateBinMat, templateBinMat, 128, 255, ThresholdTypes.Binary);

            // マッチング
            var resultMat = new Mat();
            Cv2.MatchTemplate(targetBinMat, templateBinMat, resultMat, TemplateMatchModes.CCoeffNormed);

            // 一番マッチした箇所のマッチ具合（0～1）と、その位置を取得する（画像内でマッチした左上座標）
            Cv2.MinMaxLoc(resultMat, out _, out var maxVal, out _, out matchPoint);

            if (maxVal < matchThreshold)
            {
                return -1.0;
            }

            // 閾値超えのマッチ箇所を強調させておく
            var binMat = new Mat();
            Cv2.Threshold(resultMat, binMat, matchThreshold, 1.0, ThresholdTypes.Binary);

            return maxVal;
        }

        /// <summary>
        /// MMG形式に変換する
        /// </summary>
        /// <param name="text">OCRテキスト</param>
        /// <returns>MMG形式テキスト</returns>
        /// <exception cref="Exception">MMG形式変換エラー</exception>
        private string CovertToMMGFormat(string text)
        {
            MMGPlayOCRDto mmgPlayOCRDto = new MMGPlayOCRDto();
            try
            {
                List<string> lines = new List<string>(text.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.None));
                List<FormationDto> formationDtoList = new List<FormationDto>();
                FormationDto formationDto = new FormationDto();
                List<CharacterDto> characterDtoList = new List<CharacterDto>();
                string baseName = string.Empty;
                int sumPartyNum = 0;
                string playerName = string.Empty;
                List<string> formationLvList = new List<string>();
                int formationPosition = -1;
                int rank = -1;
                long combatPower = -1;
                int cnt = lines.Count;
                int no = 1;

                if (CheckBoxCharacterMatching.IsChecked == true)
                {
                    // キャラクター画像をマッチングしてキャラクターを取得する
                    LoadFormationCharacterNameList(imageFilePath);
                }

                for (int i = 0; i < cnt; i++)
                {
                    string line = lines[i];

                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(baseName))
                    {
                        // 陣地名(稀にゴミ記号が含まれる場合があるので実際の陣地名を元にチェック)
                        if (BaseGBGroup.baseNames.Any(k => line.Contains(k.Value)) == false)
                        {
                            continue;
                        }
                        else
                        {
                            // 陣地名を取得
                            baseName = BaseGBGroup.baseNames.FirstOrDefault(k => line.Contains(k.Value)).Value;
                            mmgPlayOCRDto.BaseName = baseName;
                            continue;
                        }
                    }

                    if (0 >= sumPartyNum)
                    {
                        if (line.Contains("合計パーティ数"))
                        {
                            continue;
                        }
                        else
                        {
                            // 陣地名の前後に防衛実施、防衛中パーティ、進攻中パーティなどの不要なゴミが入ることがあるので対応
                            if (int.TryParse(line.Trim(), out sumPartyNum))
                            {
                                // 合計パーティ数
                                mmgPlayOCRDto.SumPartyNum = sumPartyNum;
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    // 編成情報(画面の表示枠的に最大4つ ※5つ目が一部見切れて表示されることがある)
                    //  |_プレイヤー名
                    if (string.IsNullOrEmpty(playerName))
                    {
                        playerName = line; // プレイヤー名を「 」にしているパターンあり
                        formationDto.PlayerName = playerName;
                        continue;
                    }

                    //  |_キャラクター情報(最大5)
                    //     |_Lv(一部が異なる値となることがある 画像文字が小さすぎる?)
                    if (line.ToUpper().Contains("LV"))
                    {
                        string[] lvLines = line.Split(new[] { " " }, StringSplitOptions.None);

                        foreach (string lvLine in lvLines)
                        {
                            string lv = System.Text.RegularExpressions.Regex.Replace(lvLine, "[^0-9]", "");

                            if (string.IsNullOrEmpty(lv) == false)
                            {
                                formationLvList.Add(lv);
                            }
                        }

                        continue;
                    }

                    if (0 < formationLvList.Count)
                    {
                        if (characterDic.Count < no)
                        {
                            break;
                        }

                        List<CharacterDto> characterNameDtoList = characterDic[no].ToList();

                        for (int j = 0; j < formationLvList.Count; j++)
                        {
                            // キャラクター名を取得
                            string charaName = string.Empty;
                            if (characterNameDtoList.Count > j)
                            {
                                charaName = characterNameDtoList[j].Name;
                            }

                            // キャラクター情報を作成
                            CharacterDto characterDto = new CharacterDto
                            {
                                Name = charaName,
                                Level = int.Parse(formationLvList[j])
                            };
                            characterDtoList.Add(characterDto);
                        }

                        formationDto.CharacterDtos = characterDtoList.ToArray();
                        formationLvList.Clear();
                    }

                    //  |_編成位置
                    if (0 > formationPosition)
                    {
                        int.TryParse(line.Trim(), out formationPosition);
                        formationDto.FormationPosition = formationPosition;
                        continue;
                    }

                    //  |_ランク
                    if (0 > rank)
                    {
                        if (line.Contains("ランク"))
                        {
                            continue;
                        }
                        else
                        {
                            // 編成配置とランクの間にゴミ数値が入ることがあるので対応
                            if (lines[i + 1].Contains("ランク"))
                            {
                                continue;
                            }
                            else
                            {
                                int.TryParse(line.Trim(), out rank);
                                formationDto.Rank = rank;
                                continue;
                            }
                        }
                    }

                    //  |_戦闘力
                    if (0 > combatPower)
                    {
                        if (line.Contains("戦闘力"))
                        {
                            continue;
                        }
                        else
                        {
                            string combatPowerNum = System.Text.RegularExpressions.Regex.Replace(line, "[^0-9]", "");
                            long.TryParse(combatPowerNum, out combatPower);
                            formationDto.CombatPower = combatPower;
                        }
                    }

                    formationDtoList.Add(formationDto);
                    formationDto = new FormationDto();
                    playerName = string.Empty;
                    combatPower = -1;
                    rank = -1;
                    formationPosition = -1;
                    no++;
                    characterDtoList = new List<CharacterDto>();
                }

                mmgPlayOCRDto.Formations = formationDtoList.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("MMG形式への変換に失敗しました。", ex);
            }

            if (CheckBoxCorrection.IsChecked == true)
            {
                // Lvの値を補正する (キャラクターや進化状況によっては1～240までのレベルを考慮)
                foreach (var formation in mmgPlayOCRDto.Formations)
                {
                    formation.UpdateCharaLv();
                }
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(mmgPlayOCRDto);
        }

        /// <summary>
        /// ボタンをクリックした時にOCR種別によってOCRを実行しMMG-Formatに変換する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonAnalyze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonAnalyze.IsEnabled = false;

                if (CheckBoxCharacterMatchingView.IsChecked == true)
                {
                    isCharacterMatchingView = true;
                }
                else
                {
                    isCharacterMatchingView = false;
                }

                if (RadioButtonGoogleDriveAPI.IsChecked == true)
                {
                    InputCheck(OCRType.GoogleDriveAPI);
                    ProgressBar.Visibility = Visibility.Visible;
                    await Task.Run(() => GoogleDriveDocumentOcr());
                }
                else if (RadioButtonTesseract.IsChecked == true)
                {
                    InputCheck(OCRType.Tesseract);
                    ProgressBar.Visibility = Visibility.Visible;
                    await Task.Run(() => GoogleTesseractOCR(OCRType.Tesseract));
                }
                else if (RadioButtonTesseractOCR.IsChecked == true)
                {
                    InputCheck(OCRType.TesseractOCR);
                    ProgressBar.Visibility = Visibility.Visible;
                    await Task.Run(() => GoogleTesseractOCR(OCRType.TesseractOCR));
                }

                TextBoxOCR.Text = ocrText;

                if (CheckBoxAutoConversion.IsChecked == true)
                {
                    TextBoxMMGFormat.Text = CovertToMMGFormat(ocrText);
                }
            }
            catch (Google.GoogleApiException ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ButtonAnalyze.IsEnabled = true;
            }
        }

        /// <summary>
        /// 画像ファイルをダブルクリックしたときにファイル選択ダイアログを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxFilePath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "画像ファイル (*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png",
                    Title = "画像ファイルを選択してください"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    TextBoxFilePath.Text = openFileDialog.FileName;
                    imageFilePath = openFileDialog.FileName;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ImageInput.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        ///<summary>
        /// 画像を左クリックしたときに画像ウィンドウを表示
        ///</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageInput_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!isImageWindow)
                {
                    isImageWindow = true;
                    return;
                }

                ImageWindow imageWindow = new ImageWindow(imageFilePath)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                imageWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// GoogleDriveAPIを選択したときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonGoogleDriveAPI_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxTrainingModelFilePath.IsEnabled = false;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Tesseractを選択したときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonTesseract_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxTrainingModelFilePath.IsEnabled = true;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// TesseractOCRを選択したときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonTesseractOCR_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxTrainingModelFilePath.IsEnabled = true;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// tessdataのパスを選択するためのフォルダ選択ダイアログを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxTrainingModelFilePath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                using (var cofd = new CommonOpenFileDialog()
                {
                    Title = "フォルダを選択してください",
                    IsFolderPicker = true,
                    RestoreDirectory = true,
                })
                {
                    if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
                    {
                        return;
                    }

                    TextBoxTrainingModelFilePath.Text = cofd.FileName;
                    trainingModelPath = cofd.FileName;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// MMG形式に変換するボタンをクリックしたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonMMGFormatConversion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProgressBar.Visibility = Visibility.Visible;

                TextBoxMMGFormat.Text = CovertToMMGFormat(TextBoxOCR.Text);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// キャラクターマッチングテキストボックスをダブルクリックしたときにフォルダ選択ダイアログを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxCharacterMatchingFilePath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                using (var cofd = new CommonOpenFileDialog()
                {
                    Title = "フォルダを選択してください",
                    IsFolderPicker = true,
                    RestoreDirectory = true,
                })
                {
                    if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
                    {
                        return;
                    }

                    TextBoxCharacterMatchingFilePath.Text = cofd.FileName;
                    characterMatchingFilePath = cofd.FileName;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// キャラクターマッチングボタンをクリックしたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCharacterMatching_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProgressBar.Visibility = Visibility.Visible;

                InputCheck(OCRType.None);

                if (CheckBoxCharacterMatchingView.IsChecked == true)
                {
                    isCharacterMatchingView = true;
                }
                else
                {
                    isCharacterMatchingView = false;
                }

                LoadFormationCharacterNameList(TextBoxFilePath.Text);

                TextBoxMMGFormat.Text = Newtonsoft.Json.JsonConvert.SerializeObject(formationCharacterNameList);
                TextBoxMMGFormat.Text += Environment.NewLine + "-----------------------------" + Environment.NewLine;
                TextBoxMMGFormat.Text += Newtonsoft.Json.JsonConvert.SerializeObject(characterDic);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
                MessageBox.Show(ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }
    }
}
