using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConsoleServer1C.Models
{
    /// <summary>
    /// Базовый класс оповещения о изменениях в свойствах
    /// </summary>
    public class NotifyPropertyChangedClass : INotifyPropertyChanged
    {
        /// <summary>
        /// Базовое событие
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Метод оповещения изменений
        /// </summary>
        /// <param name="propertyName"></param>
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
