using Backrole.Orp.Abstractions;
using Backrole.Orp.Meshes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Backrole.Orp.Tests
{
    public class Program
    {

        public static async Task Main(string[] Args)
        {
            if (Args.Length <= 0)
            {
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("please specify port numbers.");
                    return;
                }

                Args = new string[]
                {
                    "9000",
                    "9001",
                    "9002"
                };
            }

            var Options = new OrpMeshOptions();

            Options.ProtocolOptions.Epoch = DateTime.UnixEpoch;
            Options.ProtocolOptions.UseLittleEndian = true;
            Options.ProtocolOptions.IncomingQueueSize = 128;

            Options.ProtocolOptions.Map(typeof(Program).Assembly,
                X => X.GetCustomAttribute<OrpMessageAttribute>() != null);

            Options.MeshNetworkId = Encoding.ASCII.GetBytes("ORP_TEST_APP");

            Options.MaxRetriesPerPeer = 5;
            Options.ConnectionTimeout = TimeSpan.FromSeconds(30);
            Options.ConnectionRecoveryDelay = TimeSpan.FromSeconds(30);

            var Queue = new Queue<IPEndPoint>();
            foreach(var Each in Args)
            {
                Queue.Enqueue(new IPEndPoint(IPAddress.Loopback, int.Parse(Each)));
            }

            Options.Advertisement = Queue.Dequeue();
            while (Queue.TryDequeue(out var Peer))
                Options.InitialPeers.Add(Peer);

            var Mesh = new OrpMesh(Options.Advertisement, Options);

            Mesh.Peers.StateChanged += (Peers, Peer) =>
            {
                Console.WriteLine($"{Peer.RemoteEndPoint}: {Peer.State.ToString()}.");
            };

            Mesh.Start();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var Message = await Mesh.WaitAsync();
                    Console.WriteLine($"Message, {Message.TimeStamp.ToString("o")} --");
                    Console.WriteLine(JsonConvert.SerializeObject(Message.Message));
                    Console.WriteLine();
                }
            });

            while(true)
            {
                var Line = Console.ReadLine();

                await Mesh.BroadcastAsync(new Test { Message = Line });
            }
        }
    }

    [OrpMessage]
    public class Test : IOrpPackable, IOrpUnpackable
    {
        public string Message { get; set; } 

        public bool TryPack(BinaryWriter Output, IOrpReadOnlyOptions Options)
        {
            Output.Write(Message);
            return true;
        }

        public bool TryUnpack(BinaryReader Input, IOrpReadOnlyOptions Options)
        {
            Message = Input.ReadString();
            return true;
        }
    }
    
}
