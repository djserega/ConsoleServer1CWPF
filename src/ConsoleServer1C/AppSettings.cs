using ConsoleServer1C.Converters;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleServer1C
{
    public sealed class AppSettings : Models.NotifyPropertyChangedClass
    {
        #region Private fields

        private readonly string _compName = Environment.MachineName + "Software";
        private const string _prefixRegistrySoftware = "Software";
        private const string _prefixKeyApplication = "ConsoleServer1C";
        private const string _prefixRegistryKey = _prefixRegistrySoftware + "\\" + _prefixKeyApplication;

        private string _findBase = string.Empty;
        private string _findUser = string.Empty;

        #endregion

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

        ~AppSettings()
        {
            SetDefaultSettings(saveCurrent: true);
        }

        #region Public properties

        public string ServerName { get; set; } = string.Empty;
        public int UpdateSessionMinute { get; set; } = 60;
        public string FilterInfoBaseName { get; set; } = string.Empty;
        public bool SortDbProcTook { get; set; } = true;
        public bool NotifyWhenBlockingTimeDBIsExceeded { get; set; } = false;
        public List<Models.HistoryConnection> ListHistoryConnection { get; private set; } = new List<Models.HistoryConnection>();
        public Dictionary<object, bool> VisibilityDataGridSessionColumn { get; private set; } = new Dictionary<object, bool>();

        public string FindBase { get => _findBase; set { _findBase = value; NotifyPropertyChanged(); } }
        public string FindUser { get => _findUser; set { _findUser = value; NotifyPropertyChanged(); } }

        #endregion

        #region Internal property ExceededThreshold

        internal static readonly int ExceededThresholdDbProcTookCritical = 30;
        internal static readonly int ExceededThresholdDbProcTookHigh = 20;
        internal static readonly int ExceededThresholdDbProcTookElevated = 10;

        #endregion

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

        private void SetValue(RegistryKey regKey, string key, string value, bool converting = false)
        {
            string regValue;
            if (converting)
                regValue = ConverterToValue(value);
            else
                regValue = value ?? string.Empty;

            regKey?.SetValue(key, regValue);
        }

        private string ConverterToValue(string text)
        {
            byte[] decrypted = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = new byte[decrypted.Length];
            for (int i = 0; i < decrypted.Length; i++)
                encrypted[i] = (byte)(decrypted[i] ^ _compName[i % _compName.Length]);
            return Convert.ToBase64String(encrypted);
        }

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

        private void SetValueIfNotFinded(RegistryKey regKey, string[] names, string key, string value = "", bool initialize = false)
        {
            if (!string.IsNullOrWhiteSpace(names.FirstOrDefault(f => f == key)) || initialize)
            {
                string currentValue = GetValue(regKey, key, true);
                if (string.IsNullOrEmpty(currentValue) || initialize)
                    SetValue(regKey, key, value, true);
            }
        }
        private void SetValueIfNotFinded(RegistryKey regKey, string[] names, string key, int value = 0, bool initialize = false)
        {
            if (!string.IsNullOrWhiteSpace(names.FirstOrDefault(f => f == key)) || initialize)
            {
                string currentValue = GetValue(regKey, key);
                if (string.IsNullOrEmpty(currentValue) || initialize)
                    SetValue(regKey, key, value.ToString());
            }
        }
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
