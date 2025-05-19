using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MMG_IIA
{
    /// <summary>
    /// ImageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageWindow : Window
    {
        private readonly ViewModel vm; // ViewModelのインスタンス

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filePath">画像ファイルのパス</param>
        public ImageWindow(string filePath = "")
        {
            InitializeComponent();

            vm = new ViewModel();
            DataContext = vm;

            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    using (var ms = new MemoryStream(File.ReadAllBytes(filePath)))
                    {
                        vm.Img = new WriteableBitmap(BitmapFrame.Create(ms));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ウィンドウのサイズ変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement frameworkElement = Content as FrameworkElement;
            vm.ImgHeight = frameworkElement.ActualHeight;
            vm.ImgWidth = frameworkElement.ActualWidth;
        }
    }
}
