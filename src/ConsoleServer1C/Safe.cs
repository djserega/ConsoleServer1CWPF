using System;
using System.Diagnostics;

namespace ConsoleServer1C
{
    internal class Safe
    {
        internal static void SafeAction(Action action, string description = null)
        {
            description = description ?? "Перехвачена ошибка выполнения.\nДетальную информацию можно найти в событиях Windows.";

            try
            {
                action();
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry(
                        ex.Message
                        + "\n" + "\n" +
                        ex.InnerException?.Message
                        + "\n" + "\n" +
                        ex.InnerException?.InnerException?.Message
                        + "\n" + "\n" +
                        ex.InnerException?.InnerException?.InnerException?.Message,
                        EventLogEntryType.Warning);
                }
                Dialogs.Show(description);
            }
        }
    }
}
