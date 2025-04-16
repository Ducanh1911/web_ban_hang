using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTap.Areas.Customer.Controllers
{
    public class RedirectController : Controller
    {
        public ActionResult ToCustomerHome()
        {
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }

}