using System;
using System.Web.Http;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;

[assembly: OwinStartup(typeof(Kentico.KInspector.WebApplication.Startup))]

namespace Kentico.KInspector.WebApplication
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Cors is needed to request localhost server from filesystem
            appBuilder.UseCors(CorsOptions.AllowAll);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var physicalFileSystem = new PhysicalFileSystem(@".\www"); //. = root, Web = your physical directory that contains all other static content, see prev step
            var options = new FileServerOptions
            {
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem
            };
            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" }; //put whatever default pages you like here
            appBuilder.UseFileServer(options);

            //appBuilder.UseStaticFiles(staticFileOptions);
            //appBuilder.UseStaticFiles("/FrontEnd");
            appBuilder.UseWebApi(config);

            //appBuilder.Run(context =>
            //{
            //    context.Response.ContentType = "text/plain";
            //    return context.Response.WriteAsync("Hello, world.");
            //});
        }
    }
}
