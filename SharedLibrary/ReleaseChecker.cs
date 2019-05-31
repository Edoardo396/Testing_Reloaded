using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace SharedLibrary {
    public class ReleaseChecker {

        private string appName;
        private GitHubClient appClient;

     //  public Release LatestRelease => GetLatestRelease().Result;

        public ReleaseChecker(string appName) {
            this.appName = appName;
            this.appClient =
                new GitHubClient(new ProductHeaderValue(appName, Statics.Constants.APPLICATION_VERSION.ToString()));
        }

        public async Task<Release> GetLatestRelease() {
            IReadOnlyList<Release> releases;

            try {
                releases = await appClient.Repository.Release.GetAll("edofullin", "Testing_Reloaded");
            } catch (Exception e) {
                return null;
            }

            if (releases.Count == 0) return null;

            return releases.FirstOrDefault(r => r.Name.Contains("stable"));
        }


        public async Task<Version> GetLatestVersion() {
            return Version.Parse((await GetLatestRelease()).Name.Split('-')[0]);
        }

    }
}