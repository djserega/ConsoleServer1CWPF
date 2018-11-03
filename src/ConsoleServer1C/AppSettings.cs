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

        public string ServerName { get; set; }
        public int UpdateSessionMinute { get; set; }
        public string FilterInfoBaseName { get; set; }


        internal void GetAllSettings()
        {
            try
            {
                using (RegistryKey currentUser = Registry.CurrentUser)
                {
                    using (RegistryKey registryKeyApplication = currentUser.OpenSubKey(_prefixRegistryKey, true))
                    {
                        ServerName = GetValue(registryKeyApplication, "ServerName");

                        int.TryParse(GetValue(registryKeyApplication, "UpdateSessionMinute"), out int updateSessionMinute);
                        UpdateSessionMinute = updateSessionMinute;

                        FilterInfoBaseName = GetValue(registryKeyApplication, "FilterInfoBaseName");
                    }
                }
            }
            catch (Exception ex)
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
    }
}
