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

namespace FullAutoInstaller
{
    public partial class Form1 : Form
    {
        string payloadFolder = "installData";
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

        void installFullAuto(object sender, EventArgs e)
        {
            string installDir = txtInstallDir.Text;
            if (!Directory.Exists(Path.Combine(installDir, "Beat Saber_Data")))
            {
                MessageBox.Show("FullAuto couldn't find a directory called 'Beat Saber_Data' in the Beat Saber folder. Did you get the folder correct? Hit the button again to force install (not recommended).");
                Directory.CreateDirectory(Path.Combine(installDir, "Beat Saber_Data"));
                return;
            }
            // Rename Beat Saber
            File.Move(Path.Combine(installDir, "Beat Saber.exe"), Path.Combine(installDir, "Game.exe"));
            Directory.Move(Path.Combine(installDir, "Beat Saber_Data"), Path.Combine(installDir, "Game_Data"));

            // Edit the settings json
            string jsonPath = Path.Combine(payloadFolder, "fullAuto.json");
            dynamic settings = JsonConvert.DeserializeObject(getTextFileContents(jsonPath));
            string platform = "steam";
            // Experimental Oculus support
            if (!installDir.ToLower().Contains("steam") && installDir.ToLower().Contains("oculus")) platform = "oculus";

            settings.platform = platform;
            settings.installFolder = installDir;

            // Re-save
            StreamWriter sw = new StreamWriter(jsonPath);
            sw.Write(JsonConvert.SerializeObject(settings));
            sw.Close();

            // Copy FullAuto to folder
            DirectoryInfo d = new DirectoryInfo(payloadFolder);
            foreach (FileInfo f in d.GetFiles())
            {
                File.Copy(f.FullName, Path.Combine(installDir, f.Name), true);
            }

            // Done
            MessageBox.Show("FullAuto has been installed with default settings underneath Beat Saber. Start it from Steam or Oculus home, and FullAuto will update your mods before launching if necessary.\nYou can re-run this installer at any point to put your game back how FullAuto found it.\n\nThe documentation will now be opened. Read it to find out how to add custom mods.");
            Application.Exit();
        }
        void uninstallFullAuto(object sender, EventArgs e)
        {
            string installDir = txtInstallDir.Text;
            if (!Directory.Exists(Path.Combine(installDir, "Game_Data")))
            {
                MessageBox.Show("FullAuto couldn't find a directory called 'Game_Data' in the Beat Saber folder. Did you get the folder correct? Hit the button again to force uninstall (not recommended).");
                Directory.CreateDirectory(Path.Combine(installDir, "Game_Data"));
                return;
            }
            // Un-rename Beat Saber
            File.Delete(Path.Combine(installDir, "Beat Saber.exe"));
            File.Move(Path.Combine(installDir, "Game.exe"), Path.Combine(installDir, "Beat Saber.exe"));
            Directory.Move(Path.Combine(installDir, "Game_Data"), Path.Combine(installDir, "Beat Saber_Data"));

            MessageBox.Show("The FullAuto game launcher has been uninstalled.\nMods might still be installed. If this isn't what you want, delete the .dll files in /plugins/ in the install folder.");
            Application.Exit();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog oFD = new FolderBrowserDialog();
            oFD.Description = "Please select the folder which contains Beat Saber_Data if installing and Game_Data if uninstalling.";
            oFD.ShowDialog();
            txtInstallDir.Text = oFD.SelectedPath;
        }
    }
}
