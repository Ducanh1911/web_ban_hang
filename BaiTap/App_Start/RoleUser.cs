using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BaiTap.App_Start
{
    public class RoleUser : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var User = SessionConfig.GetUser();
            if (User == null)
            {
                // Điều hướng về trang đăng nhập
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "User",
                        action = "Login",
                        area = "",
                    }));
                return;
            }

            return;
        }

    }
}