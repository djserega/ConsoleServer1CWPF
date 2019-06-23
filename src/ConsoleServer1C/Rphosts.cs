using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C
{
    internal sealed class Rphosts
    {
        internal bool Get(string serverName, string[] connectionSettings)
        {
            bool result = false;

            try
            {
                if (CheckConnectionServer.CheckConnectionToServer(serverName))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(
                        "cmd",
                        "/c tasklist " + GetArgumentsStartInfo(serverName, connectionSettings))
                    {
                        StandardOutputEncoding = Encoding.GetEncoding("cp866"),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process processTaskList = new Process()
                    {
                        StartInfo = startInfo
                    };
                    processTaskList.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        var rphost = new Models.RphostObject();
                        if (rphost.Fill(e.Data))
                        {
                            Events.RphostEvents.Created(rphost);
                        }
                    };
                    processTaskList.Start();
                    processTaskList.BeginOutputReadLine();

                    processTaskList.WaitForExit(10 * 1000);

                    if (!processTaskList.HasExited)
                        processTaskList.Kill();

                    result = true;
                }
            }
            catch (ConnectAgentException ex)
            {
                Dialogs.Show($"Ошибка соединения с сервером.\n{ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Dialogs.Show($"Не удалось получить доступ к процессам сервера.\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Dialogs.Show($"Не предвиденная ошибка.\n{ex.Message}");
            }

            return result;
        }

        internal bool KillProcess(string serverName, string[] connectionSettings, int pid)
        {
            bool result = false;

            try
            {
                if (CheckConnectionServer.CheckConnectionToServer(serverName))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(
                        "cmd",
                        $"/c taskkill {GetArgumentsStartInfo(serverName, connectionSettings)} /pid {pid}");
                    Process processTaskList = new Process()
                    {
                        StartInfo = startInfo
                    };
                    processTaskList.Start();
                    processTaskList.WaitForExit(10 * 1000);

                    if (!processTaskList.HasExited)
                        processTaskList.Kill();

                    result = true;
                }
            }
            catch (ConnectAgentException ex)
            {
                Dialogs.Show($"Ошибка соединения с сервером.\n{ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Dialogs.Show($"Не удалось получить доступ к процессам сервера.\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Dialogs.Show($"Не предвиденная ошибка.\n{ex.Message}");
            }

            return result;
        }

        private string GetArgumentsStartInfo(string serverName, string[] connectionSettings)
        {
            StringBuilder builderArguments = new StringBuilder();
            builderArguments.Append("/s ");
            builderArguments.Append(serverName);
            builderArguments.Append(" ");
            builderArguments.Append(@"/fi ""imagename eq rphost.exe"" ");

            if (connectionSettings != null)
            {
                builderArguments.Append("/u ");
                builderArguments.Append(AppSettings.ConverterInValue(connectionSettings[0]));
                builderArguments.Append(" ");
                builderArguments.Append("/p ");
                builderArguments.Append(AppSettings.ConverterInValue(connectionSettings[1]));
                builderArguments.Append(" ");
            }

            return builderArguments.ToString();
        }
    }
}
