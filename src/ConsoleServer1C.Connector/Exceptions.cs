using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C.Connector
{
    public class CreateV83ComConnector : Exception
    {
        public CreateV83ComConnector(string message) : base(message) { }
    }

    public class ConnectAgentException : Exception
    {
        public ConnectAgentException(string message) : base(message) { }
    }

    public class WorkingProcessException : Exception
    {
        public WorkingProcessException(string message) : base(message) { }
    }
    public class TerminateSessionException : Exception
    {
        public TerminateSessionException(string message) : base(message) { }
    }
}
