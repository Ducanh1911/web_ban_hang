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
        public static void SetUserId(int userId)
        {
            HttpContext.Current.Session["UserId"] = userId;
        }
        public static User GetUser()
        {
            return (User)HttpContext.Current.Session["user"];
        }
        public static int? GetUserId()
        {
            return HttpContext.Current.Session["UserId"] as int?;
        }
    }
}