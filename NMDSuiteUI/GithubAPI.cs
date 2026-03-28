using Newtonsoft.Json;
using NMDBase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NMDSuite
{
    public static class GithubAPI
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        public static ReleaseInfo LatestRelease { get; private set; }

        public static async Task<bool> IsUpdateAvailableAsync(Splash splash,string owner, string repo, string currentVersion)
        {
            var latestRelease = await GetLatestReleaseAsync(splash,owner, repo,currentVersion);
            LatestRelease = latestRelease;
            if (latestRelease.Tag == "Error")
            {
                splash.UpdateStatusText("Unable to check for Updates");
                return false;
            }
            return Version.Parse(latestRelease.Tag) > Version.Parse(currentVersion);
        }
        
        private static async Task<ReleaseInfo> GetLatestReleaseAsync( Splash splash,string owner, string repo,string currentVersion)
        {
            var releasesUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            var request = new HttpRequestMessage(HttpMethod.Get, releasesUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(repo, currentVersion));
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(+https://api.github.com/meta)"));

            // var response = await _httpClient.SendAsync(request);
            //var content = await response.Content.ReadAsStringAsync();
            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(releasesUrl))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to get latest release info");
                    }

                    var release = JsonConvert.DeserializeObject<ReleaseInfo>(await response.Content.ReadAsStringAsync());
                    _httpClient.Dispose();
                    return release;
                }
            }
            catch
            {
                _httpClient.Dispose();
                splash.UpdateStatusText("Unable to check for updates.");
                return new ReleaseInfo()
                {
                    Tag = "Error",
                };
            }
            
            

            
            
        }
    }
    


    public class ReleaseInfo
    {
        [JsonProperty("tag_name")]
        public string Tag { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("assets")]
        public List<Asset> Assets { get; set; }
    }

    public class Asset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("browser_download_url")]
        public string DownloadUrl { get; set; }
    }
}
