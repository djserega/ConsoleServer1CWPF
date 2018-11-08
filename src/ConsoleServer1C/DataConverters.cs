using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C
{
    internal class DataConverters
    {
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
