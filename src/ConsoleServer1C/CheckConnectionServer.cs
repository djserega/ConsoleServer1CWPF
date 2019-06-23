using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C
{
    internal static class CheckConnectionServer
    {
        /// <summary>
        /// Проверка наличия соединения к серверу 1С
        /// </summary>
        /// <param name="server">Сервер проверки подключения</param>
        /// <param name="throwException">Нужно ли выбросить исключение при не удачно проверке подключения</param>
        /// <returns>true: доступ есть</returns>
        internal static bool CheckConnectionToServer(string server, bool throwException = true)
        {
            bool result = false;
            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply reply = ping.Send(server, 5);
                    result = reply.Status == IPStatus.Success;
                }
                catch (PingException)
                {
                }

            }
            if (!result
                && throwException)
            {
                throw new ConnectAgentException("Сервер не доступен.");
            }

            return result;
        }

    }
}
