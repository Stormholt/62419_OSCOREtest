using Com.AugustCellars.CoAP;
using Com.AugustCellars.CoAP.Server;
using Com.AugustCellars.CoAP.Server.Resources;
using Com.AugustCellars.CoAP.Util;
using Com.AugustCellars.CoAP.OSCOAP;
using System;
using System.IO;
using PeterO.Cbor;
using Com.AugustCellars.COSE;

using System.Text;

using Com.AugustCellars.CoAP.Net;

namespace CoAP_.NET_
{
    class Program
    {
        private static readonly CBORObject _UsageKey = CBORObject.FromObject("usage");
        static void Main(string[] args)
        {
            /*  Console.WriteLine("Hello World!");
              string filename = null;
              SecurityContext ctx = LoadContext(filename);
              // create a new server
              CoapServer server = new CoapServer();
              server.Add(new HelloWorldResource());
              server.SecurityContexts.Add(ctx);
              server.Start();
            //  Console.WriteLine(server.SecurityContexts.ToString());

              Request request = new Request(Method.GET);
              //  SecurityContextSet set = LoadContextSet(filename);

              request.OscoreContext = ctx;
              request.URI = new Uri("coap://192.168.0.13/hello-world");
              request.Send();

              // wait for one response
              Response response = request.WaitForResponse();

              Console.WriteLine(Utils.ToString(response));*/

            Oscoap os = new Oscoap();

            os.SetupServer();
            os.Ocoap_Get();
            os.ShutdownServer();

        }

        private static SecurityContextSet LoadContextSet(string fileName)
        {
            if (fileName == null) fileName = "ServerKeys.cbor";
            KeySet keys = new KeySet();
            SecurityContextSet newSet = new SecurityContextSet();

            FileStream fs = new FileStream(fileName, FileMode.Open);
            using (BinaryReader reader = new BinaryReader(fs))
            {
                byte[] data = reader.ReadBytes((int)fs.Length);
                CBORObject obj = CBORObject.DecodeFromBytes(data);
                for (int i = 0; i < obj.Count; i++)
                {
                    OneKey key = new OneKey(obj[i]);
                    SecurityContext ctx = SecurityContext.DeriveContext(
                               key[CoseKeyParameterKeys.Octet_k].GetByteString(),
                               null,
                               key[CBORObject.FromObject("RecipID")].GetByteString(),
                               key[CBORObject.FromObject("SenderID")].GetByteString(), null,
                               key[CoseKeyKeys.Algorithm]);
                    newSet.Add(ctx);
                }
            }
            return newSet;
        }
        private static SecurityContext LoadContext(string fileName)
        {
            if (fileName == null) fileName = "ServerKeys.cbor";
            KeySet keys = new KeySet();
            SecurityContext newSet = new SecurityContext();

            FileStream fs = new FileStream(fileName, FileMode.Open);
            using (BinaryReader reader = new BinaryReader(fs))
            {
                byte[] data = reader.ReadBytes((int)fs.Length);
                CBORObject obj = CBORObject.DecodeFromBytes(data);
              //  for (int i = 0; i < obj.Count; i++)
                //{
                    OneKey key = new OneKey(obj[0]);
                    SecurityContext ctx = SecurityContext.DeriveContext(
                               key[CoseKeyParameterKeys.Octet_k].GetByteString(),
                               null,
                               key[CBORObject.FromObject("RecipID")].GetByteString(),
                               key[CBORObject.FromObject("SenderID")].GetByteString(), null,
                               key[CoseKeyKeys.Algorithm]);
                    newSet = ctx;
               // }
            }
            return newSet;


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
              //  Console.WriteLine();
               // Console.WriteLine(Utils.ToString(r));
                // Assert.IsNotNull(r);
                // Assert.AreEqual("/abc", r.PayloadString);
            }

            private void CreateServer()
            {
                CoAPEndPoint endpoint = new CoAPEndPoint(0);
                _server = new CoapServer();

                //           _resource = new StorageResource(TARGET, CONTENT_1);
              //             _server.Add(_resource);

                Resource r2 = new HelloWorldResource();
                _server.Add(r2);

                //r2.Add(new EchoLocation("def"));

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

