﻿using CrossWordGameServerProject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CrossWordGameServer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private const string SQL_DATABASE_PATH = @"~\databases";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DatabaseHelper.SetupDatabase(SQL_DATABASE_PATH);
        }

        protected void Application_End()
        {
            DatabaseHelper.ShutdownDatabase();
        }
    }
}
