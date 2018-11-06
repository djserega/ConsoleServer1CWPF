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
        private bool _formIsWidenSizeWE = false;
        private bool _formIsWidenSizeNS = false;

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
                    UpdateBindingTarget(ProgressBarStatusConnection, ProgressBar.ValueProperty);
                    UpdateBindingTarget(TextBlockStatusConnection, TextBlock.TextProperty);
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
            set { _selectedItemListBases = value; SetItemSourceListSession(); }
        }
        public Models.Session SelectedItemSession { get; set; }

        private void SetItemSourceListSession()
        {
            DataGridListSessions.ItemsSource = _selectedItemListBases?.ListSessions;
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
                if (!_formIsWidenSizeWE && !_formIsWidenSizeNS)
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

            SortListBasesToDbProcTook();
        }

        private void SortListBasesToDbProcTook()
        {
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                if (AppSettings.SortDbProcTook)
                {
                    ListBases = new ObservableCollection<Models.InfoBase>(ListBases.OrderBy(f => -f.DbProcTook));
                    UpdateBindingTarget(DataGridListBases, DataGrid.ItemsSourceProperty);
                    for (int i = 0; i < ListBases.Count; i++)
                    {
                        ListBases[i].ListSessions = new List<Models.Session>(ListBases[i].ListSessions.OrderBy(f => -f.DbProcTook));
                    }
                    SetItemSourceListSession();
                    UpdateBindingTarget(DataGridListSessions, DataGrid.ItemsSourceProperty);
                }
                else
                {
                    SetItemSourceListSession();
                    UpdateBindingTarget(DataGridListBases, DataGrid.ItemsSourceProperty);
                }
            }));
        }

        private void TextBoxServerName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UpdateListBases();
        }

        private void TextBoxFilterInfoBaseName_KeyDown(object sender, KeyEventArgs e)
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

        #region Change size main window

        private void RectangleSizeWE_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _formIsWidenSizeWE = true;
        }

        private void RectangleSizeWE_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _formIsWidenSizeWE = false;
            ((Rectangle)sender).ReleaseMouseCapture();
        }

        private void RectangleSizeWE_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                _formIsWidenSizeWE = false;

            if (_formIsWidenSizeWE)
            {
                ((Rectangle)sender).CaptureMouse();

                double newWidth = e.GetPosition(this).X + 1;
                if (newWidth > 0)
                    Width = newWidth;
            }
        }

        private void RectangleSizeNS_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _formIsWidenSizeNS = true;
        }

        private void RectangleSizeNS_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _formIsWidenSizeNS = false;
            ((Rectangle)sender).ReleaseMouseCapture();
        }

        private void RectangleSizeNS_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                _formIsWidenSizeNS = false;

            if (_formIsWidenSizeNS)
            {
                ((Rectangle)sender).CaptureMouse();

                double newHeight = e.GetPosition(this).Y + 1;
                if (newHeight > 0)
                    Height = newHeight;
            }
        }

        private void RectangleSizeNWSE_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _formIsWidenSizeNS = true;
            _formIsWidenSizeWE = true;
        }

        private void RectangleSizeNWSE_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _formIsWidenSizeNS = false;
            _formIsWidenSizeWE = false;
            ((Rectangle)sender).ReleaseMouseCapture();
        }

        private void RectangleSizeNWSE_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _formIsWidenSizeNS = false;
                _formIsWidenSizeWE = false;
            }

            if (_formIsWidenSizeNS && _formIsWidenSizeWE)
            {
                ((Rectangle)sender).CaptureMouse();

                double newHeight = e.GetPosition(this).Y + 1;
                if (newHeight > 0)
                    Height = newHeight;

                double newWidth = e.GetPosition(this).X + 1;
                if (newWidth > 0)
                    Width = newWidth;
            }
        }

        #endregion

        private static void UpdateBindingTarget(DependencyObject target, DependencyProperty dp)
        {
            try
            {
                BindingOperations.GetBindingExpression(target, dp).UpdateTarget();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void MenuItemSessionTerminateSession_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItemSession != null)
            {
                try
                {
                    new ConnectToAgent(AppSettings.ServerName).TerminateSession(SelectedItemSession);
                }
                catch (TerminateSessionException ex)
                {
                    MessageBox.Show("Не удалось отключить сессию.\n" + ex.Message);
                }
            }
        }
    }
}
