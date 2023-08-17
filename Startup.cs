using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using System.Configuration;
using System.IO;
using TFramework.Web;

namespace TFWeb
{
    class Startup
    {
        private string GetBrowserFileProvider()
        {
            string path = ConfigurationManager.AppSettings["BrowserFile"];
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            return path;
        }

        private string GetStaticFileProvider()
        {
            string path = ConfigurationManager.AppSettings["StaticFile"];
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            return path;
        }

        public void Configure(IApplicationBuilder app)
        {
            DirectoryBrowserOptions dbo = new DirectoryBrowserOptions { FileProvider = new PhysicalFileProvider(GetBrowserFileProvider()) };
            app.UseDirectoryBrowser(dbo);

            StaticFileOptions sfo = new StaticFileOptions { FileProvider = new PhysicalFileProvider(GetStaticFileProvider()) };
            sfo.DefaultContentType = "application/x-msdownload";
            sfo.ServeUnknownFileTypes = true;
            sfo.ContentTypeProvider = new FileExtensionContentTypeProvider();
            app.UseStaticFiles(sfo);

            app.UseMiddleware<UnityWebRPC>();
            app.Run((content) => Task.CompletedTask);
        }
    }
}
