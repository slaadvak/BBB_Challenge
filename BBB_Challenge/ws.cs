using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Sqlite;

using Microsoft.AspNetCore.Hosting;
using ThreadUtils;

namespace Web 
{
    public class Startup
    {

        public void Configure(IApplicationBuilder app)
        {
            var dbWriter = new SqliteEventWriter("GpioEvents.db");
            var res = dbWriter.ReadAllEvents();
            var sb = new StringBuilder();

            sb.Append(@"<table class=""table"">");
            foreach (var row in res)
            {
                sb.Append("<tr>\n");
                foreach (var field in row)
                    sb.Append("<td>" + field + "</td>");
                sb.Append("</tr>\n");
            }
            sb.Append(@"</table>");


            app.Run(context => context.Response.WriteAsync(sb.ToString()));
            
        }
    }

    public class wsWorker : Worker
    {
        public override void DoWork()
        {
             var host = new WebHostBuilder()
            .UseKestrel()
            .UseStartup<Startup>()
            .Build();

            host.Run();
        }
    }



}