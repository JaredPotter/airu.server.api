using System;
using Microsoft.Owin;
using Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using server_api.App_Start;
using Swashbuckle.Application;

[assembly: OwinStartup(typeof(server_api.App_Start.Startup))]

namespace server_api.App_Start
{
    /// <summary>
    /// *xml comment*
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// *xml comment*
        /// </summary>
        /// <param name="app">*xml comment*</param>
        public void Configuration(IAppBuilder app)
        {

            HttpConfiguration config = new HttpConfiguration();

            WebApiConfig.Register(config);

            config.EnableSwagger(c => {
                c.SingleApiVersion("v1", "server_api");
                c.IncludeXmlComments(GetXmlCommentsPath());

            } ).EnableSwaggerUi();

           //Swashbuckle.Bootstrapper.Init(config);

            app.UseWebApi(config);
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }

        /// <summary>
        /// *xml comment*
        /// </summary>
        /// <returns></returns>
        protected static string GetXmlCommentsPath()
        {
            return System.String.Format(@"{0}\bin\server_api.XML", System.AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
