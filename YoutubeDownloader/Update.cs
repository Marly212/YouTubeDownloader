using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeDownloader
{
    public class Update
    {
        static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
        public static async Task<string> UpdateYTDL()
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "Dependencies\\youtube-dl.exe",
                        Arguments = " -U",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                var startInfo = proc.StartInfo;

                proc.Start();

                return await proc.StandardOutput.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                Logger.Log.Ging("Error while updating ytdl: ");
                Logger.Log.Ging(ex.Message);
                return null;
            }
        }

        public static async Task<string> UpdateProject()
        {
            GitHubClient client = new(new ProductHeaderValue("ILoveTess"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Marly212", "PictureConverter");

            Version latestGitHubVersion = new(releases[0].TagName);

            Assembly assembly = Assembly.GetExecutingAssembly();

            string fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;

            Version localVersion = new(fileVersionInfo);

            int versionComparison = localVersion.CompareTo(latestGitHubVersion);
            if (versionComparison < 0)
            {
                //The version on GitHub is more up to date than this local release.
                Logger.Log.Ging("Update Project: Github Version is greater");

                var latestVersionURL = "https://api.github.com/repos/Marly212/PictureConverter/releases/latest";

                var request = new HttpRequestMessage
                {
                    RequestUri = new(latestVersionURL),
                    Method = HttpMethod.Get
                };

                request.Headers.Add("Accept", "application/vnd.github.v3+json");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    GithubJSONModel.Asset newVersionDownloadLink = JsonConvert.DeserializeObject<GithubJSONModel.Asset>(responseString);

                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers.Add("Accept", "application/vnd.github.v3+json");
                        webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
                        webClient.DownloadFile(newVersionDownloadLink.browser_download_url, "Release.zip");
                    }
                }
            }
            else if (versionComparison > 0)
            {
                //This local version is greater than the release version on GitHub.
                Logger.Log.Ging("Update Project: Local Version is greater. Idk how the fuck that happened");
            }
            else
            {
                //This local Version and the Version on GitHub are equal.
                Logger.Log.Ging("Update Project: Github Version is equal");
            }
            return "";
        }
    }
}
