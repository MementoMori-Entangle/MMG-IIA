using System;
using System.Collections.Generic;

namespace MMG_IIA.Dto
{
    public class FormationDto : Dto
    {
        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;
        /// <summary>
        /// キャラクター情報配列
        /// </summary>
        public CharacterDto[] CharacterDtos { get; set; } = Array.Empty<CharacterDto>();
        /// <summary>
        /// 編成位置
        /// </summary>
        public int FormationPosition { get; set; } = 0;
        /// <summary>
        /// ランク
        /// </summary>
        public int Rank { get; set; } = 0;
        /// <summary>
        /// 戦闘力
        /// </summary>
        public long CombatPower { get; set; } = 0;

        /// <summary>
        /// キャラクターLvを更新する
        /// </summary>
        public void UpdateCharaLv()
        {
            // lvListのLvで同じ物が一番多いものを取得する
            Dictionary<int, int> lvCount = new Dictionary<int, int>();

            foreach (var characterDto in CharacterDtos)
            {
                if (lvCount.ContainsKey(characterDto.Level))
                {
                    lvCount[characterDto.Level]++;
                }
                else
                {
                    lvCount[characterDto.Level] = 1;
                }
            }

            int maxCount = 0;
            int maxLv = 0;

            foreach (var kvp in lvCount)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    maxLv = kvp.Key;
                }
            }

            // 一番多いLvを取得できたら、lvListのLvを全てそのLvにする
            if (maxCount > 0)
            {
                foreach (var characterDto in CharacterDtos)
                {
                    characterDto.Level = maxLv;
                }
            }
        }
    }
}
