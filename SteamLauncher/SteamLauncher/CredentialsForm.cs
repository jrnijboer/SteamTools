namespace SteamLauncher
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    public partial class CredentialsForm : Form
    {
        Launcher.SteamAccount steamAccount;
        public bool RunSteam = false;
        public CredentialsForm(ref Launcher.SteamAccount account)
        {
            steamAccount = account;
            InitializeComponent();
            btnSave.Select();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            var newKey = Guid.NewGuid().ToString() + "42040d5e-0480-45da-abea-f74933c4be20";

            Properties.Settings.Default.username = txtUsername.Text;
            if (txtPassword1.Text.Equals(txtPassword2.Text))
            {
                var encryptedPassword = CryptoProvider.Encrypt(txtPassword1.Text, newKey);
                Properties.Settings.Default.password = encryptedPassword;
            }

            Properties.Settings.Default.steamlocation = txtSteamFolder.Text;
            Properties.Settings.Default.key = newKey;            
            Properties.Settings.Default.Save();
            RunSteam = chkLaunchSteam.Checked;
            Close();
        }


        private void CredentialsForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(steamAccount.SteamFolder))
            {
                if (Environment.Is64BitOperatingSystem && File.Exists(@"C:\Program Files (x86)\Steam\Steam.exe"))
                {
                    txtSteamFolder.Text = @"c:\Program Files (x86)\Steam\Steam.exe";
                }
                else if (!Environment.Is64BitOperatingSystem && File.Exists(@"C:\Program Files\Steam\Steam.exe"))
                {
                    txtSteamFolder.Text = @"c:\Program Files\Steam\Steam.exe";
                }
            }
            else
            {
                txtSteamFolder.Text = steamAccount.SteamFolder;
            }

            txtUsername.Text = steamAccount.Username;

            var decrypted = CryptoProvider.Decrypt(steamAccount.Password, steamAccount.Key);
            txtPassword1.Text = decrypted;
            txtPassword2.Text = decrypted;
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Locate Steam executable folder";
            folderBrowserDialog1.ShowDialog();
            txtSteamFolder.Text = folderBrowserDialog1.SelectedPath;
        }
    }
}
