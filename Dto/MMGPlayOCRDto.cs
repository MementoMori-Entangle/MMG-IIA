using System;

namespace MMG_IIA.Dto
{
    [Serializable]
    public class MMGPlayOCRDto : Dto
    {
        /// <summary>
        /// 陣地名
        /// </summary>
        public string BaseName { get; set; } = string.Empty;
        /// <summary>
        /// 合計パーティ数
        /// </summary>
        public int SumPartyNum { get; set; } = 0;
        /// <summary>
        /// 編成情報配列
        /// </summary>
        public FormationDto[] Formations { get; set; } = Array.Empty<FormationDto>();
    }
}
