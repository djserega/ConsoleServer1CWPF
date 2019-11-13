using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ConsoleServer1C
{
    /// <summary>
    /// Логика взаимодействия для Rphosts1CServer.xaml
    /// </summary>
    public partial class Rphosts1CServer : Window
    {
        private string _serverName;
        private string[] _connectSettings;

        public Rphosts1CServer(string serverName, string[] connectSettings)
        {
            _serverName = serverName;
            _connectSettings = connectSettings;

            InitializeComponent();

            ListProcesses = new ObservableCollection<Models.RphostObject>();

            DataContext = this;

            Events.RphostEvents.NewObject += (Models.RphostObject rphost) =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    ListProcesses.Add(rphost);

                    ListProcesses = new ObservableCollection<Models.RphostObject>(ListProcesses.OrderByDescending(f => f.Size));

                    DataGridListProcesses.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateSource();
                }));
            };
        }

        public ObservableCollection<Models.RphostObject> ListProcesses
        {
            get { return (ObservableCollection<Models.RphostObject>)GetValue(ListProcessesProperty); }
            set { SetValue(ListProcessesProperty, value); }
        }

        public static readonly DependencyProperty ListProcessesProperty =
            DependencyProperty.Register(
                "ListProcesses",
                typeof(ObservableCollection<Models.RphostObject>),
                typeof(Rphosts1CServer));

        public Models.RphostObject ListProcessesSelectedItem
        {
            get { return (Models.RphostObject)GetValue(ListProcessesSelectedItemProperty); }
            set { SetValue(ListProcessesSelectedItemProperty, value); }
        }

        public static readonly DependencyProperty ListProcessesSelectedItemProperty =
            DependencyProperty.Register(
                "ListProcessesSelectedItem",
                typeof(Models.RphostObject),
                typeof(Rphosts1CServer));

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
            else if (e.Key == Key.F5)
                LoadProcesses();
        }

        private void ButtonUpdateListProcesses_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        internal bool LoadProcesses()
        {
            ListProcesses.Clear();

            bool result = new Rphosts().Get(_serverName, _connectSettings);
            
            return result;
        }

        private void MenuItemKillProcessRphost_Click(object sender, RoutedEventArgs e)
        {
            if (ListProcessesSelectedItem != null)
                if (new Rphosts().KillProcess(_serverName, _connectSettings, ListProcessesSelectedItem.PID))
                    LoadProcesses();
        }

    }
}
