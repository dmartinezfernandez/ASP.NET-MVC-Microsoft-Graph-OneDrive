using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace graph_tutorial.Models
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
        public static async Task<IEnumerable<CustomDriveItem>> ScanDriveAsync()
        {
            GraphServiceClient graphClient = Helpers.GraphHelper.GetAuthenticatedClient();

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
            return allItems;
        }
    }
}