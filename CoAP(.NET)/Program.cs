using Com.AugustCellars.CoAP;
using Com.AugustCellars.CoAP.Server;
using Com.AugustCellars.CoAP.Server.Resources;
using Com.AugustCellars.CoAP.OSCOAP;
using System.Text;
using Com.AugustCellars.CoAP.Net;

//Based on test of OSCOAP from :https://github.com/jimsch/CoAP-CSharp

namespace CoAP_.NET_
{
    class Program
    {

        static void Main(string[] args)
        {


            Oscoap os = new Oscoap();

            os.SetupServer();
            os.Ocoap_Get();
            os.ShutdownServer();

        }


        public class Oscoap
        {
            int _serverPort;
            CoapServer _server;
            private static readonly byte[] clientId = Encoding.UTF8.GetBytes("client");
            private static readonly byte[] serverId = Encoding.UTF8.GetBytes("server");
            private static readonly byte[] secret = new byte[] { 01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23 };

            public void SetupServer()
            {

                CreateServer();
            }

            public void ShutdownServer()
            {
                _server.Dispose();
            }

            public void Ocoap_Get()
            {
                CoapClient client = new CoapClient($"coap://localhost:{_serverPort}/hello-world")
                {
                    OscoreContext = SecurityContext.DeriveContext(secret, null, clientId, serverId)
                };
                Response r = client.Get();
            }

            private void CreateServer()
            {
                CoAPEndPoint endpoint = new CoAPEndPoint(0);
                _server = new CoapServer();

                Resource r2 = new HelloWorldResource();
                _server.Add(r2);

                _server.AddEndPoint(endpoint);
                _server.Start();
                _serverPort = ((System.Net.IPEndPoint)endpoint.LocalEndPoint).Port;

                SecurityContextSet oscoapContexts = new SecurityContextSet();
                _server.SecurityContexts.Add(SecurityContext.DeriveContext(secret, null, serverId, clientId));
            }
        }

            class HelloWorldResource : Resource
        {
            public HelloWorldResource()
                : base("hello-world")
            {
                Attributes.Title = "GET a friendly greeting!";
            }

            protected override void DoGet(CoapExchange exchange)
            {
                exchange.Respond("Hello World from CoAP.NET!");
            }
        }
    }
}

