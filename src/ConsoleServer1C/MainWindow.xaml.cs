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

namespace ConsoleServer1C
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Models.InfoBase _selectedItemListBases = new Models.InfoBase();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            UpdateInfoMainWindowEvents.UpdateListBasesMainWindowEvent += () =>
            {
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    RefreshDataContextListBase(UpdateInfoMainWindowEvents.InfoBases);
                }));
            };
        }

        public ObservableCollection<Models.InfoBase> ListBases { get; private set; } = new ObservableCollection<Models.InfoBase>();
        public Models.InfoBase SelectedItemListBases
        {
            get => _selectedItemListBases;
            set { _selectedItemListBases = value; DataGridListSessions.ItemsSource = _selectedItemListBases?.ListSessions; }
        }
        public AppSettings AppSettings { get; set; } = new AppSettings();

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

        private async void UpdateListBases()
        {
            ListBases.Clear();
            try
            {
                using (ConnectToAgent connectToAgent = new ConnectToAgent(AppSettings.ServerName))
                {
                    //if (infoBaseUpdate != null)
                    //{
                    //    connectToAgent.InfoBaseUpdate = infoBaseUpdate;
                    //    connectToAgent.SetListInfoBases(_listBases.ToList());
                    //}
                    //else if (updateOnlySeansInfo)
                    //    connectToAgent.SetListInfoBases(_listBases.ToList());

                    //connectToAgent.UpdateOnlySeansInfo = updateOnlySeansInfo;

                    connectToAgent.InfoBases.Clear();
                    foreach (Models.InfoBase item in ListBases)
                        connectToAgent.InfoBases.Add(item);

                    await connectToAgent.GetListBaseAsync();
                }

                //LastUpdate = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

                //BindingOperations.GetBindingExpression(TextBlockLastUpdate, TextBlock.TextProperty).UpdateTarget();
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

    }
}
