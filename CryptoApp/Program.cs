using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using log4net;

namespace CryptoApp
{
    static class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _logger.Info("CryptoApp Launched");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CryptoForm());
            _logger.Info("CryptoApp Exited");
        }
    }
}
