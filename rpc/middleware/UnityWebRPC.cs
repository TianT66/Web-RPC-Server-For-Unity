using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace TFramework.Web
{
    public class UnityWebRPC
    {
        private readonly RequestDelegate next;
        private const string UnityWebRPCCode = "UnityWebRPC";
        private readonly WebServiceExecutor executor;

        public UnityWebRPC(RequestDelegate next, IServiceProvider serviceProvider)
        {
            this.next = next;
            executor = new WebServiceExecutor(serviceProvider);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.HasFormContentType && UnityWebRPCCode == context.Request.Form["Channel"])
            {
                WebMessage msg = JsonConvert.DeserializeObject<WebMessage>(context.Request.Form["Data"]);
                object res = await executor.ExecuteAsync(msg, context);
                if (res != null) await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            }
            else
                await next.Invoke(context);
        }
    }
}
