using ConsoleServer1C.Connector;
using ConsoleServer1C.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using timers = System.Timers;

namespace ConsoleServer1C
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private fields

        private Connector.Models.InfoBase _selectedItemListBases = new Connector.Models.InfoBase();
        private readonly timers.Timer _timer = new timers.Timer();
        private bool _formIsWidenSizeWE = false;
        private bool _formIsWidenSizeNS = false;
        private bool _refreshData = false;

        private string[] _connectSettings;

        private bool _maximized;
        private double _lastLeft;
        private double _lastTop;
        private double _lastWidth;
        private double _lastHeight;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            NotUpdating = true;

            DataContext = this;

            #region Events

            Connector.Events.UpdateInfoMainWindowEvents.UpdateListBasesMainWindowEvent += (bool updateSessions) =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    RefreshDataContextListBase(Connector.Events.UpdateInfoMainWindowEvents.InfoBases);
                    if (!updateSessions)
                        ListBasesNotFiltered = ListBases.ToList();
                    NotUpdating = true;
                    StartStopAutoUpdating();
                }));
            };

            Connector.Events.ConnectionStatusEvents.ConnectionStatusEvent += () =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    ProgressBarValue = Connector.Events.ConnectionStatusEvents.CurrentStateProgress;
                    UpdateBindingTarget(ProgressBarStatusConnection, ProgressBar.ValueProperty);
                    UpdateBindingTarget(TextBlockStatusConnection, TextBlock.TextProperty);
                }));
            };

            _timer.Elapsed += (object sender, timers.ElapsedEventArgs e) =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate { UpdateListBases(true); }));
            };

            TaskbarIcon.Events.TaskbarIconEvents.TaskbarIconEvent += (string title, string message) =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    if (AppSettings.NotifyWhenBlockingTimeDBIsExceeded && !_refreshData)
                        TaskbarIcon.Icon.ShowBalloonTip(title, message);

                    _refreshData = false;
                }));
            };

            Settings.Events.ChangeFilterEvents.ChangeFilterFindBaseEvent += () => { Dispatcher.Invoke(new ThreadStart(delegate { ApplyFilterListBase(); })); };
            Settings.Events.ChangeFilterEvents.ChangeFilterFindUserEvent += () => { Dispatcher.Invoke(new ThreadStart(delegate { ApplyFilterListUser(); })); };

            #endregion

            TaskbarIcon.Icon.SetIconSource(new BitmapImage(new Uri("pack://application:,,,/Консоль сервера 1С;component/" + "icon.ico")));
            TaskbarIcon.Icon.SetToolTipText("Консоль сервера 1С");

            InitializeTaskbarIcon();

            Topmost = AppSettings.IsTopmost;
        }

        #region Public properties

        /// <summary>
        /// Список полученного списка баз данных (с учетом пользовательского фильтра)
        /// </summary>
        public ObservableCollection<Connector.Models.InfoBase> ListBases { get; private set; } = new ObservableCollection<Connector.Models.InfoBase>();
        /// <summary>
        /// Список полученного списка баз данных 
        /// </summary>
        public List<Connector.Models.InfoBase> ListBasesNotFiltered { get; private set; } = new List<Connector.Models.InfoBase>();
        /// <summary>
        /// Выделенный объект базы данных
        /// </summary>
        public Connector.Models.InfoBase SelectedItemListBases
        {
            get => _selectedItemListBases;
            set { _selectedItemListBases = value; SetItemSourceListSession(); }
        }
        /// <summary>
        /// Выделенный объект сессии
        /// </summary>
        public Connector.Models.Session SelectedItemSession { get; set; }

        private void SetItemSourceListSession()
        {
            DataGridListSessions.ItemsSource = _selectedItemListBases?.ListSessions;
            RefreshUI();
        }

        public AppSettings AppSettings { get; set; } = new AppSettings();
        public int ProgressBarValue { get; set; } = 0;
        public int CountElementsListBases { get => ListBases.Count; }
        public int CountElementsListSession { get => SelectedItemListBases?.SessionCount ?? 0; }



        public bool NotUpdating
        {
            get { return (bool)GetValue(NotUpdatingProperty); }
            set { SetValue(NotUpdatingProperty, value); }
        }

        public static readonly DependencyProperty NotUpdatingProperty =
            DependencyProperty.Register(
                "NotUpdating",
                typeof(bool),
                typeof(MainWindow));



        #endregion

        #region WindowMain event

        private void WindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<object, bool> keyVisible in AppSettings.VisibilityDataGridSessionColumn)
                ChangeVisibleColumnDataGridSession(keyVisible.Key, keyVisible.Value);

            foreach (DictionaryEntry itemResource in DataGridListSessions.Resources)
            {
                if (itemResource.Value is ContextMenu contextMenu)
                {
                    if (contextMenu.Name.Equals("DataGridSessionContextMenuHeader"))
                    {
                        contextMenu.ItemsSource = GetMenuItemContextMenuDataGridSessionColumnHeader();
                        break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(AppSettings.ServerName)
                && !string.IsNullOrWhiteSpace(AppSettings.FilterInfoBaseName))
                UpdateListBases();
        }

        private void WindowMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettings.VisibilityDataGridSessionColumn.Clear();
            foreach (DataGridColumn column in DataGridListSessions.Columns)
                AppSettings.VisibilityDataGridSessionColumn.Add(column.Header, column.Visibility == Visibility.Visible);
        }

        private void WindowMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                if (!_formIsWidenSizeWE && !_formIsWidenSizeNS)
                    DragMove();
        }

        #endregion

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

        #region Button event

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (_maximized)
            {
                Width = _lastWidth;
                Height = _lastHeight;
                Left = _lastLeft;
                Top = _lastTop;

                _maximized = false;
            }
            else
            {
                _lastWidth = Width;
                _lastHeight = Height;
                _lastLeft = Left;
                _lastTop = Top;

                Rect workArea = SystemParameters.WorkArea;

                Width = workArea.Width;
                Height = workArea.Height;
                Left = workArea.Left;
                Top = workArea.Top;

                _maximized = true;
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            UpdateListBases();
            AddCurrentConnectionToHistory();
        }

        private void ButtonRefreshData_Click(object sender, RoutedEventArgs e)
        {
            _refreshData = true;
            UpdateListBases(true);
        }

        private void ButtonHistory_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = ((Button)sender).ContextMenu;
            contextMenu.DataContext = null;
            contextMenu.ItemsSource = null;
            contextMenu.DataContext = AppSettings.ListHistoryConnection;
            contextMenu.ItemsSource = AppSettings.ListHistoryConnection;
            contextMenu.IsOpen = true;
        }

        private void ButtonClearAppSettingsFindUser(object sender, RoutedEventArgs e)
        {
            AppSettings.FindUser = string.Empty;
        }

        private void ButtonClearAppSettingsFindBase(object sender, RoutedEventArgs e)
        {
            AppSettings.FindBase = string.Empty;
        }

        #endregion

        #region TextBox connected server1C

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

        #endregion

        #region History

        private void MenuItemSelectedHistory(object sender, RoutedEventArgs e)
        {
            Settings.Models.HistoryConnection elementHistory = AppSettings.ListHistoryConnection.FirstOrDefault(f => f.Date == (DateTime)((MenuItem)sender).Tag);

            if (elementHistory != null)
            {
                AppSettings.ServerName = elementHistory.Server;
                AppSettings.FilterInfoBaseName = elementHistory.FilterBase;

                BindingOperations.GetBindingExpression(TextBoxServerName, TextBox.TextProperty).UpdateTarget();
                BindingOperations.GetBindingExpression(TextBoxFilterInfoBaseName, TextBox.TextProperty).UpdateTarget();
            };
        }

        /// <summary>
        /// Добавление текущего подключения в список истории
        /// </summary>
        private void AddCurrentConnectionToHistory()
        {
            Settings.Models.HistoryConnection elementHistory = AppSettings.ListHistoryConnection.FirstOrDefault(
                f => f.Server == AppSettings.ServerName && f.FilterBase == AppSettings.FilterInfoBaseName);
            if (elementHistory != null)
                AppSettings.ListHistoryConnection.Remove(elementHistory);

            AppSettings.ListHistoryConnection.Insert(
                0,
                new Settings.Models.HistoryConnection(AppSettings.ServerName, AppSettings.FilterInfoBaseName, true));
        }

        #endregion

        #region Updating list bases

        /// <summary>
        /// Обновление списка базы данных
        /// </summary>
        /// <param name="updateSessionInfo"></param>
        private async void UpdateListBases(bool updateSessionInfo = false)
        {
            NotUpdating = false;
            StartStopAutoUpdating();

            if (!updateSessionInfo)
            {
                ListBases.Clear();
                ListBasesNotFiltered.Clear();
            }

            try
            {
                using (ConnectToAgent connectToAgent = new ConnectToAgent(AppSettings.ServerName))
                {
                    connectToAgent.FilterInfoBaseName = AppSettings.FilterInfoBaseName;
                    connectToAgent.UpdateSessions = updateSessionInfo;

                    connectToAgent.InfoBases.Clear();
                    foreach (Connector.Models.InfoBase item in ListBases)
                        connectToAgent.InfoBases.Add(item);

                    await connectToAgent.GetListBaseAsync();
                }
            }
            catch (ArgumentException ex)
            {
                Dialogs.Show(ex.Message);
                Focus();
                TextBoxServerName.Focus();
            }
            catch (CreateV83ComConnector ex)
            {
                Dialogs.Show($"Не удалось создать COMConnector.\n{ex.Message}");
            }
            catch (ConnectAgentException ex)
            {
                Dialogs.Show($"Ошибка соединения с сервером.\n{ex.Message}");
            }
            catch (WorkingProcessException ex)
            {
                Dialogs.Show($"Ошибка соединения с рабочим процессом.\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Dialogs.Show(ex.Message);
            }

            ApplyFilterListBase();
            ApplyFilterListUser();
        }

        /// <summary>
        /// Обновление текущего списка баз данных по новым данным обновления
        /// </summary>
        /// <param name="newListBases"></param>
        private void RefreshDataContextListBase(List<Connector.Models.InfoBase> newListBases)
        {
            List<Connector.Models.InfoBase> deletingRow = new List<Connector.Models.InfoBase>();
            foreach (Connector.Models.InfoBase itemRow in ListBases)
            {
                Connector.Models.InfoBase newInfoBase = newListBases.FirstOrDefault(f => f.NameToUpper == itemRow.NameToUpper);
                if (newInfoBase == null)
                    deletingRow.Add(itemRow);
                else
                {
                    itemRow.Fill(newInfoBase);
                    newListBases.Remove(newInfoBase);
                }
            }
            foreach (Connector.Models.InfoBase item in deletingRow)
                ListBases.Remove(item);

            foreach (Connector.Models.InfoBase item in newListBases)
                ListBases.Add(item);

            SortListBasesToDbProcTook();

            RefreshUI();
        }

        /// <summary>
        /// Обновление UI 
        /// </summary>
        private void RefreshUI()
        {
            UpdateBindingTarget(DataGridListBases, DataGrid.ItemsSourceProperty);
            UpdateBindingTarget(LabelCountElementsListBases, Label.ContentProperty);
            UpdateBindingTarget(LabelCountElementsListSession, Label.ContentProperty);
        }

        private void TextBoxUpdateSessionMinute_TextChanged(object sender, TextChangedEventArgs e)
        {
            StartStopAutoUpdating();
        }

        /// <summary>
        /// Управление авто-обновлением данных сессии
        /// </summary>
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

        #endregion

        /// <summary>
        /// Сортировка списка сессий по убыванию значения времени блокировки СУБД
        /// </summary>
        private void SortListBasesToDbProcTook()
        {
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                if (AppSettings.SortDbProcTook)
                {
                    ListBases = new ObservableCollection<Connector.Models.InfoBase>(ListBases.OrderBy(f => -f.DbProcTook));
                    for (int i = 0; i < ListBases.Count; i++)
                        ListBases[i].ListSessions = new List<Connector.Models.Session>(ListBases[i].ListSessions.OrderBy(f => -f.DbProcTook));
                }
                SetItemSourceListSession();
                RefreshUI();
            }));
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
                    Dialogs.Show("Не удалось отключить сессию.\n" + ex.Message);
                }
            }
        }

        private static void UpdateBindingTarget(DependencyObject target, DependencyProperty dp)
        {
            try
            {
                BindingOperations.GetBindingExpression(target, dp).UpdateTarget();
            }
            catch (Exception)
            {
            }
        }

        private void InitializeTaskbarIcon()
        {
            #region menuItemTopmostInWindow

            MenuItem menuItemTopmostInWindow = new MenuItem()
            {
                HeaderTemplate = new DataTemplate()
                {
                    DataType = typeof(MenuItem),
                    VisualTree = CreateHeaderTemplate_VisualTree("Поверх всех окон")
                },
                IsChecked = AppSettings.IsTopmost,
                IsCheckable = true
            };
            menuItemTopmostInWindow.Click += (object sender, RoutedEventArgs e) =>
            {
                bool newValueIsTopmost = !AppSettings.IsTopmost;
                AppSettings.IsTopmost = newValueIsTopmost;
                Topmost = newValueIsTopmost;
            };

            #endregion

            #region menuItemExit

            MenuItem menuItemExit = new MenuItem()
            {
                Header = "Выход"
            };
            menuItemExit.Click += (object sender, RoutedEventArgs e) => { Application.Current.Shutdown(); };

            #endregion

            TaskbarIcon.Icon.SetContextMenu(new ContextMenu()
            {
                Items =
                {
                    menuItemTopmostInWindow,
                    new Separator(),
                    menuItemExit
                }
            });
        }

        private static FrameworkElementFactory CreateHeaderTemplate_VisualTree(string header, ImageSource image = null)
        {
            FrameworkElementFactory elementFactoryAutostartTextBlock = new FrameworkElementFactory(typeof(TextBlock));
            elementFactoryAutostartTextBlock.SetValue(TextBlock.TextProperty, header);
            elementFactoryAutostartTextBlock.SetValue(MarginProperty, new Thickness(0, 0, 5, 0));

            FrameworkElementFactory elementFactoryAutostart = new FrameworkElementFactory(typeof(StackPanel));
            elementFactoryAutostart.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            elementFactoryAutostart.AppendChild(elementFactoryAutostartTextBlock);

            if (image != null)
            {
                FrameworkElementFactory elementFactoryAutostartIcon = new FrameworkElementFactory(typeof(Image));
                elementFactoryAutostartIcon.SetValue(Image.SourceProperty, image);
                elementFactoryAutostartIcon.SetValue(WidthProperty, 14.0);
                elementFactoryAutostartIcon.SetValue(HeightProperty, 14.0);

                elementFactoryAutostart.AppendChild(elementFactoryAutostartIcon);
            }

            return elementFactoryAutostart;
        }

        #region Filter data

        private void ApplyFilterListBase()
        {
            Safe.SafeAction(() =>
            {
                if (string.IsNullOrWhiteSpace(AppSettings.FindBase))
                    ListBases = new ObservableCollection<Connector.Models.InfoBase>(ListBasesNotFiltered);
                else
                    ListBases = new ObservableCollection<Connector.Models.InfoBase>(ListBasesNotFiltered.Where(f => f.Name.ToUpper().Contains(AppSettings.FindBase.ToUpper())).ToList());

                RefreshUI();
            });
        }

        private void ApplyFilterListUser()
        {
            Safe.SafeAction(() =>
            {
                if (string.IsNullOrWhiteSpace(AppSettings.FindUser))
                    DataGridListSessions.ItemsSource = _selectedItemListBases?.ListSessions;
                else
                {
                    string textFilter = AppSettings.FindUser.ToUpper();

                    DataGridListSessions.ItemsSource = _selectedItemListBases?.ListSessions.Where(
                        f => f.UserName.ToUpper().Contains(textFilter)
                        || f.AppID.ToUpper().Contains(textFilter)
                        || f.Host.ToUpper().Contains(textFilter));
                }
            });
        }

        #endregion

        #region Visibility datagrid

        private void LabelListBaseCollapsed_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => ChangeVisibilityPanelListBases(Visibility.Collapsed);

        private void LabelListBaseVisible_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => ChangeVisibilityPanelListBases(Visibility.Visible);

        private void ChangeVisibilityPanelListBases(Visibility newVisibility)
        {
            DoubleAnimation timeAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            timeAnimation.Completed += (object sender, EventArgs e) =>
            {
                GridListBases.Visibility = newVisibility;
                LabelListBaseVisible.Visibility = newVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                LabelListBaseCollapsed.Visibility = newVisibility;
            };

            if (newVisibility == Visibility.Visible)
            {
                timeAnimation.To = 450;
                ChangeVisibilityBeginAnimation(null, null, BorderListBases, timeAnimation);
            }
            else
            {
                ChangeVisibilityBeginAnimation(BorderListBases, timeAnimation, null, null);
            }
        }

        private void ChangeVisibilityBeginAnimation(
            FrameworkElement visibleElement,
            DoubleAnimation visibleAnimation,
            FrameworkElement collapsedElement,
            DoubleAnimation сollapsedAnimation)
        {
            if (collapsedElement != null)
            {
                сollapsedAnimation.Completed += (object sender, EventArgs e) => { collapsedElement.Visibility = Visibility.Visible; };
                collapsedElement.BeginAnimation(WidthProperty, сollapsedAnimation);
            }

            if (visibleElement != null)
            {
                visibleElement.Visibility = Visibility.Visible;
                visibleElement.BeginAnimation(WidthProperty, visibleAnimation);
            }
        }

        #endregion

        #region Visibility column datagrid session

        private List<MenuItem> GetMenuItemContextMenuDataGridSessionColumnHeader()
        {
            List<MenuItem> list = new List<MenuItem>();

            foreach (DataGridColumn itemColumn in DataGridListSessions.Columns)
                list.Add(GetMenuItemByDataGridColumn(itemColumn));

            return list;
        }

        private MenuItem GetMenuItemByDataGridColumn(DataGridColumn dataGridColumn)
        {
            MenuItem menuItem = new MenuItem()
            {
                Header = dataGridColumn.Header,
                IsCheckable = true,
                IsChecked = dataGridColumn.Visibility == Visibility.Visible,
                StaysOpenOnClick = true
            };
            menuItem.Checked += (object sender, RoutedEventArgs e) =>
            {
                ChangeVisibleColumnDataGridSession(((MenuItem)sender).Header, ((MenuItem)sender).IsChecked);
            };
            menuItem.Unchecked += (object sender, RoutedEventArgs e) =>
            {
                ChangeVisibleColumnDataGridSession(((MenuItem)sender).Header, ((MenuItem)sender).IsChecked);
            };

            return menuItem;
        }

        private void ChangeVisibleColumnDataGridSession(object header, bool isChecked)
        {
            DataGridColumn findedColumn = DataGridListSessions.Columns.FirstOrDefault(f => (string)f.Header == (string)header);
            if (findedColumn != null)
                findedColumn.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        private void ButtonConnectTo1CServerSettings_Click(object sender, RoutedEventArgs e)
        {
            var windowSettings = new ConnectTo1CServerSettingsWindow() { Owner = this };
            windowSettings.ShowDialog();
            _connectSettings = windowSettings.Data;
        }

        private void ButtonListProcessRphost1C_Click(object sender, RoutedEventArgs e)
        {
            var windowProcesses = new Rphosts1CServer(AppSettings.ServerName, _connectSettings) { Owner = this };
            if (windowProcesses.LoadProcesses())
            {
                windowProcesses.ShowDialog();
                Focus();
            }
        }

        private void MenuItemUpdateSessionMinute_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.UpdateSessionMinute = int.Parse((string)((MenuItem)sender).Tag);
            TextBoxUpdateSessionMinute.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }
    }
}
