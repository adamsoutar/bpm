using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Net;
using Ionic.Zip;

namespace FullAuto
{
    class Program
    {
        static string settingsPath = "fullAuto.json";
        static string apiVersion = "1.1";
        static string IPAZipURL = "https://github.com/Eusth/IPA/releases/download/3.3/IPA_3.3.zip";
        static string apiBase = "https://www.modsaber.org";
        static string apiURL = $"{apiBase}/api/v{apiVersion}/";
        static string fullAutoVersion = "0.0.1-beta";
        static string tempDir = "FullAutoTemp";
        static string installDir;
        static string defaultSettingsJSON = "{\"packages\":[\"song-loader\",\"scoresaber\",\"camera-plus\", \"beatsaverdownloader\",\"songbrowserplugin\"],\"versions\":[],\"platform\":\"steam\",\"installFolder\":\"C:\\\\Program Files (x86)\\\\Steam\\\\steamapps\\\\common\\\\Beat Saber\",\"askBeforeUpdate\":false}";
        static string updateFileURL = "https://raw.githubusercontent.com/Adybo123/BeatSaberFullAuto/master/update.txt";
        // Change this when the next version is ready and pushed to master
        static int updateVersion = 1;
        static bool updating = false;

        static void Main(string[] args)
        {
            // TODO: More error handling

            Console.WriteLine($"Beat Saber FullAuto mod loader v{fullAutoVersion} by deeBo{Environment.NewLine}----");
            if (args.Length > 0)
            {
                if (args[0] == "--update") updating = true;
            }
            fullAutoMain();
            try
            {
                var beatSaberProcess = System.Diagnostics.Process.Start(Path.Combine(installDir, "Game.exe"));
                beatSaberProcess.WaitForExit();
            } catch (Exception ex)
            {
                Console.WriteLine("Couldn't find the Beat Saber executable! Does this file exist:");
                Console.WriteLine(Path.Combine(installDir, "Game.exe"));
                Console.Write("Press Any Key to Exit");
                Console.ReadKey();
            }
        }

        static string textFileContents(string filePath)
        {
            StreamReader sw = new StreamReader(filePath);
            string rS = sw.ReadToEnd();
            sw.Close();
            return rS;
        }

        static dynamic getAPIJSON(string urlToGet)
        {
            using (WebClient client = new WebClient())
            {
                Console.WriteLine($"Getting {urlToGet}");
                string urlString = client.DownloadString(urlToGet);
                return JsonConvert.DeserializeObject(urlString);
            }
        }

        static void downloadAndExtract(string zipURL)
        {
            string fullTempDir = Path.Combine(installDir, tempDir);
            if (!Directory.Exists(fullTempDir)) Directory.CreateDirectory(fullTempDir);
            // Download
            string tempFile = Path.Combine(fullTempDir, Path.GetFileName(zipURL));
            using (WebClient client = new WebClient()) client.DownloadFile(zipURL, tempFile);
            // Extract
            using (ZipFile zip = ZipFile.Read(tempFile))
            {
                foreach (ZipEntry e in zip) e.Extract(installDir, ExtractExistingFileAction.OverwriteSilently);
            }
            // Clean up
            File.Delete(tempFile);
        }

        static (bool, dynamic) checkModUpdates(dynamic settings)
        {
            Console.WriteLine("Scraping API for versions...");
            // Scrape backwards by gameVersion, and install if newer
            string gameVersionEndpoint = $"{apiURL}mods/approved/newest-by-gameversion/";
            int i = 0;
            int maxPages = 100;
            bool updatesInstalled = false;

            List<string> settingsPackages = settings.packages.ToObject<List<string>>();
            List<string> settingsVersions = settings.versions.ToObject<List<string>>();
            // Padd arrays to avoid indexing errors
            while (settingsVersions.Count < settingsPackages.Count) settingsVersions.Add("0");
            while (settingsVersions.Count > settingsPackages.Count) settingsVersions.RemoveAt(settingsVersions.Count - 1);
            List<string> remainingMods = settingsPackages;

            // For all API call pages
            while (i < maxPages)
            {
                dynamic thisPage = getAPIJSON($"{gameVersionEndpoint}{i}");
                if (i == 0) maxPages = thisPage.lastPage;
                
                // For all mods on this page
                for (int m = 0; m < thisPage.mods.Count; m++)
                {
                    dynamic mod = thisPage.mods[m];
                    string mN = mod.name;
                    if (remainingMods.Contains(mN))
                    {
                        // We don't need to check for this any more
                        // (The one closest to the top is always the newest one)
                        remainingMods = remainingMods.Where(val => val != mN).ToList<string>();
                        // Check this mod, maybe it needs an update
                        int packageID = settingsPackages.IndexOf(mN);
                        string lastVersion = settingsVersions[packageID];
                        string mV = mod.version;
                        if (lastVersion == null || mV != lastVersion)
                        {
                            Console.WriteLine($"{Environment.NewLine}Update found for {mN} - {lastVersion} to {mV}...");

                            // Do we need to ask permission?
                            // TODO
                            bool needToAsk = false;
                            if (needToAsk)
                            {
                                Console.WriteLine(" - Do you want to install this update? (y/n)");
                                Console.Write(">");
                                if (Console.ReadLine().ToLower() != "y") continue;
                            }

                            // TODO: Use the provided hashes
                            string platform = settings.platform;
                            string modURL = mod.files[platform].url;
                            downloadAndExtract(modURL);
                            settingsVersions[packageID] = mV;
                            updatesInstalled = true;
                            Console.WriteLine($" - {mN} {mV} installed successfully!");
                        }
                    }
                    if (remainingMods.Count == 0) break;
                }
                if (remainingMods.Count == 0) break;
                i++;
            }
            if (remainingMods.Count != 0)
            {
                Console.WriteLine("The following package names were not found on ModSaber:");
                for (int n = 0; n < remainingMods.Count; n++)
                {
                    Console.WriteLine($" - {remainingMods[n]}");
                }
            }
            Console.WriteLine("Update check done");
            return (updatesInstalled, settings);
        }

        static void checkForIPA()
        {
            // Download IPA if we don't have it
            if (!File.Exists(Path.Combine(installDir, "IPA.exe")))
            {
                Console.WriteLine("Didn't find IPA for injection, installing...");
                downloadAndExtract(IPAZipURL);
                Console.WriteLine("Installed IPA.");
            }
        }

        static void appendToErrorLog(string errorStr)
        {
            Console.WriteLine("The log was written to at fullAutoLog.txt");
            File.AppendAllText("fullAutoLog.txt", $"{errorStr}{Environment.NewLine}");
        }

        static void tryAutoUpdate(string installDir)
        {
            string updateString;
            try
            {
                using (WebClient client = new WebClient())
                {
                    updateString = client.DownloadString(updateFileURL);
                    string[] updatesArray = updateString.Split('|');
                    // Always make sure updatesArray[0] is int parse-able
                    int newVersion = Int32.Parse(updatesArray[0]);
                    if (newVersion > updateVersion)
                    {
                        Console.WriteLine("A new version of fullAuto is available.");
                        string updateExe = Path.Combine(installDir, "fullAutoUpdate.exe");
                        if (File.Exists(updateExe)) File.Delete(updateExe);
                        client.DownloadFile(updatesArray[1], updateExe);
                        System.Diagnostics.Process.Start(updateExe, "--update");
                        // Quit, the updated version will take over
                        Environment.Exit(0);
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine("Update check failed!");
                appendToErrorLog(ex.ToString());
            }
        }

        static void moveIfUpdating(string installDir)
        {
            if (updating)
            {
                Console.WriteLine("We're updating, so we'll copy ourselves into the correct place while deleting the old version.");
                File.Delete(Path.Combine(installDir, "Beat Saber.exe"));
                File.Copy(Path.Combine(installDir, "fullAutoUpdate.exe"), Path.Combine(installDir, "Beat Saber.exe"));
            }
        }

        static void fullAutoMain()
        {
            // Setup and checks
            try
            {
                if (!File.Exists(settingsPath))
                {
                    Console.WriteLine("Settings file was not found, reconstructing...");
                    StreamWriter settingsWriter = new StreamWriter(settingsPath);
                    // Create default settings file
                    settingsWriter.Write(defaultSettingsJSON);
                    settingsWriter.Close();
                    Console.WriteLine("Reconstructing default settings file...");
                    appendToErrorLog("fullAuto.json wasn't found, and it was reconstructed. This will cause a fail on future launches if you're not using the Steam version.");
                    appendToErrorLog("Change installDir on next start.");
                }
                dynamic settings = JsonConvert.DeserializeObject(textFileContents(settingsPath));
                installDir = settings.installFolder;
                // NEW: Auto-update
                moveIfUpdating(installDir);
                tryAutoUpdate(installDir);

                Console.WriteLine($"Install path: {installDir}");
                Console.WriteLine($"Checking for plugin updates...");

                var updates = checkModUpdates(settings);
                // If we installed updates
                if (updates.Item1)
                {
                    Console.WriteLine("Mods updated, performing IPA repatch...");
                    var IPA = new System.Diagnostics.Process();
                    IPA.StartInfo.FileName = Path.Combine(installDir, "IPA.exe");
                    IPA.StartInfo.Arguments = "Game.exe";
                    IPA.Start();
                    IPA.WaitForExit();
                    Console.WriteLine("IPA repatch performed.");

                    // Re-save to JSON file, since we updated a mod
                    Console.WriteLine("Re-saving fullAuto.json...");
                    string settingsString = JsonConvert.SerializeObject(updates.Item2);
                    StreamWriter sw = new StreamWriter(settingsPath);
                    sw.Write(settingsString);
                    sw.Close();
                    Console.WriteLine("Saved :)");
                }
            } catch (Exception ex) {
                Console.WriteLine("There was an issue updating your mods. FullAuto is starting Beat Saber anyway, but there's a log in your Beat Saber folder if you want it.");
                // Log exception
                appendToErrorLog(ex.ToString());
            }

            Console.WriteLine("Ready to launch game!");
        }
    }
}
