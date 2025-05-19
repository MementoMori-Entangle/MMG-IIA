using System;

namespace MMG_IIA.Dto
{
    [Serializable]
    public class Dto
    {
    }

    public static class CopyHelper
    {
        /// <summary>
        /// 深いコピーを行う
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(this T src)
        {
            ReadOnlySpan<byte> b = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes<T>(src);
            return System.Text.Json.JsonSerializer.Deserialize<T>(b);
        }
    }
}
