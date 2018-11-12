﻿using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Text;
using VstsRestAPI.Viewmodel.Repository;

namespace VstsRestAPI.Git
{
    public class Repository : ApiServiceBase
    {
        public Repository(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Get Source Code from Git Hub
        /// </summary>
        /// <param name="json"></param>
        /// <param name="project"></param>
        /// <param name="repositoryID"></param>
        /// <returns></returns>
        public bool GetSourceCodeFromGitHub(string json, string project, string repositoryID)
        {
            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, _configuration.UriString + project + "/_apis/git/repositories/" + repositoryID + "/importRequests?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    return response.IsSuccessStatusCode;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }
            return false;
        }

        /// <summary>
        /// Delete the default repository
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public string GetRepositoryToDelete(string project)
        {
            GetAllRepositoriesResponse.Repositories viewModel = new GetAllRepositoriesResponse.Repositories();
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(project + "/_apis/git/repositories?api-version=" + _configuration.VersionNumber).Result;
                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<GetAllRepositoriesResponse.Repositories>().Result;
                    string repository = viewModel.value.Where(x => x.name == project).FirstOrDefault().id;
                    return repository;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }

            return string.Empty;
        }

        /// <summary>
        ///Get Default repository to delete 
        /// </summary>
        /// <param name="RepoName"></param>
        /// <returns></returns>
        public string[] GetDefaultRepository(string RepoName)
        {
            string[] repo = new string[2];
            GetAllRepositoriesResponse.Repositories viewModel = new GetAllRepositoriesResponse.Repositories();
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(RepoName + "/_apis/git/repositories?api-version=" + _configuration.VersionNumber).Result;
                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<GetAllRepositoriesResponse.Repositories>().Result;
                    if (viewModel.count > 0)
                    {
                        repo[0] = viewModel.value.FirstOrDefault().id;
                        repo[1] = viewModel.value.FirstOrDefault().name;
                    }
                    return repo;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }
            return repo;
        }

        /// <summary>
        /// Get list of Repositories
        /// </summary>
        /// <returns></returns>
        public GetAllRepositoriesResponse.Repositories GetAllRepositories()
        {
            GetAllRepositoriesResponse.Repositories viewModel = new GetAllRepositoriesResponse.Repositories();
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync("/_apis/git/repositories?api-version=" + _configuration.VersionNumber).Result;
                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<GetAllRepositoriesResponse.Repositories>().Result;
                    return viewModel;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }

            return new GetAllRepositoriesResponse.Repositories();
        }

        /// <summary>
        /// Creates Repository
        /// </summary>
        /// <param name="name"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public string[] CreateRepository(string name, string projectId)
        {
            string[] repository = new string[2];


            dynamic objJson = new System.Dynamic.ExpandoObject();
            objJson.name = name;
            objJson.project = new System.Dynamic.ExpandoObject();
            objJson.project.id = projectId;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(objJson);

            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, _configuration.UriString + "/_apis/git/repositories?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseDetails = response.Content.ReadAsStringAsync().Result;
                    JObject objResponse = JObject.Parse(responseDetails);
                    repository[0] = objResponse["id"].ToString();
                    repository[1] = objResponse["name"].ToString();
                    return repository;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }
            return repository;
        }

        /// <summary>
        /// Delete repository
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public bool DeleteRepository(string repositoryId)
        {
            using (var client = GetHttpClient())
            {
                var method = new HttpMethod("DELETE");
                var request = new HttpRequestMessage(method, _configuration.UriString + Project + "/_apis/git/repositories/" + repositoryId + "?api-version=" + _configuration.VersionNumber);
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Create Pull Request
        /// </summary>
        /// <param name="json"></param>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public string[] CreatePullRequest(string json, string repositoryId)
        {
            string[] pullRequest = new string[2];

            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, Project + "/_apis/git/repositories/" + repositoryId + "/pullRequests?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseDetails = response.Content.ReadAsStringAsync().Result;
                    JObject objResponse = JObject.Parse(responseDetails);
                    pullRequest[0] = objResponse["pullRequestId"].ToString();
                    pullRequest[1] = objResponse["title"].ToString();

                    return pullRequest;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return pullRequest;
                }
            }
        }

        /// <summary>
        /// Create Comment thread for pull request
        /// </summary>
        /// <param name="repositorId"></param>
        /// <param name="pullRequestId"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public string CreateCommentThread(string repositorId, string pullRequestId, string json)
        {
            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, Project + "/_apis/git/repositories/" + repositorId + "/pullRequests/" + pullRequestId + "/threads?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseDetails = response.Content.ReadAsStringAsync().Result;
                    JObject objResponse = JObject.Parse(responseDetails);
                    string id = objResponse["id"].ToString();
                    return id;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                    return string.Empty;
                }
            }

        }

        /// <summary>
        /// Add Comment thread
        /// </summary>
        /// <param name="repositorId"></param>
        /// <param name="pullRequestId"></param>
        /// <param name="threadId"></param>
        /// <param name="json"></param>
        public void AddCommentToThread(string repositorId, string pullRequestId, string threadId, string json)
        {
            using (var client = GetHttpClient())
            {
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");

                var request = new HttpRequestMessage(method, Project + "/_apis/git/repositories/" + repositorId + "/pullRequests/" + pullRequestId + "/threads/" + threadId + "/comments?api-version=" + _configuration.VersionNumber) { Content = jsonContent };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseDetails = response.Content.ReadAsStringAsync().Result;
                    JObject objResponse = JObject.Parse(responseDetails);
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }
            }
        }
    }
}