using Newtonsoft.Json;
using System;
using Windows.Storage;

namespace Kurosuke_Universal.Utils
{
    public class StoreSettings
    {
        public StoreSettings()
        {
            try
            {
                localSettings = ApplicationData.Current.LocalSettings;
            }
            catch (Exception)
            {
            }
        }
        ApplicationDataContainer localSettings;
        public TValue TryGetValueWithDefault<TValue>(string key, TValue defaultvalue)
        {
            TValue value;

            // If the key exists, retrieve the value.
            if (localSettings.Values.ContainsKey(key))
            {
                var json = (string)localSettings.Values[key];
                value = JsonConvert.DeserializeObject<TValue>(json);

            }
            // Otherwise, use the default value.
            else
            {
                value = defaultvalue;
            }

            return value;
        }

        public bool AddOrUpdateValue(string key, object value)
        {
            bool valueChanged = false;
            string json = JsonConvert.SerializeObject(value);

            // If the key exists
            //if (localSettings.Contains(Key))
            if (localSettings.Values.ContainsKey(key))
            {
                // If the value has changed
                if ((string)localSettings.Values[key] != json)
                {
                    // Store the new value
                    localSettings.Values[key] = json;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                localSettings.Values.Add(key, json);
                valueChanged = true;
            }

            return valueChanged;
        }

        public void DeleteAllSettings()
        {
            localSettings.Values.Remove("Accounts");
            localSettings.Values.Remove("UseInAppBrowser");
            localSettings.Values.Remove("TweetCount");
            localSettings.Values.Remove("TextFontSize");
        }

    }
}
