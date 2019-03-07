using graph_tutorial.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace graph_tutorial.Controllers
{
    public class DriveController : Controller
    {
        // GET: Drive
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var model = await Models.CustomDriveItem.ScanDriveAsync();
            string fileName;
            AppDataHelper.WriteCsv(model, out fileName);
            if (Session["FileName"] == null)
                Session.Add("FileName", fileName);
            else
            {
                /* Delete previous scan file and keep last.
                 * Don't use Session_End in order to let the user do more things than keep waiting.
                 * Undeleted scans will be deleted on Application_Disposed event.
                 */
                AppDataHelper.Delete((string)Session["FileName"]);
                Session["FileName"] = fileName;
            }
            return View(model);
        }
        // GET: Drive/Download
        [Authorize]
        public async Task<ActionResult> Download(string id)
        {
            return File(AppDataHelper.Get(id), "text/csv;charset=UTF-8");
        }
    }
}