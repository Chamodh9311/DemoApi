using System;
using System.Web.Http;
using DemoApi.Configuration;

namespace DemoApi
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(DemoWebAPIConfig.Register);
        }
    }
}