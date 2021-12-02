using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JetBrains.Annotations;

namespace Auth.Utils
{
    public static class UtilsConfig
    {
        private static NanoConf _internalConfig;
        private static readonly string Path = Directory.GetCurrentDirectory() + "/apiConfig.json";

        public static NanoConf Config
        {
            get
            {
                TryLoad();
                return _internalConfig;
            }
        }
        static UtilsConfig() => TryLoad();

        private static void TryLoad()
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path);
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        _internalConfig = JsonConvert.DeserializeObject<NanoConf>(json);
                    }
                    catch (Exception ex)
                    {
                        nanoAPIAdminPanel.Auth.LoginWindow.NanoLog(ex.ToString(), ConsoleColor.Red);
                    }
                }
            }
            if (_internalConfig != null) return;
            _internalConfig = new NanoConf();
            Save();
        }

        public static void Save()
        {
            try
            {
                File.WriteAllText(Path, JsonConvert.SerializeObject(_internalConfig));
            }
            catch (Exception e)
            {
                nanoAPIAdminPanel.Auth.LoginWindow.NanoLog(e.ToString(), ConsoleColor.Red);
            }
        }
        public class NanoConf
        {
            [CanBeNull] public string AuthKey { get; set; }
        }
    }
}
