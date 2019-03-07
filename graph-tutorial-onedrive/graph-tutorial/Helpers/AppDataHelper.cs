using graph_tutorial.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace graph_tutorial.Helpers
{
    public class AppDataHelper
    {
        static AppDataHelper()
        {
            Path = HttpContext.Current.Server.MapPath("~/App_Data/");
        }
        static readonly string Path;
        static string AddPath(string fileName)
        {
            return Path + fileName;
        }
        public static void WriteCsv(IEnumerable<CustomDriveItem> items, out string fileName)
        {
            fileName = Guid.NewGuid().ToString("N");
            using (System.IO.Stream file = new FileStream(AddPath(fileName), FileMode.Create))
            {
                using (System.IO.TextWriter writer = new StreamWriter(file, Encoding.UTF8))
                {
                    writer.Write("Path,Name,Type,Size,Children");
                    foreach (CustomDriveItem item in items)
                    {
                        writer.Write(string.Format("\r\n\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                            item.Path.Replace("\"", "\"\""),
                            item.Name.Replace("\"", "\"\""),
                            (item.IsFolder ? "Folder" : "File"),
                            item.Size,
                            (item.IsFolder ? item.ChildCount.ToString() : string.Empty)));
                    }
                    writer.Flush();
                }
            }
        }
        public static byte[] Get(string fileName)
        {
            using (FileStream file = File.Open(AddPath(fileName), FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[file.Length];
                file.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
        public static void Delete(string fileName)
        {
            File.Delete(AddPath(fileName));
        }
        public static void Delete()
        {
            foreach (string file in Directory.GetFiles(Path))
                File.Delete(file);
        }
    }
}