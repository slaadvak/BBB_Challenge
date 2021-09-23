using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sqlite;

using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Routing;
using ThreadUtils;

namespace Web 
{
    public class Startup
    {
        public static string GetTable()
        {
            var dbWriter = new SqliteEventWriter("GpioEvents.db");
            var res = dbWriter.ReadAllEvents();
            var sb = new StringBuilder();

            sb.Append(@"<html>
                        <head>
                        <style>
                        table, th, td {
                        border: 1px solid black;
                        border-collapse: collapse;
                        }
                        </style>
                        </head>
                        <body>
                        <table class=""table"">");
            foreach (var row in res)
            {
                sb.Append("<tr>\n");
                foreach (var field in row)
                    sb.Append("<td>" + field + "</td>");
                sb.Append("</tr>\n");
            }
            sb.Append(@"</table>
                        </body>
                        </html>");
            return sb.ToString();
        }

        public void ConfigureServices(IServiceCollection services){
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseEndpointRouting();
            app.UseEndpoint();

            app.UseMvc(endpoints =>
            {
                endpoints.MapGet("/home/evt", async context =>
                {
                    await context.Response.WriteAsync(GetTable());
                });
            });

            app.Run(context => context.Response.WriteAsync("Hello from BBB!"));
        }
    }

    public class wsWorker : Worker, IDisposable
    {
        private IWebHost host;
        public override void DoWork()
        {
             host = new WebHostBuilder()
            .UseKestrel()
            .UseUrls("http://192.168.0.195:8069", "http://localhost:8069", "http://192.168.7.2:8069")
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .Build();
            
            host.Run();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && host != null)
            {
                host.Dispose();
                host = null;
            }
        }

        ~wsWorker() => Dispose(false);
    }



}