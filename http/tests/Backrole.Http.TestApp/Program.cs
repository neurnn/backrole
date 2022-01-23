using Backrole.Core.Abstractions;
using Backrole.Core.Builders;
using Backrole.Http.Abstractions;
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
                    Builder.Configure(App =>
                    {
                        App.Use(async (Http, Next) =>
                        {
                            var WebSock = Http.Services.GetService<IHttpWebSocketFeature>();
                            if (WebSock != null && WebSock.CanUpgrade)
                            {
                                var WebSocket = await WebSock.UpgradeAsync();
                                var Temp = new byte[2048];

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

                            Http.Response.Status = 200;
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
                            await Next();
                        });
                    });
                })
                .Build()
                .RunAsync();
        }
    }
}
