using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HookHandler.Api
{
    /// Composition Root / Entry point
    public class Program
    {
        /// Entry point
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// Build the application
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
