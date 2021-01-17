using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ExchangeApp
{
    /// <summary>
    /// Simple, at least marginally secure settings you can leave on your computer.
    ///
    /// The settings are protected using .NET's Data Protection API (DPAPI) with 
    /// per-user keys.
    /// 
    /// This is, of course, not bullet-proof -- it's a trivially better alternative to 
    /// saving your password in plain text.
    /// 
    /// The common case, which this is based around on, is that it's more likely you 
    /// accidentally copy your settings file (<see cref="FileName"/>) somewhere, and 
    /// other random people can't just that file to peek into your schedule and join 
    /// your Zoom meetings.
    ///
    /// For anyone else than you being able to decrypt this file, they need access to 
    /// your (unlocked) computer, and at that point they can do much, much more.
    /// </summary>
    public sealed class Settings
    {
        private const string FileName = "ExchangeSettings.blob";
        private const char Splitter = '\u001f';

        public string ExchangeUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public static Settings Load()
        {
            if (!File.Exists(FileName))
                return new Settings();

            try
            {
                string content = Unprotect(File.ReadAllBytes(FileName));
                string[] tokens = content.Split(Splitter);
                if (tokens.Length != 3)
                    return new Settings();

                return new Settings
                {
                    ExchangeUrl = tokens[0],
                    User = tokens[1],
                    Password = tokens[2],
                };
            }
            catch (Exception)
            {
                return new Settings();
            }
        }

        public void Save()
        {
            File.WriteAllBytes(FileName, Protect(
                $"{ExchangeUrl}{Splitter}{User}{Splitter}{Password}"));
        }

        private static byte[] Protect(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            return ProtectedData.Protect(Encoding.UTF8.GetBytes(input), null, DataProtectionScope.CurrentUser);
        }

        private static string Unprotect(byte[] input)
        {
            if (input == null || input.Length == 0)
                return null;

            return Encoding.UTF8.GetString(ProtectedData.Unprotect(input, null, DataProtectionScope.CurrentUser));
        }
    }
}
