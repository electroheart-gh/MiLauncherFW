using System;
using System.Windows.Forms;

namespace MiLauncher
{
    internal static class Program
    {
        // Global variable for configuration
        static public AppSettings appSettings;

        // Constant for configuration
        private const string configFilePath = "myConfig.json";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Read configuration
            appSettings = SettingManager.LoadSettings<AppSettings>(configFilePath);

            if (appSettings is null) {
                appSettings = new AppSettings();
                SettingManager.SaveSettings(appSettings, configFilePath);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
