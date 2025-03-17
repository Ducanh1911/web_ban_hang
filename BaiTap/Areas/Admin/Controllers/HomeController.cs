using BaiTap.Areas.Admin.Servive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
       
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}