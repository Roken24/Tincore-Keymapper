using System;

namespace Unity.Messenger
{
    public partial class Utils
    {
        public static string ReadableSize(int size)
        {
            if (size < 1024)
            {
                return $"{size}B";
            }
            if (size < 1024 * 1024)
            {
                return $"{Math.Round(size / 1024f, 2)}K";
            }

            if (size < 1024 * 1024 * 1024)
            {
                return $"{Math.Round(size / 1024f / 1024f, 2)}M";
            }

            return $"{Math.Round(size / 1024f / 1024f / 1024f, 2)}G";
        }
    }
}