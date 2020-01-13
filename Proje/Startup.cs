using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(Proje.Startup))]

namespace Proje
{
    public class Startup
    {
        public void Configuration(IAppBuilder App)
        {
            App.MapSignalR();
        }
    }
}