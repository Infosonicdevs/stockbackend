using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Stock_Backend.Controllers
{
    public class HomeController : Controller
    {
        //DbClass db = new DbClass();
        // GET: Home
        public ActionResult Index()
        {
            //db.Connect();
            return View();
        }
    }
}