namespace SteamLauncher
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Windows.Forms;

    public static class Launcher
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var account = getSteamAccount();
            bool runSteam = true;

            if (string.IsNullOrWhiteSpace(account.Username) || args.Contains("/credentials") || string.IsNullOrWhiteSpace(account.SteamFolder))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var form = new CredentialsForm(ref account);
                Application.Run(form);
                runSteam = form.RunSteam;
            }
            if (runSteam)
            {
                account = getSteamAccount();
                RunSteam(account);
            }
        }

        static SteamAccount getSteamAccount()
        {

            var username = Properties.Settings.Default.username;
            var encryptedPassword = Properties.Settings.Default.password;
            var key = Properties.Settings.Default.key;
            var steamFolder = Properties.Settings.Default.steamlocation;

            return new SteamAccount
            {
                Username = username,
                Password = encryptedPassword,
                Key = key,
                SteamFolder = steamFolder
            };
        }

        static void RunSteam(SteamAccount steamAccount)
        {
            try
            {
                HttpWebRequest w = WebRequest.Create("http://localhost:8088/steam/" + Environment.UserName.ToLower() + "/stop") as HttpWebRequest;

                var response = w.GetResponse();

                using (Stream s = response.GetResponseStream())
                using (StreamReader readStream = new StreamReader(s, Encoding.UTF8))
                {
                    string responsestring = readStream.ReadToEnd();
                    if (responsestring.StartsWith("steam stopped") || responsestring.ToLower() == "steam not running")
                    {
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.FileName = steamAccount.SteamFolder;
                        info.CreateNoWindow = true;
                        info.UseShellExecute = false;
                        info.RedirectStandardError = true;
                        info.RedirectStandardInput = true;
                        info.RedirectStandardOutput = true;
                        var decryptedPassword = CryptoProvider.Decrypt(steamAccount.Password, steamAccount.Key);
                        info.Arguments = "-login " + steamAccount.Username + " " + decryptedPassword;
                        Process.Start(info);
                    }
                    else
                    {
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.FileName = steamAccount.SteamFolder;
                        info.CreateNoWindow = true;
                        info.UseShellExecute = false;
                        info.RedirectStandardError = true;
                        info.RedirectStandardInput = true;
                        info.RedirectStandardOutput = true;
                        Process.Start(info);
                    }
                }
            }

            catch (Exception e)
            {
                MessageBox.Show("Unable to launch steam, check if the SteamStopperService is running. The unfriendly error message is: " + e.Message);
            }
        }

        public class SteamAccount
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Key { get; set; }
            public string SteamFolder { get; set; }
        }
    }
}