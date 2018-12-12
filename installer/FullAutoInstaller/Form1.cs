using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace bpmInstaller
{
    public partial class Form1 : Form
    {
        string payloadFolder = "installData";
        string helpString = "\nIf you have persistent trouble, ask @deeBo on the modding Discord, or pull up an issue on GitHub at Adybo123/bpm";
        bool showUninstallMessage = true;
        public Form1()
        {
            InitializeComponent();
        }

        string getTextFileContents(string path)
        {
            StreamReader sr = new StreamReader(path);
            string txt = sr.ReadToEnd();
            sr.Close();
            return txt;
        }

        bool isBeatSaberFolder(string installDir, string bsName)
        {
            return File.Exists(Path.Combine(installDir, $"{bsName}.exe")) && Directory.Exists(Path.Combine(installDir, $"{bsName}_Data"));
        }

        void installbpm(object sender, EventArgs e)
        {
            string installDir = txtInstallDir.Text;
            if (!isBeatSaberFolder(installDir, "Beat Saber"))
            {
                if (isBeatSaberFolder(installDir, "Game"))
                {
                    DialogResult installOver = MessageBox.Show("It looks like a version of bpm is already installed there.\nDo you want to install over the old one?",
                        "bpm Installer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (installOver == DialogResult.No) return;
                    showUninstallMessage = false;
                    uninstallbpm(null, null);
                }
                else
                {
                    MessageBox.Show($"That doesn't seem to be a Beat Saber install folder. Please check the path and try again.\n{helpString}",
                      "bpm Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                // Rename Beat Saber (with a copy so it's not gone if we fail to extract our version)
                File.Copy(Path.Combine(installDir, "Beat Saber.exe"), Path.Combine(installDir, "Game.exe"));
                Directory.Move(Path.Combine(installDir, "Beat Saber_Data"), Path.Combine(installDir, "Game_Data"));

                // Edit the settings json
                string jsonPath = Path.Combine(payloadFolder, "bpm.json");
                dynamic settings = JsonConvert.DeserializeObject(getTextFileContents(jsonPath));
                string platform = "steam";
                // Experimental Oculus support
                if (!installDir.ToLower().Contains("steam") && installDir.ToLower().Contains("oculus")) platform = "oculus";

                settings.platform = platform;
                settings.installDir = installDir;

                // Re-save
                StreamWriter sw = new StreamWriter(jsonPath);
                sw.Write(JsonConvert.SerializeObject(settings, Formatting.Indented));
                sw.Close();

                // Copy bpm to folder
                DirectoryInfo d = new DirectoryInfo(payloadFolder);
                foreach (FileInfo f in d.GetFiles())
                {
                    File.Copy(f.FullName, Path.Combine(installDir, f.Name), true);
                }

                // Done
                MessageBox.Show("bpm has been installed with default settings underneath Beat Saber. Start it from Steam or Oculus home, and bpm will update your mods before launching if necessary.\nYou can re-run this installer at any point to put your game back how bpm found it.\n\nThe documentation will now be opened. Read it to find out how to add custom mods.",
                    "bpm Installer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start("https://github.com/Adybo123/bpm/blob/master/README.md");
                Application.Exit();
            } catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong while installing bpm.\nbpm will now put your Beat Saber folder back how it was just in case anything was messed up.{helpString}",
                    "bpm Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                uninstallbpm(null, null);
                logException(ex);
            }
        }
        void uninstallbpm(object sender, EventArgs e)
        {
            string installDir = txtInstallDir.Text;
            if (!isBeatSaberFolder(installDir, "Game"))
            {
                MessageBox.Show($"bpm doesn't seem to be installed there. Please check the path and try again.{helpString}",
                    "bpm Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Un-rename Beat Saber
                string gameExe = Path.Combine(installDir, "Game.exe");
                if (File.Exists(gameExe))
                {
                    string beatSaberExe = Path.Combine(installDir, "Beat Saber.exe");
                    if (File.Exists(beatSaberExe)) File.Delete(beatSaberExe);
                    File.Move(gameExe, beatSaberExe);
                }
                string gameFolder = Path.Combine(installDir, "Game_Data");
                if (Directory.Exists(gameFolder))
                {
                    string beatSaberFolder = Path.Combine(installDir, "Beat Saber_Data");
                    Directory.Move(gameFolder, beatSaberFolder);
                }

                if (showUninstallMessage) MessageBox.Show("The bpm game launcher has been uninstalled.\nMods might still be installed. If this isn't what you want, delete the .dll files in /plugins/ in the install folder.",
                    "bpm Installer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex)
            {
                MessageBox.Show($"An error occurred. Check installLog.txt.{helpString}",
                    "bpm Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logException(ex);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog oFD = new FolderBrowserDialog();
            oFD.Description = "Please select the folder which contains Beat Saber_Data if installing and Game_Data if uninstalling.";
            oFD.ShowDialog();
            txtInstallDir.Text = oFD.SelectedPath;
        }

        void logException(Exception ex)
        {
            StreamWriter sw = new StreamWriter("installerLog.txt");
            sw.Write(ex.ToString());
            sw.Close();
        }

        void tryDetectInstallLocation()
        {
            try
            {
                string installPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 620980", "InstallLocation", null);
                txtInstallDir.Text = installPath ?? throw new Exception("Steam install registry key not present.");
            } catch (Exception ex) {
                txtInstallDir.Text = "Please select your install folder... and be careful.";
                MessageBox.Show("It doesn't look like you're using the Steam version of Beat Saber.\n\nPlease note that, for now, it is not recommended to install bpm on anything but the Steam version. The installer will open, but you should not do this unless you're helping @deeBo test.",
                    "bpm Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logException(ex);
            } 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("installData"))
            {
                MessageBox.Show("The installData folder was not found.\nDid you extract the zip file before running the installer?\nTry downloading it again.",
                    "bpm Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            tryDetectInstallLocation();
        }
    }
}
