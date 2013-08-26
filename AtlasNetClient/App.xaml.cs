using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AtlasNetClient
{
    public partial class App : Application
    {
        public static App Instance;
        public string DataPath, ConfigPath;
        public Config Config;
        public ConnectionPool ConnectionPool = new ConnectionPool();

        public App() : base()
        {
            Instance = this;

            DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AtlasNet");
            Directory.CreateDirectory(DataPath);
            ConfigPath = Path.Combine(DataPath, "config.json");

            if (File.Exists(ConfigPath))
            {
                Config = Config.Load(ConfigPath);
            }
            else
            {
                Config = new Config();
                Config.Save(ConfigPath);
            }

            if (!Config.Contacts.Any(x => x.IsAnonymous))
                Config.Contacts.Add(new Contact { Name = "Anonymous", PublicKey = null });

            Run(new MainWindow());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Config.Save(ConfigPath);
        }

        [System.STAThreadAttribute()]
        public static void Main()
        {
            new App();
        }
    }
}
