using System;
using System.Threading;
using System.Windows.Forms;
using ExchangeApp.Localization;

namespace ExchangeApp
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            using (ExchangeNotification notification = new ExchangeNotification())
            {
                while (notification.IsVisible)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }
            }

            Application.Exit();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                e.ExceptionObject.ToString(),
                string.Format(Translations.Alert_Unhandled_Exception, e.ExceptionObject.GetType()),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
