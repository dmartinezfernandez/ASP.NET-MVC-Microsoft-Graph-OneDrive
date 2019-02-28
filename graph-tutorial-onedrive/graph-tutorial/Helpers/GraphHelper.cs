// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE in the project root for license information.
using graph_tutorial.TokenStorage;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace graph_tutorial.Helpers
{
    public class CustomDriveItem
    {
        public CustomDriveItem(DriveItem item)
        {
            this.Path = item.ParentReference.Path;
            this.Name = item.Name;
            this.Size = item.Size;
            if (item.Folder != null)
                this.ChildCount = item.Folder.ChildCount;
        }
        public string Path { get; }
        public string Name { get; }
        public long? Size { get; }
        public int? ChildCount { get; }
        public bool IsFolder
        {
            get { return ChildCount.HasValue; }
        }
    }
    public static class GraphHelper
    {
        // Load configuration settings from PrivateSettings.config
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];

        public static async Task<User> GetUserDetailsAsync(string accessToken)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);
                    }));

            return await graphClient.Me.Request().GetAsync();
        }

        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var events = await graphClient.Me.Events.Request()
                .Select("subject,organizer,start,end")
                .OrderBy("createdDateTime DESC")
                .GetAsync();

            return events.CurrentPage;
        }

        private static GraphServiceClient GetAuthenticatedClient()
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        // Get the signed in user's id and create a token cache
                        string signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                        SessionTokenStore tokenStore = new SessionTokenStore(signedInUserId,
                            new HttpContextWrapper(HttpContext.Current));

                        var idClient = new ConfidentialClientApplication(
                            appId, redirectUri, new ClientCredential(appSecret),
                            tokenStore.GetMsalCacheInstance(), null);

                        var accounts = await idClient.GetAccountsAsync();

                        // By calling this here, the token can be refreshed
                        // if it's expired right before the Graph call is made
                        var result = await idClient.AcquireTokenSilentAsync(
                            graphScopes.Split(' '), accounts.FirstOrDefault());

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
        }

        public static async Task<IEnumerable<CustomDriveItem>> GetDriveAsync()
        {
            GraphServiceClient graphClient = GetAuthenticatedClient();

            List<CustomDriveItem> allItems = new List<CustomDriveItem>();
            Stack<DriveItem> stack = new Stack<DriveItem>();

            IDriveItemChildrenCollectionPage page = await graphClient.Me.Drive.Root.Children.Request().GetAsync();
            while (page != null)
            {
                foreach (DriveItem item in page)
                    stack.Push(item);
                page = (page.NextPageRequest != null ? await page.NextPageRequest.GetAsync() : null);
            }

            while (stack.Count > 0)
            {
                DriveItem item = stack.Pop();
                allItems.Add(new CustomDriveItem(item));
                if (item.Folder != null)
                {
                    Debug.Print(string.Format("** {0} items...", allItems.Count));
                    Debug.Print(item.ParentReference.Path);

                    page = await graphClient.Me.Drive.Items[item.Id].Children.Request().GetAsync();
                    while (page != null)
                    {
                        foreach (DriveItem child in page)
                            stack.Push(child);
                        page = (page.NextPageRequest != null ? await page.NextPageRequest.GetAsync() : null);
                    }
                }
            }

#if DEBUG
            WriteLog(allItems);
#endif
            return allItems;
        }
        private static void WriteLog(IEnumerable<CustomDriveItem> items)
        {
            string path = HttpContext.Current.Server.MapPath(string.Format("~/App_Data/log {0:yyyy-MM-dd hh.mm.ss}.txt", DateTime.Now.ToUniversalTime()));
            using(Stream file = new FileStream(path, FileMode.Create))
            {
                TextWriter writer = new StreamWriter(file, Encoding.UTF8);
                writer.Write("Path\tName\tType\tSize\tChildren\r\n");
                foreach (CustomDriveItem item in items)
                    writer.Write(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\r\n", 
                        item.Path, 
                        item.Name, 
                        (item.IsFolder ? "Folder" : "File"), 
                        item.Size,
                        (item.IsFolder ? item.ChildCount.ToString() : string.Empty)));
            }
        }
    }
}