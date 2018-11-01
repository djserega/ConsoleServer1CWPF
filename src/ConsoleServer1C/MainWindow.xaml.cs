using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using timers = System.Timers;

namespace ConsoleServer1C
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Models.InfoBase _selectedItemListBases = new Models.InfoBase();
        private timers.Timer _timer = new timers.Timer();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Events.UpdateInfoMainWindowEvents.UpdateListBasesMainWindowEvent += () =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    RefreshDataContextListBase(Events.UpdateInfoMainWindowEvents.InfoBases);
                    DataGridListBases.Items.Refresh();
                    DataGridListSessions.Items.Refresh();
                    NotUpdating = true;
                    ButtonConnect.IsEnabled = NotUpdating;
                    StartStopAutoUpdating();
                }));
            };

            Events.ConnectionStatusEvents.ConnectionStatusEvent += () =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    ProgressBarValue = Events.ConnectionStatusEvents.CurrentStateProgress;
                    BindingOperations.GetBindingExpression(ProgressBarStatusConnection, ProgressBar.ValueProperty).UpdateTarget();
                    BindingOperations.GetBindingExpression(TextBlockStatusConnection, TextBlock.TextProperty).UpdateTarget();
                }));
            };

            _timer.Elapsed += (object sender, timers.ElapsedEventArgs e) =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate { UpdateListBases(true); }));
            };
        }

        public ObservableCollection<Models.InfoBase> ListBases { get; private set; } = new ObservableCollection<Models.InfoBase>();
        public Models.InfoBase SelectedItemListBases
        {
            get => _selectedItemListBases;
            set { _selectedItemListBases = value; DataGridListSessions.ItemsSource = _selectedItemListBases?.ListSessions; }
        }
        public AppSettings AppSettings { get; set; } = new AppSettings();
        public int ProgressBarValue { get; set; } = 0;
        public bool NotUpdating { get; private set; } = true;


        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            UpdateListBases();
        }

        private async void UpdateListBases(bool updateSessionInfo = false)
        {
            NotUpdating = false;
            ButtonConnect.IsEnabled = NotUpdating;
            StartStopAutoUpdating();

            if (!updateSessionInfo)
                ListBases.Clear();

            try
            {
                using (ConnectToAgent connectToAgent = new ConnectToAgent(AppSettings.ServerName))
                {
                    connectToAgent.FilterInfoBaseName = AppSettings.FilterInfoBaseName;
                    connectToAgent.UpdateSessions = updateSessionInfo;

                    connectToAgent.InfoBases.Clear();
                    foreach (Models.InfoBase item in ListBases)
                        connectToAgent.InfoBases.Add(item);

                    await connectToAgent.GetListBaseAsync();
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                Focus();
                TextBoxServerName.Focus();
            }
            catch (CreateV83ComConnector ex)
            {
                MessageBox.Show($"Не удалось создать COMConnector.\n{ex.Message}");
            }
            catch (ConnectAgentException ex)
            {
                MessageBox.Show($"Ошибка соединения с сервером.\n{ex.Message}");
            }
            catch (WorkingProcessException ex)
            {
                MessageBox.Show($"Ошибка соединения с рабочим процессом.\n{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshDataContextListBase(List<Models.InfoBase> newListBases)
        {
            List<Models.InfoBase> deletingRow = new List<Models.InfoBase>();
            foreach (Models.InfoBase itemRow in ListBases)
            {
                Models.InfoBase newInfoBase = newListBases.FirstOrDefault(f => f.NameToUpper == itemRow.NameToUpper);
                if (newInfoBase == null)
                    deletingRow.Add(itemRow);
                else
                {
                    itemRow.Fill(newInfoBase);
                    newListBases.Remove(newInfoBase);
                }
            }
            foreach (Models.InfoBase item in deletingRow)
                ListBases.Remove(item);

            foreach (Models.InfoBase item in newListBases)
                ListBases.Add(item);
        }

        private void TextBoxServerName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UpdateListBases();
        }

        private void TextBoxUpdateSessionMinute_TextChanged(object sender, TextChangedEventArgs e)
        {
            StartStopAutoUpdating();
        }

        private void StartStopAutoUpdating()
        {
            if (!NotUpdating || (AppSettings.UpdateSessionMinute == 0 || ListBases.Count == 0))
            {
                _timer.Stop();
                BorderUpdateSessionMinute.Background = new SolidColorBrush();
            }
            else
            {
                _timer.Interval = AppSettings.UpdateSessionMinute * 1000;
                _timer.Start();
                BorderUpdateSessionMinute.Background = (Brush)new BrushConverter().ConvertFrom("#C7DFFC");
            }
        }
    }
}
