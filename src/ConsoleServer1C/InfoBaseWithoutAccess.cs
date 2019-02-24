using System.Collections.Generic;

namespace ConsoleServer1C
{
    public static class InfoBaseWithoutAccess
    {
        private static readonly object _lockerAdd = new object();
        private static readonly object _lockerContains = new object();

        public static List<string> InfoBasesName { get; set; } = new List<string>();

        public static void AddInfoBase(string infoBaseName)
        {
            lock (_lockerAdd)
            {
                if (!InfoBaseContains(infoBaseName))
                    InfoBasesName.Add(infoBaseName);
            }
        }

        public static bool InfoBaseContains(string infoBaseName)
        {
            lock (_lockerContains)
            {
                return InfoBasesName.Contains(infoBaseName);
            }
        }
    }
}
