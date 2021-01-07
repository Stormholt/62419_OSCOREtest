using Com.AugustCellars.CoAP;
using Com.AugustCellars.CoAP.Server;
using Com.AugustCellars.CoAP.Server.Resources;
using Com.AugustCellars.CoAP.Util;
using Com.AugustCellars.CoAP.OSCOAP;
using System;
using System.IO;
using PeterO.Cbor;
using Com.AugustCellars.COSE;
using System.Linq;


namespace CoAP_.NET_
{
    class Program
    {
        private static readonly CBORObject _UsageKey = CBORObject.FromObject("usage");
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // create a new server
            CoapServer server = new CoapServer();
            server.Add(new HelloWorldResource());
            server.Start();
            string filename = null;
            Request request = new Request(Method.GET);
            SecurityContextSet set = LoadContextSet(filename);
           
            request.OscoreContext = set.All.ElementAt(0);
            request.URI = new Uri("coap://192.168.0.13/hello-world");
            request.Send();

            // wait for one response
            Response response = request.WaitForResponse();

            Console.WriteLine(Utils.ToString(response));

          
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
                    string[] usages = key[_UsageKey].AsString().Split(' ');

                    foreach (String usage in usages)
                    {
                        if (usage == "oscoap")
                        {
                            SecurityContext ctx = SecurityContext.DeriveContext(
                                key[CoseKeyParameterKeys.Octet_k].GetByteString(),
                                null,
                                key[CBORObject.FromObject("RecipID")].GetByteString(),
                                key[CBORObject.FromObject("SenderID")].GetByteString(), null,
                                key[CoseKeyKeys.Algorithm]);
                            newSet.Add(ctx);
                            break;
                        }
                        else if (usage == "oscoap-group")
                        {
                            SecurityContext ctx = SecurityContext.DeriveGroupContext(
                                key[CoseKeyParameterKeys.Octet_k].GetByteString(), key[CBORObject.FromObject(2)].GetByteString(), key[CBORObject.FromObject("SenderID")].GetByteString(),
                                null, null,
                                null, null, null, key[CoseKeyKeys.Algorithm]);
                            foreach (CBORObject recipient in key[CBORObject.FromObject("recipients")].Values)
                            {
                                ctx.AddRecipient(recipient[CBORObject.FromObject("RecipID")].GetByteString(), new OneKey(recipient[CBORObject.FromObject("sign")]));
                            }
                            newSet.Add(ctx);
                        }
                    }

                    if ((usages.Length != 1) || (usages[0] != "oscoap"))
                    {
                        keys.AddKey(key);
                    }
                }
                reader.Close();
            }

            //
            return newSet;

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

