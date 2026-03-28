using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NMDSuiteUpdater
{
    public static class GithubAPI
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public static ReleaseInfo LatestRelease { get; private set; }

        public static async Task<bool> IsUpdateAvailableAsync(string owner, string repo, string currentVersion)
        {
            var latestRelease = await GetLatestReleaseAsync(owner, repo);
            LatestRelease = latestRelease;
            return Version.Parse(latestRelease.Tag) > Version.Parse(currentVersion);
        }

        private static async Task<ReleaseInfo> GetLatestReleaseAsync(string owner, string repo)
        {
            var releasesUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            var request = new HttpRequestMessage(HttpMethod.Get, releasesUrl);
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "HttpClient");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get latest release info: {content}");
            }

            var release = JsonConvert.DeserializeObject<ReleaseInfo>(content);
            return release;
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
