using System.ComponentModel;
using System.Runtime.CompilerServices;

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
