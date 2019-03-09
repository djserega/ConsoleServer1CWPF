namespace ConsoleServer1C.Converters
{
    /// <summary>
    /// Конвертеры байт в строку (B, KB, MB, GB, TB, PB, EB)
    /// </summary>
    internal class DateConverters
    {
        /// <summary>
        /// Конвертер с ulong в string
        /// </summary>
        /// <param name="bytes">Количество байт</param>
        /// <returns></returns>
        internal static string BytesToString(ulong bytes)
        {
            if (bytes == 0)
                return "";

            else if (bytes < 1024)
                return (bytes).ToString("F0") + " B";

            else if ((bytes >> 10) < 1024)
                return (bytes / (float)1024).ToString("F1") + " KB";

            else if ((bytes >> 20) < 1024)
                return ((bytes >> 10) / (float)1024).ToString("F1") + " MB";

            else if ((bytes >> 30) < 1024)
                return ((bytes >> 20) / (float)1024).ToString("F1") + " GB";

            else if ((bytes >> 40) < 1024)
                return ((bytes >> 30) / (float)1024).ToString("F1") + " TB";

            else if ((bytes >> 50) < 1024)
                return ((bytes >> 40) / (float)1024).ToString("F1") + " PB";

            else
                return ((bytes >> 50) / (float)1024).ToString("F0") + " EB";
        }

        /// <summary>
        /// Конвертер с long в string
        /// </summary>
        /// <param name="bytes">Количество байт</param>
        /// <returns></returns>
        internal static string BytesToString(long bytes)
        {
            if (bytes == 0)
                return "";

            if (bytes > 0)
            {
                if (bytes < 1024)
                    return (bytes).ToString("F0") + " B";

                else if ((bytes >> 10) < 1024)
                    return (bytes / (float)1024).ToString("F1") + " KB";

                else if ((bytes >> 20) < 1024)
                    return ((bytes >> 10) / (float)1024).ToString("F1") + " MB";

                else if ((bytes >> 30) < 1024)
                    return ((bytes >> 20) / (float)1024).ToString("F1") + " GB";

                else if ((bytes >> 40) < 1024)
                    return ((bytes >> 30) / (float)1024).ToString("F1") + " TB";

                else if ((bytes >> 50) < 1024)
                    return ((bytes >> 40) / (float)1024).ToString("F1") + " PB";

                else
                    return ((bytes >> 50) / (float)1024).ToString("F0") + " EB";
            }
            else
            {
                if (bytes > -1024)
                    return (bytes).ToString("F0") + " B";

                else if ((bytes >> 10) > -1024)
                    return (bytes / (float)1024).ToString("F1") + " KB";

                else if ((bytes >> 20) > -1024)
                    return ((bytes >> 10) / (float)1024).ToString("F1") + " MB";

                else if ((bytes >> 30) > -1024)
                    return ((bytes >> 20) / (float)1024).ToString("F1") + " GB";

                else if ((bytes >> 40) > -1024)
                    return ((bytes >> 30) / (float)1024).ToString("F1") + " TB";

                else if ((bytes >> 50) > -1024)
                    return ((bytes >> 40) / (float)1024).ToString("F1") + " PB";

                else
                    return ((bytes >> 50) / (float)1024).ToString("F0") + " EB";

            }
        }
    }
}
