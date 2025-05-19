using System;

namespace MMG_IIA.Dto
{
    [Serializable]
    public class CharacterDto : Dto
    {
        /// <summary>
        /// キャラクター名
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// レベル
        /// </summary>
        public int Level { get; set; } = 0;
    }
}
