using BaiTap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaiTap.App_Start
{
    public static class SessionConfig
    {
        public static void SetUser(User user)
        {
            HttpContext.Current.Session["user"] = user;
        }
        public static User GetUser()
        {
            return (User)HttpContext.Current.Session["user"];
        }
    }
}