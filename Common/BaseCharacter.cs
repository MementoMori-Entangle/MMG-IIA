using MMG_IIA.Dto;

namespace MMG_IIA.Common
{
    public class BaseCharacter
    {
        public static BaseCharacterDto[] GetBaseCharacterDtos()
        {
            BaseCharacterDto[] BaseCharacterDtos = new BaseCharacterDto[]
            {
                new BaseCharacterDto()
                {
                    Id = 1001,
                    Attribute = "藍",
                    Name = "ソルティーナ",
                    Speed = 3302,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id= 1002,
                    Attribute = "藍",
                    Name = "アムレート",
                    Speed = 2766,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1003,
                    Attribute = "藍",
                    Name = "フェンリル",
                    Speed = 2894,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1004,
                    Attribute = "藍",
                    Name = "フローレンス",
                    Speed = 3022,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1005,
                    Attribute = "藍",
                    Name = "ソーニャ",
                    Speed = 3376,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1006,
                    Attribute = "藍",
                    Name = "モーザ",
                    Speed = 2826,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1007,
                    Attribute = "藍",
                    Name = "シヴィ",
                    Speed = 2855,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1008,
                    Attribute = "藍",
                    Name = "ステラ",
                    Speed = 2698,
                    Rarity = 3,
                    SpeedEWBuff = new int[] { 25 }
                },
                new BaseCharacterDto()
                {
                    Id = 1009,
                    Attribute = "藍",
                    Name = "イリア",
                    Speed = 2733,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1010,
                    Attribute = "藍",
                    Name = "アイリス",
                    Speed = 3133,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1011,
                    Attribute = "藍",
                    Name = "ロキ",
                    Speed = 3066,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1012,
                    Attribute = "藍",
                    Name = "モニカ",
                    Speed = 2600,
                    Rarity = 0
                },
                new BaseCharacterDto()
                {
                    Id = 1013,
                    Attribute = "藍",
                    Name = "フェーネ",
                    Speed = 2853,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1014,
                    Attribute = "藍",
                    Name = "[聖夜の祈り]トロポン",
                    Speed = 3057,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1101,
                    Attribute = "赤",
                    Name = "サブリナ",
                    Speed = 2966,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1102,
                    Attribute = "赤",
                    Name = "フレイシア",
                    Speed = 2680,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1103,
                    Attribute = "赤",
                    Name = "アモール",
                    Speed = 2550,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1104,
                    Attribute = "赤",
                    Name = "リーン",
                    Speed = 2813,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1105,
                    Attribute = "赤",
                    Name = "ベル",
                    Speed = 3217,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1106,
                    Attribute = "赤",
                    Name = "ディアン",
                    Speed = 3080,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1107,
                    Attribute = "赤",
                    Name = "ソフィア",
                    Speed = 3080,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1108,
                    Attribute = "赤",
                    Name = "シフォン",
                    Speed = 2783,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1109,
                    Attribute = "赤",
                    Name = "アーティ",
                    Speed = 2734,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1110,
                    Attribute = "赤",
                    Name = "プリシラ",
                    Speed = 3167,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1111,
                    Attribute = "赤",
                    Name = "[夏の残響]コルディ",
                    Speed = 3575,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1112,
                    Attribute = "赤",
                    Name = "アリアンロッド",
                    Speed = 2921,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1113,
                    Attribute = "赤",
                    Name = "テオドラ",
                    Speed = 2866,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1114,
                    Attribute = "赤",
                    Name = "ペトラ",
                    Speed = 3166,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1115,
                    Attribute = "赤",
                    Name = "シャーロット",
                    Speed = 2716,
                    Rarity = 0
                },
                new BaseCharacterDto()
                {
                    Id = 1201,
                    Attribute = "翠",
                    Name = "アイビー",
                    Speed = 2790,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1202,
                    Attribute = "翠",
                    Name = "マーリン",
                    Speed = 2888,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1203,
                    Attribute = "翠",
                    Name = "コルディ",
                    Speed = 3562,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1204,
                    Attribute = "翠",
                    Name = "ニーナ",
                    Speed = 2839,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1205,
                    Attribute = "翠",
                    Name = "メルティーユ",
                    Speed = 2796,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1206,
                    Attribute = "翠",
                    Name = "ルーク",
                    Speed = 3093,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1207,
                    Attribute = "翠",
                    Name = "レア",
                    Speed = 3571,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1208,
                    Attribute = "翠",
                    Name = "フィアー",
                    Speed = 2645,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1209,
                    Attribute = "翠",
                    Name = "[涼風の軍神]サブリナ",
                    Speed = 3418,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1210,
                    Attribute = "翠",
                    Name = "ザラ",
                    Speed = 2766,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1211,
                    Attribute = "翠",
                    Name = "ロザリー",
                    Speed = 2800,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1212,
                    Attribute = "翠",
                    Name = "リブラ",
                    Speed = 3066,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1213,
                    Attribute = "翠",
                    Name = "シズ",
                    Speed = 2520,
                    Rarity = 0
                },
                new BaseCharacterDto()
                {
                    Id = 1301,
                    Attribute = "黄",
                    Name = "ミミ",
                    Speed = 3328,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1302,
                    Attribute = "黄",
                    Name = "トロポン",
                    Speed = 2657,
                    Rarity = 3,
                    SpeedSkillBuff = new int[] { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
                                                 20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
                                                 20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
                                                 20, 20, 20, 20, 20, 20, 20, 20, 20, 20
                                               }
                },
                new BaseCharacterDto()
                {
                    Id = 1303,
                    Attribute = "黄",
                    Name = "ハトホル",
                    Speed = 3334,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1304,
                    Attribute = "黄",
                    Name = "オリヴィエ",
                    Speed = 3120,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1305,
                    Attribute = "黄",
                    Name = "プリマヴェーラ",
                    Speed = 3268,
                    Rarity = 3,
                    SpeedSkillBuff = new int[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
                                                 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
                                                 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
                                                 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
                                               }
                },
                new BaseCharacterDto()
                {
                    Id = 1306,
                    Attribute = "黄",
                    Name = "カロル",
                    Speed = 3452,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1307,
                    Attribute = "黄",
                    Name = "ウィーラ",
                    Speed = 3654,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1308,
                    Attribute = "黄",
                    Name = "リシェス",
                    Speed = 3596,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1309,
                    Attribute = "黄",
                    Name = "[墓守の夏休み]モーザ",
                    Speed = 3245,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1310,
                    Attribute = "黄",
                    Name = "スクルド",
                    Speed = 2590,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1311,
                    Attribute = "黄",
                    Name = "チェルナ",
                    Speed = 2809,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1312,
                    Attribute = "黄",
                    Name = "ソテイラ",
                    Speed = 3019,
                    Rarity = 1
                },
                new BaseCharacterDto()
                {
                    Id = 1313,
                    Attribute = "黄",
                    Name = "ガルム",
                    Speed = 2880,
                    Rarity = 0
                },
                new BaseCharacterDto()
                {
                    Id = 1314,
                    Attribute = "黄",
                    Name = "[聖夜の贈り物]アモール",
                    Speed = 3450,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1315,
                    Attribute = "黄",
                    Name = "[黒鎧の従者]アイリス",
                    Speed = 2933,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1401,
                    Attribute = "天",
                    Name = "[弔花の魔女]ナターシャ",
                    Speed = 2926,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1402,
                    Attribute = "天",
                    Name = "[聖剣の魔女]フォルティナ",
                    Speed = 3059,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1403,
                    Attribute = "天",
                    Name = "[雷啼の魔女]ケロベロス",
                    Speed = 3363,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1404,
                    Attribute = "天",
                    Name = "[天泣の魔女]ルサールカ",
                    Speed = 2972,
                    Rarity = 3,
                    SpeedEWBuff = new int[] { 30 }
                },
                new BaseCharacterDto()
                {
                    Id = 1405,
                    Attribute = "天",
                    Name = "[聖槍の魔女]エルフリンデ",
                    Speed = 3049,
                    Rarity = 3,
                    SpeedEWBuff = new int[] { 30 }
                },
                new BaseCharacterDto()
                {
                    Id = 1501,
                    Attribute = "冥",
                    Name = "[雪幻の魔女]ルナリンド",
                    Speed = 3304,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1502,
                    Attribute = "冥",
                    Name = "[却火の魔女]ヴァルリーデ",
                    Speed = 2883,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1503,
                    Attribute = "冥",
                    Name = "[錆鉄の魔女]A.A.",
                    Speed = 2687,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1504,
                    Attribute = "冥",
                    Name = "[骸晶の魔女]オフィーリア",
                    Speed = 3007,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1505,
                    Attribute = "冥",
                    Name = "[英霊の魔女]アームストロング",
                    Speed = 3047,
                    Rarity = 3
                },
                new BaseCharacterDto()
                {
                    Id = 1506,
                    Attribute = "冥",
                    Name = "[神呪の魔女]イリア",
                    Speed = 2703,
                    Rarity = 3
                }
            };

            return BaseCharacterDtos;
        }
    }
}
