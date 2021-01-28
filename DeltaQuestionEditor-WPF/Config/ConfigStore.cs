using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static DeltaQuestionEditor_WPF.Helpers.Helper;
using Newtonsoft.Json;

namespace DeltaQuestionEditor_WPF.Config
{
    static class ConfigStore
    {
        public static ConfigObject Config { get; private set; }

        static ConfigStore()
        {
            EnsurePathExist(AppDataPath());
            Load();
        }

        public static void Save()
        {
            EnsurePathExist(AppDataPath());
            File.WriteAllText(AppDataPath("config.json"), JsonConvert.SerializeObject(Config));
        }

        public static void Load()
        {
            if (Config != null)
            {
                Config.PropertyChanged -= Config_PropertyChanged;
            }
            if (File.Exists(AppDataPath("config.json")))
            {
                Config = (ConfigObject)JsonConvert.DeserializeObject(File.ReadAllText(AppDataPath("config.json")), typeof(ConfigObject));
            }
            else
            {
                Config = new ConfigObject();
            }
            Config.PropertyChanged += Config_PropertyChanged;
        }

        private static void Config_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Save();
        }
    }
}
