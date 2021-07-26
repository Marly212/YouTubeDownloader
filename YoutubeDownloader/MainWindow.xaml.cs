using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YoutubeDownloader.Properties;

namespace YoutubeDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void txtYTURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            bDownload.IsEnabled = true;
            txtAusgabe.Text = "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.Default.Path))
            {
                txtOutputPath.Text = Settings.Default.Path;
                txtAusgabe.Text = "Pls input valid YT url";
                txtYTURL.IsEnabled = true;
            }

            if (string.IsNullOrEmpty(txtOutputPath.Text))
            {
                txtAusgabe.Text = "Pls choose a output path";
            }

            cboxFormat.Items.Add("mp3");
            cboxFormat.SelectedItem = cboxFormat.Items[0];
        }

        private void bBrowseClick(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new();

            bool? result = dialog.ShowDialog();

            if (result == false)
            {
                return;
            }
            txtAusgabe.Text = "Pls input valid YT url";
            txtYTURL.IsEnabled = true;
            txtOutputPath.Text = dialog.SelectedPath;
            Settings.Default.Path = dialog.SelectedPath;
            Settings.Default.Save();
        }

        private async void Update(object sender, RoutedEventArgs e)
        {
            try
            {
                //YoutubeDownloader.Update.UpdateProject();
                DisableAll();
                bUpdate.IsEnabled = false;
                progressBar.Visibility = Visibility.Visible;
                txtAusgabe.Text = "Updating YTDL...";

                var result = await YoutubeDownloader.Update.UpdateYTDL();

                txtAusgabe.Text = $"Finished: {result}. If the error still occurs pls contact developer";
            }
            catch (Exception ex)
            {
                Logger.Log.Ging("");
            }
            finally
            {
                EnableAll();
                progressBar.Visibility = Visibility.Hidden;
            }
        }

        private async void Download(object sender, RoutedEventArgs e)
        {
            Regex videoUrl = new("([\\s\\S]*?)&list=");
            try
            {
                string url = txtYTURL.Text;
                if (url.Contains("&list="))
                {
                    Match match = videoUrl.Match(url);
                    url = match.Groups[1].Value;
                }
                txtAusgabe.Text = "Beginn Downloading...";

                DisableAll();

                progressBar.Visibility = Visibility.Visible;

                var callback = await Download(url);

                if (callback == "Done")
                {
                    txtAusgabe.Text = "Finished";
                }

                else if (callback == "Error")
                {
                    txtAusgabe.Text += "Try the Update button";
                    bUpdate.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                Logger.Log.Ging("Error while downloading the video and converting");
            }

            finally
            {
                EnableAll();
                progressBar.Visibility = Visibility.Hidden;
            }
        }

        private async void GetFormats()
        {
            try
            {
                string inputURL = txtYTURL.Text;
            }
            catch (Exception ex)
            {
                txtAusgabe.Text = "Error while getting video formats";
                Logger.Log.Ging(ex.Message);
            }
        }

        private async Task<string> Download(string url)
        {
            try
            {
                Process proc = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "Dependencies\\youtube-dl.exe",
                        Arguments = $" -f \"bestaudio\" --extract-audio --audio-format \"mp3\" --ffmpeg-location \"Dependencies\\ffmpeg.exe\" -o \"{txtOutputPath.Text + "\\"}%(title)s.%(ext)s\" {url}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                var startInfo = proc.StartInfo;

                proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data) && e.Data.Contains("[download]"))
                    {
                        Logger.Log.Ging(e.Data);
                    }
                });

                proc.Start();

                proc.BeginOutputReadLine();

                await proc.WaitForExitAsync();

                var output = proc.StandardError.ReadToEnd();

                proc.Close();

                if (output.StartsWith("ERROR"))
                {
                    var test = 
                    txtAusgabe.Text = output;
                    return "Error";
                }
                return "Done";
            }
            catch (Exception ex)
            {
                Logger.Log.Ging("Error while downloading the video and converting");
                Logger.Log.Ging(ex.Message);
                return "null";
            }
        }

        #region Helper Methodes
        private void DisableAll()
        {
            bBrowse.IsEnabled = false;
            bDownload.IsEnabled = false;
            txtYTURL.IsEnabled = false;
        }

        private void EnableAll()
        {
            bBrowse.IsEnabled = true;
            bDownload.IsEnabled = true;
            txtYTURL.IsEnabled = true;
        }
        #endregion
    }
}
