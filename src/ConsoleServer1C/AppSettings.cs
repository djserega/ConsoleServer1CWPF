using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C
{
    public class AppSettings : Models.NotifyPropertyChangedClass
    {
        private const string _prefixRegistrySoftware = "Software";
        private const string _prefixKeyApplication = "ConsoleServer1C";
        private const string _prefixRegistryKey = _prefixRegistrySoftware + "\\" + _prefixKeyApplication;

        private string _findBase = string.Empty;
        private string _findUser = string.Empty;

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

        public string ServerName { get; set; } = string.Empty;
        public int UpdateSessionMinute { get; set; } = 60;
        public string FilterInfoBaseName { get; set; } = string.Empty;
        public bool SortDbProcTook { get; set; } = true;
        public bool NotifyWhenBlockingTimeDBIsExceeded { get; set; } = false;

        public string FindBase { get => _findBase; set { _findBase = value; NotifyPropertyChanged(); } }
        public string FindUser { get => _findUser; set { _findUser = value; NotifyPropertyChanged(); } }

        internal static readonly int ExceededThresholdDbProcTookCritical = 30;
        internal static readonly int ExceededThresholdDbProcTookHigh = 20;
        internal static readonly int ExceededThresholdDbProcTookElevated = 10;

        internal void GetAllSettings()
        {
            try
            {
                using (RegistryKey currentUser = Registry.CurrentUser)
                {
                    using (RegistryKey registryKeyApplication = currentUser.OpenSubKey(_prefixRegistryKey, true))
                    {
                        ServerName = GetValue(registryKeyApplication, "ServerName");

                        int.TryParse(GetValue(registryKeyApplication, "UpdateSessionMinute"), out int UpdateSessionMinuteValue);
                        UpdateSessionMinute = UpdateSessionMinuteValue;

                        FilterInfoBaseName = GetValue(registryKeyApplication, "FilterInfoBaseName");

                        bool.TryParse(GetValue(registryKeyApplication, "SortDbProcTook"), out bool SortDbProcTookValue);
                        SortDbProcTook = SortDbProcTookValue;

                        bool.TryParse(GetValue(registryKeyApplication, "NotifyWhenBlockingTimeDBIsExceeded"), out bool NotifyWhenBlockingTimeDBIsExceededValue);
                        NotifyWhenBlockingTimeDBIsExceeded = NotifyWhenBlockingTimeDBIsExceededValue;
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
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names, "ServerName", ServerName, keyEmpty || saveCurrent);
                        if (keyEmpty || key == "UpdateSessionMinute")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names, "UpdateSessionMinute", UpdateSessionMinute, keyEmpty || saveCurrent);
                        if (keyEmpty || key == "FilterInfoBaseName")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names, "FilterInfoBaseName", FilterInfoBaseName, keyEmpty || saveCurrent);
                        if (keyEmpty || key == "SortDbProcTook")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names, "SortDbProcTook", SortDbProcTook, keyEmpty || saveCurrent);
                        if (keyEmpty || key == "NotifyWhenBlockingTimeDBIsExceeded")
                            SetValueIfNotFinded(tempRegistryKeyApplicationValues, names, "NotifyWhenBlockingTimeDBIsExceeded", NotifyWhenBlockingTimeDBIsExceeded, keyEmpty || saveCurrent);
                    }
                }
            }
        }


        private string GetValue(RegistryKey regKey, string keyName) => regKey.GetValue(keyName)?.ToString();

        private void SetValue(RegistryKey regKey, string key, string value) => regKey?.SetValue(key, value ?? string.Empty);

        private void SetValueIfNotFinded(RegistryKey regKey, string[] names, string key, string value = "", bool initialize = false)
        {
            if (!string.IsNullOrWhiteSpace(names.FirstOrDefault(f => f == key)) || initialize)
            {
                string currentValue = GetValue(regKey, key);
                if (string.IsNullOrEmpty(currentValue) || initialize)
                    SetValue(regKey, key, value);
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
    }
}
