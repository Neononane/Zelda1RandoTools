using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace Z1RSignalRHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Limits.MinRequestBodyDataRate = null;
                    });
                    webBuilder.UseUrls("http://0.0.0.0:5000"); // listen on all interfaces
                    webBuilder.UseStartup<Startup>();
                });
    }
}
