using graph_tutorial.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var items = await GraphHelper.GetDriveAsync();
            return View(items);
        }
    }
}