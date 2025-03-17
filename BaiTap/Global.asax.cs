using Autofac;
using Autofac.Integration.Mvc;
using BaiTap.Areas.Admin.Repository;
using BaiTap.Areas.Admin.Servive;
using BaiTap.Models;
using BaiTap.Repository;
using BaiTap.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BaiTap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var builder = new ContainerBuilder();

            // Đăng ký các Controller
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            // Đăng ký testEntities
            builder.RegisterType<ShopEntities>().InstancePerRequest();

            // Đăng ký Repository và Service
            builder.RegisterType<UserRepository>().InstancePerRequest();
            builder.RegisterType<UserService>().InstancePerRequest();
            builder.RegisterType<ProductRepository>().InstancePerRequest();
            builder.RegisterType<ProductService>().InstancePerRequest();


            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

        }
    }
}
