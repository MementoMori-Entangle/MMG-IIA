using System;

namespace MMG_IIA.Dto
{
    [Serializable]
    public class BaseCharacterDto : Dto
    {
        /// <summary>
        ///　キャラクターのID
        /// </summary>
        public int Id { get; set; } = 0;
        /// <summary>
        /// キャラクターの属性
        /// </summary>
        public string Attribute { get; set; } = string.Empty;
        /// <summary>
        /// キャラクターの名前
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// キャラクターのスピード
        /// </summary>
        public int Speed { get; set; } = 0;
        /// <summary>
        /// キャラクターのレアリティ
        /// </summary>
        public int Rarity { get; set; } = 0;
        /// <summary>
        /// キャラクターのスキルスピードバフ
        /// </summary>
        public int[] SpeedSkillBuff { get; set; }
        /// <summary>
        ///　キャラクターの専用武器スピードバフ
        /// </summary>
        public int[] SpeedEWBuff { get; set; }
    }
}
