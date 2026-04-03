//using Ionic.Zip;
using Newtonsoft.Json;
using NMDSuite;
using NMDSuiteUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace NMDBase
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        private const string RepoOwner = "LogzatioStudios150";
        private const string Repo = "NMDSuite";
        private readonly string _currentVersion;
        MainWindow main = new MainWindow();
        HttpClient httpClient = new HttpClient();
        private string _tempFilePath;
        private ReleaseInfo _releaseInfo;
        public static string tempFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update");
        public static string? fileToParse;
        public static bool keepeye = false;
        private string[]? fileList;
        private string? exportDirectory;



        //public Splash(string? fileToParse, string[]? fileList, string? exportDirectory)
        //{
        //    this.fileToParse = fileToParse;
        //    this.fileList = fileList;
        //    this.exportDirectory = exportDirectory;
        //}

        public Splash()
        {
            InitializeComponent();
            UpdatePanel.IsVisibleChanged += UpdatePanel_IsVisibleChanged;
            _currentVersion = "1.1.1";
            if (Directory.Exists(tempFolderPath))
            {
                Directory.Delete(tempFolderPath, true);
            }
            UpdateStatusText("Checking for updates...");
            

            
        }


        private async void CheckForUpdates()
        {
            Trace.WriteLine("Checking for Updates");
            var isUpdateAvailable = await GithubAPI.IsUpdateAvailableAsync(this,RepoOwner, Repo, _currentVersion);
            if (isUpdateAvailable)
            {
                UpdateStatusText($"Update available: v{GithubAPI.LatestRelease.Tag}");
                ShowUpdateNotification();
            }
            else
            {
                if(GithubAPI.LatestRelease.Tag != "Error")
                {
                    UpdateStatusText("Up to date");
                    await Task.Delay(500);
                    if (fileToParse != null)
                    {
                        if (keepeye)
                        {
                            UpdateStatusText($"Opening {Path.GetFileName(fileToParse)} for editing");
                        }
                        else
                        {
                            UpdateStatusText($"Opening {Path.GetFileName(fileToParse)} for editing and removing eye bones");
                        }
                    }
                    

                }
                await Task.Delay(3000);
                Close();
                ShowMainApplication();
            }
        }
        private void ShowUpdateNotification()
        {
            Info_Title.Text = $"Update Available! - v{GithubAPI.LatestRelease.Tag}";
            Body.Markdown = GithubAPI.LatestRelease.Body;
            UpdatePanel.Visibility = Visibility.Visible;
        }
        public void UpdateStatusText(string text)
        {
            StatusText.Dispatcher.Invoke(() =>
            {
                StatusText.Text = text;
            });
        }
        private void ShowMainApplication()
        {
            main.Title = $"NMDSuite v{_currentVersion}";
            main.Show();
        }
        private async Task DownloadUpdateAsync(string downloadUrl)
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"{GithubAPI.LatestRelease.Tag}.zip");
            using (var httpClient = new HttpClient())
            {
                using (var httpResponse = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    httpResponse.EnsureSuccessStatusCode();

                    var contentLength = httpResponse.Content.Headers.ContentLength ?? -1L;
                    var buffer = new byte[81920];
                    long totalBytesRead = 0;
                    int bytesRead;
                    string status = "Downloading...";

                    using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(_tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                    {
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            var progressPercentage = (int)((double)totalBytesRead / contentLength * 100);
                            status = $"Downloading... {progressPercentage}%";
                            UpdateStatusText(status);
                        }
                    }
                    Thread.Sleep(2000);
                    UpdateStatusText("Extracting...");
                    string extractPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";
                    await Task.Run(() => ExtractUpdate(_tempFilePath));
                    UpdateStatusText("Update extracted successfully!, restarting...");
                    Thread.Sleep(2000);
                    await Task.Run(() => LaunchNewVersion());

                }
            }
        }


        private static void ExtractUpdate(string zipPath)
        {
            try
            {
                //string _tempFolderPath = tempFolderPath;
                string extractToPath = AppDomain.CurrentDomain.BaseDirectory;

                Directory.CreateDirectory(tempFolderPath);
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string fullExtractPath = Path.Combine(tempFolderPath, entry.FullName);
                        string fullFilePath = Path.Combine(extractToPath, entry.FullName);

                        // Create directory if it doesn't exist
                        if (Path.GetFileName(fullExtractPath).Length == 0)
                        {
                            Directory.CreateDirectory(fullExtractPath);
                        }

                        if (File.Exists(fullExtractPath))
                        {
                            File.Delete(fullExtractPath);
                        }

                        if (entry.Length == 0)
                        {
                            continue;
                        }

                        entry.ExtractToFile(fullExtractPath, true);
                    }
                    
                }

                // Start a new process to move the files after the application closes
                string moveCommand = $"/C choice /C Y /N /D Y /T 3 & move \"{tempFolderPath}\\*\" \"{extractToPath}\" & start \"\" \"{extractToPath}\\NMDSuite.exe\"";
                ProcessStartInfo cmdMov = new ProcessStartInfo();
                cmdMov.FileName = "cmd.exe";
                cmdMov.Arguments = moveCommand;
                cmdMov.CreateNoWindow = true;
                cmdMov.UseShellExecute = false;
                Process.Start(cmdMov);
               
                //Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting update: " + ex.Message);
            }
        }


        private void LaunchNewVersion()
        {
            string currentPath = Process.GetCurrentProcess().MainModule.FileName;
           // Process.Start(currentPath);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        private void RenameCurrentExecutable()
        {
            string currentPath = Process.GetCurrentProcess().MainModule.FileName;
            string backupPath = Path.Combine(Path.GetDirectoryName(currentPath), "NMDSuite_BACKUP.exe");

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            File.Move(currentPath, backupPath);
        }


        private void UpdatePanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private async void Grid_Initialized(object sender, EventArgs e)
        {
            
            
        }


        
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TitleText.Text = $"NMDSuite v{_currentVersion}";
            StatusText.Text = "Checking for updates...";

            CheckForUpdates();

        }


        private void UpdateSkip_Click(object sender, RoutedEventArgs e)
        {
            UpdatePanel.Visibility=Visibility.Collapsed;
            Close();
            ShowMainApplication();
        }

        private async void UpdateConfirm_Click(object sender, RoutedEventArgs e)
        {
            UpdatePanel.Visibility = Visibility.Collapsed;
            if(GithubAPI.LatestRelease.Assets.FirstOrDefault().DownloadUrl != null)
            {
                await Task.Run(() => DownloadUpdateAsync(GithubAPI.LatestRelease.Assets.FirstOrDefault().DownloadUrl));
            }
            
        }
    }
}


