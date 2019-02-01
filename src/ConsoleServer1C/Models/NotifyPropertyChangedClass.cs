using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C.Models
{
    public class NotifyPropertyChangedClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "FindBase")
                Events.ChangeFilterEvents.InvokeFindBaseEvent();
            else if (propertyName == "FindUser")
                Events.ChangeFilterEvents.InvokeFindUserEvent();
        }
    }
}
