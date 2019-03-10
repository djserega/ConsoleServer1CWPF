using ConsoleServer1C.Converters;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleServer1C
{
    /// <summary>
    /// Работа с настройками приложения
    /// </summary>
    public sealed class AppSettings : Models.NotifyPropertyChangedClass
    {
        #region Private fields

        /// <summary>
        /// Ключ компьютера
        /// </summary>
        private readonly string _compName = Environment.MachineName + "Software";
        /// <summary>
        /// Ключ сохранения данных регистра
        /// </summary>
        private const string _prefixRegistrySoftware = "Software";
        /// <summary>
        /// Ключ приложения регистра
        /// </summary>
        private const string _prefixKeyApplication = "ConsoleServer1C";
        /// <summary>
        /// Ключ сохранения данных в регистре сведений
        /// </summary>
        private const string _prefixRegistryKey = _prefixRegistrySoftware + "\\" + _prefixKeyApplication;
        /// <summary>
        /// Строка фильтра списка баз данных
        /// </summary>
        private string _findBase = string.Empty;
        /// <summary>
        /// Строка фильтра списка сессий
        /// </summary>
        private string _findUser = string.Empty;

        #endregion

        /// <summary>
        /// Базовый конструктор настроек
        /// </summary>
        public AppSettings()
        {
            try
            {
                GetAllSettings();
            }
            catch (RegistrykeyNotFoundException)
            {
                SetDefaultSettings();
            }
        }

        /// <summary>
        /// Деструктор настроек. Сохранение текущих значений
        /// </summary>
        ~AppSettings()
        {
            SetDefaultSettings(saveCurrent: true);
        }

        #region Public properties

        /// <summary>
        /// Имя или IP адрес сервера 1С
        /// </summary>
        public string ServerName { get; set; } = string.Empty;
        /// <summary>
        /// Количество секунд авто-обновления данных
        /// </summary>
        public int UpdateSessionMinute { get; set; } = 60;
        /// <summary>
        /// Строка отбора списка баз данных при подключении к серверу 1С
        /// </summary>
        public string FilterInfoBaseName { get; set; } = string.Empty;
        /// <summary>
        /// Нужна ли сортировка по времени захвата СУБД
        /// </summary>
        public bool SortDbProcTook { get; set; } = true;
        /// <summary>
        /// Управление уведомлениями о превышении лимита времени захвата СУБД
        /// </summary>
        public bool NotifyWhenBlockingTimeDBIsExceeded { get; set; } = false;
        /// <summary>
        /// Список истории подключения
        /// </summary>
        public List<Models.HistoryConnection> ListHistoryConnection { get; private set; } = new List<Models.HistoryConnection>();
        /// <summary>
        /// Список настроек видимости колонок таблицы сессий
        /// </summary>
        public Dictionary<object, bool> VisibilityDataGridSessionColumn { get; private set; } = new Dictionary<object, bool>();
        /// <summary>
        /// Строка фильтра списка баз данных
        /// </summary>
        public string FindBase { get => _findBase; set { _findBase = value; NotifyPropertyChanged(); } }
        /// <summary>
        /// Строка фильтра списка сессий
        /// </summary>
        public string FindUser { get => _findUser; set { _findUser = value; NotifyPropertyChanged(); } }

        #endregion

        #region Internal property ExceededThreshold

        /// <summary>
        /// Критическое значение времени захвата СУБД
        /// </summary>
        internal static readonly int ExceededThresholdDbProcTookCritical = 30;
        /// <summary>
        /// Высокое значение времени захвата СУБД
        /// </summary>
        internal static readonly int ExceededThresholdDbProcTookHigh = 20;
        /// <summary>
        /// Повышенное значение времени захвата СУБД
        /// </summary>
        internal static readonly int ExceededThresholdDbProcTookElevated = 10;

        #endregion

        /// <summary>
        /// Получение всех сохраненных настроек
        /// </summary>
        internal void GetAllSettings()
        {
            try
            {
                using (RegistryKey currentUser = Registry.CurrentUser)
                {
                    using (RegistryKey registryKeyApplication = currentUser.OpenSubKey(_prefixRegistryKey, true))
                    {
                        ServerName = GetValue(registryKeyApplication, "ServerName", true);

                        int.TryParse(GetValue(registryKeyApplication, "UpdateSessionMinute"), out int UpdateSessionMinuteValue);
                        UpdateSessionMinute = UpdateSessionMinuteValue;

                        FilterInfoBaseName = GetValue(registryKeyApplication, "FilterInfoBaseName", true);

                        bool.TryParse(GetValue(registryKeyApplication, "SortDbProcTook"), out bool SortDbProcTookValue);
                        SortDbProcTook = SortDbProcTookValue;

                        bool.TryParse(GetValue(registryKeyApplication, "NotifyWhenBlockingTimeDBIsExceeded"), out bool NotifyWhenBlockingTimeDBIsExceededValue);
                        NotifyWhenBlockingTimeDBIsExceeded = NotifyWhenBlockingTimeDBIsExceededValue;

                        string ListHistoryConnectionValue = GetValue(registryKeyApplication, "HistoryConnection", true);
                        ListHistoryConnection = JsonConverter<List<Models.HistoryConnection>>.Load(ListHistoryConnectionValue)
                            ?? new List<Models.HistoryConnection>();

                        string VisibilityDataGridSessionColumnValue = GetValue(registryKeyApplication, "VisibilityDataGridSessionColumn", true);
                        VisibilityDataGridSessionColumn = JsonConverter<Dictionary<object, bool>>.Load(VisibilityDataGridSessionColumnValue)
                            ?? new Dictionary<object, bool>();
                    }
                }
            }
            catch (Exception)
            {
                throw new RegistrykeyNotFoundException("Не удалось получить настройки.");
            }
        }

        /// <summary>
        /// Установка значений по умолчанию
        /// </summary>
        /// <param name="key">Ключ настройки</param>
        /// <param name="saveCurrent">Сохранение текущего значения</param>
        internal void SetDefaultSettings(string key = "", bool saveCurrent = false)
        {
            bool keyEmpty = string.IsNullOrWhiteSpace(key);

            using (RegistryKey currentUser = Registry.CurrentUser)
            {
                using (RegistryKey registryKeyApplication = currentUser.OpenSubKey(_prefixRegistrySoftware, true))
                {
                    using (RegistryKey registryKeyApplicationValues = registryKeyApplication.OpenSubKey(_prefixKeyApplication, true))
                    {
                        RegistryKey tempRegistryKeyApplicationValues;
                        if (registryKeyApplicationValues == null)
                            tempRegistryKeyApplicationValues = registryKeyApplication.CreateSubKey(_prefixKeyApplication);
                        else
                            tempRegistryKeyApplicationValues = registryKeyApplicationValues;

                        string[] names = tempRegistryKeyApplicationValues.GetValueNames();

                        if (keyEmpty || key == "ServerName")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names,
                                                "ServerName",
                                                ServerName,
                                                keyEmpty || saveCurrent);
                        if (keyEmpty || key == "UpdateSessionMinute")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names,
                                                "UpdateSessionMinute",
                                                UpdateSessionMinute,
                                                keyEmpty || saveCurrent);
                        if (keyEmpty || key == "FilterInfoBaseName")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names,
                                                "FilterInfoBaseName",
                                                FilterInfoBaseName,
                                                keyEmpty || saveCurrent);
                        if (keyEmpty || key == "SortDbProcTook")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names,
                                                "SortDbProcTook",
                                                SortDbProcTook,
                                                keyEmpty || saveCurrent);
                        if (keyEmpty || key == "NotifyWhenBlockingTimeDBIsExceeded")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names,
                                                "NotifyWhenBlockingTimeDBIsExceeded",
                                                NotifyWhenBlockingTimeDBIsExceeded,
                                                keyEmpty || saveCurrent);
                        if (keyEmpty || key == "HistoryConnection")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names,
                                                "HistoryConnection",
                                                JsonConverter<List<Models.HistoryConnection>>.Save(ListHistoryConnection),
                                                keyEmpty || saveCurrent);
                        if (keyEmpty || key == "VisibilityDataGridSessionColumn")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names,
                                                "VisibilityDataGridSessionColumn",
                                                JsonConverter<Dictionary<object, bool>>.Save(VisibilityDataGridSessionColumn),
                                                keyEmpty || saveCurrent);
                    }
                }
            }
        }

        #region Private methods

        /// <summary>
        /// Загрузка сохраненного значения
        /// </summary>
        /// <param name="regKey">Ключ регистра</param>
        /// <param name="keyName">Ключ значения</param>
        /// <param name="converting">Необходимость конвертации значения</param>
        /// <returns></returns>
        private string GetValue(RegistryKey regKey, string keyName, bool converting = false)
        {
            string regResult = regKey.GetValue(keyName)?.ToString() ?? string.Empty;

            string result;
            if (converting)
                result = ConverterInValue(regResult);
            else
                result = regResult;

            return result;
        }

        /// <summary>
        /// Сохранение значения
        /// </summary>
        /// <param name="regKey">Ключ регистра</param>
        /// <param name="key">Ключ значения</param>
        /// <param name="value">Значение</param>
        /// <param name="converting">Необходимость конвертации</param>
        private void SetValue(RegistryKey regKey, string key, string value, bool converting = false)
        {
            string regValue;
            if (converting)
                regValue = ConverterToValue(value);
            else
                regValue = value ?? string.Empty;

            regKey?.SetValue(key, regValue);
        }

        /// <summary>
        /// Сериализация значения
        /// </summary>
        /// <param name="text">Значение</param>
        /// <returns>Результат конвертации</returns>
        private string ConverterToValue(string text)
        {
            byte[] decrypted = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = new byte[decrypted.Length];
            for (int i = 0; i < decrypted.Length; i++)
                encrypted[i] = (byte)(decrypted[i] ^ _compName[i % _compName.Length]);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Десериализация значения
        /// </summary>
        /// <param name="text">Сериализированное значение</param>
        /// <returns>Значение</returns>
        private string ConverterInValue(string text)
        {
            byte[] decoded;
            try
            {
                decoded = Convert.FromBase64String(text);
            }
            catch (FormatException)
            {
                return string.Empty;
            }
            byte[] result = new byte[decoded.Length];
            for (int c = 0; c < decoded.Length; c++)
                result[c] = (byte)((uint)decoded[c] ^ (uint)_compName[c % _compName.Length]);
            return Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// Установка значения в регистр
        /// </summary>
        /// <param name="regKey">Ключ регистра</param>
        /// <param name="names">Имена параметров</param>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <param name="initialize">Инициализация значения</param>
        private void SetValueIfNotFinded(RegistryKey regKey, string[] names, string key, string value = "", bool initialize = false)
        {
            if (!string.IsNullOrWhiteSpace(names.FirstOrDefault(f => f == key)) || initialize)
            {
                string currentValue = GetValue(regKey, key, true);
                if (string.IsNullOrEmpty(currentValue) || initialize)
                    SetValue(regKey, key, value, true);
            }
        }
        /// <summary>
        /// Установка значения в регистр
        /// </summary>
        /// <param name="regKey">Ключ регистра</param>
        /// <param name="names">Имена параметров</param>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <param name="initialize">Инициализация значения</param>
        private void SetValueIfNotFinded(RegistryKey regKey, string[] names, string key, int value = 0, bool initialize = false)
        {
            if (!string.IsNullOrWhiteSpace(names.FirstOrDefault(f => f == key)) || initialize)
            {
                string currentValue = GetValue(regKey, key);
                if (string.IsNullOrEmpty(currentValue) || initialize)
                    SetValue(regKey, key, value.ToString());
            }
        }
        /// <summary>
        /// Установка значения в регистр
        /// </summary>
        /// <param name="regKey">Ключ регистра</param>
        /// <param name="names">Имена параметров</param>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <param name="initialize">Инициализация значения</param>
        private void SetValueIfNotFinded(RegistryKey regKey, string[] names, string key, bool value = false, bool initialize = false)
        {
            if (!string.IsNullOrWhiteSpace(names.FirstOrDefault(f => f == key)) || initialize)
            {
                string currentValue = GetValue(regKey, key);
                if (string.IsNullOrEmpty(currentValue) || initialize)
                    SetValue(regKey, key, value.ToString());
            }
        }

        #endregion
    }
}
