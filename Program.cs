using Microsoft.AspNetCore.Hosting;
using System.Configuration;

namespace TFWeb
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebHost host =
               new WebHostBuilder().
               UseKestrel().
               UseUrls(ConfigurationManager.AppSettings["URL"]).
               UseStartup<Startup>().Build();
            host.Run();
            System.Console.ReadLine();
        }
    }
}
