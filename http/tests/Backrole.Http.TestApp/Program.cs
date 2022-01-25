using Backrole.Core.Abstractions;
using Backrole.Core.Builders;
using Backrole.Http.Abstractions;
using Backrole.Http.Routings;
using Backrole.Http.Routings.Abstractions;
using Backrole.Http.Routings.Results;
using Backrole.Http.StaticFiles;
using Backrole.Http.Transports.HttpSys;
using Backrole.Http.Transports.Nova;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Http.TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureHttpContainer(Builder =>
                {
                    //Builder.ConfigureTransport(Transport =>
                    //{
                    //    Transport.Clear();
                    //    Transport.UseHttpSys("http://127.0.0.1:5000/");
                    //});

                    Builder.Configure(App =>
                    {
                        App.Use(async (Http, Next) =>
                        {

                            await Next();
                        });

                        App.UseStaticFiles(Options =>
                        {
                            Options.Directory = new DirectoryInfo(".");
                            Options.DisallowPrefix(".");
                        });

                        App.UseRouter(Router =>
                        {
                            Router.Map<TestController>();
                        });
                    });
                })
                .Build()
                .RunAsync();
        }

        class MyFilter : HttpExecutionFilterAttribute
        {
            public override async Task InvokeAsync(IHttpContext Context, Func<Task> Execution)
            {
                Context.Response.Headers.Add("X-MyFilter", Priority.ToString());
                await Execution();

            }
        }

        [MyFilter(Priority = -1)] // -> execution order: 0.
        [MyFilter(Priority = int.MaxValue)] // -> execution order: 4.
        class TestController
        {
            [HttpRoute(Path = "/", Method = "GET, POST")]
            [MyFilter(Priority = 5)] // -> execution order: 3.
            [MyFilter(Priority = 1)] // -> execution order: 1.
            [MyFilter(Priority = 2)] // -> execution order: 2.
            public async Task Index(IHttpContext Http)
            {
                var WebSock = Http.Services.GetService<IHttpWebSocketFeature>();
                if (WebSock != null && WebSock.CanUpgrade)
                {
                    var WebSocket = await WebSock.UpgradeAsync();
                    while (WebSocket.State == WebSocketState.Open)
                    {
                        try
                        {
                            //var Result = await WebSocket.ReceiveAsync(Temp, default);
                            var Data = Encoding.ASCII.GetBytes("Hello");

                            await WebSocket.SendAsync(Data, WebSocketMessageType.Text, true, default);
                        }
                        catch { break; }

                        await Task.Delay(500);
                    }

                    try { await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal", default); }
                    catch { }
                    return;
                }

                Http.Response.Headers.Set("Content-Type", "text/html; charset=UTF-8");

                await Http.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(
                    "<script src='https://code.jquery.com/jquery-3.6.0.min.js'></script>" +
                    "<form method='post' enctype='multipart/form-data'><input type='file' name='hh' /><input type='submit'/></form><pre>"));

                using (var Reader = new StreamReader(Http.Request.InputStream))
                {
                    var Line = await Reader.ReadToEndAsync();
                    await Http.Response.OutputStream.WriteAsync(
                        Encoding.UTF8.GetBytes(Line));
                }
            }

            [HttpRoute(Path = "test2", Method = "GET")]
            public IHttpResult Test(IHttpContext Http)
            {
                return new RedirectResult("/", true);
            }
        }
    }
}
