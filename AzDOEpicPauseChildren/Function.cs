using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Security.KeyVault.Secrets;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System.Net;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using AzDOEpicPauseChildren.Classes;
using System.Linq;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using System.Collections.Generic;

namespace AzDOEpicPauseChildren
{
    public class Function
    {
        private readonly SecretClient _secretClient;

        public Function(SecretClient secretClient)
        {
            _secretClient = secretClient;
        }

        [FunctionName(nameof(PauseEpicChildren))]
        public async Task<IActionResult> PauseEpicChildren(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            var body = await GetBody<WebhookBody>(req);

            using (var workItemConnection = await Connect<WorkItemTrackingHttpClient>())
            {
                var epic = await workItemConnection.GetWorkItemAsync(body.resource.workItemId, expand: Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemExpand.Relations);

                if (epic.Fields["System.State"].ToString() == "New")
                {
                    foreach (var c1 in epic.Relations.Where(a => a.Rel == "System.LinkTypes.Hierarchy-Forward"))
                    {
                        var child = await workItemConnection.GetWorkItemAsync(int.Parse(c1.Url.Split("/").Last()), expand: Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemExpand.Relations);

                        foreach (var c2 in child.Relations.Where(a => a.Rel == "System.LinkTypes.Hierarchy-Forward"))
                        {
                            var id = int.Parse(c2.Url.Split("/").Last());
                            var status = await workItemConnection.GetWorkItemAsync(id, new List<string>() { "System.State" });

                            if (status.Fields["System.State"].ToString() != "Closed")
                            {
                                JsonPatchDocument c2Patch = new JsonPatchDocument();

                                c2Patch.Add(new JsonPatchOperation()
                                {
                                    Operation = Operation.Add,
                                    Path = "/fields/System.State",
                                    Value = "New"
                                });

                                await workItemConnection.UpdateWorkItemAsync(c2Patch, id);
                            }
                        }


                        if (child.Fields["System.State"].ToString() != "Closed")
                        {
                            JsonPatchDocument c1Patch = new JsonPatchDocument();

                            c1Patch.Add(new JsonPatchOperation()
                            {
                                Operation = Operation.Add,
                                Path = "/fields/System.State",
                                Value = "New"
                            });

                            await workItemConnection.UpdateWorkItemAsync(c1Patch, int.Parse(c1.Url.Split("/").Last()));
                        }
                    }
                }

            }

            return new OkResult();
        }

        public async Task<T> Connect<T>() where T : VssHttpClientBase
        {
            VssBasicCredential cred = new VssBasicCredential(new NetworkCredential("", (await _secretClient.GetSecretAsync("azure-devops-pat")).Value.Value));

            VssConnection connection = new VssConnection(new Uri(Environment.GetEnvironmentVariable("AzDOBaseUrl")), new VssCredentials(cred));

            return connection.GetClient<T>();
        }

        public async Task<T> GetBody<T>(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(requestBody);
        }
    }
}
