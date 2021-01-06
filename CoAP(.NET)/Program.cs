using System;
using CoAP;
using CoAP.Server;
using CoAP.Server.Resources;

namespace CoAP_.NET_
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // create a new server
            var server = new CoapServer();

            // add the resource to share
            server.Add(new HelloWorldResource());

            // let the server fly
            server.Start();

            // create a new client
            var client = new CoapClient();

            // set the Uri to visit
            client.Uri = new Uri("coap://192.168.43.61/helloworld");

            // now send a GET request to say hello~
            var response = client.Get();

            Console.WriteLine(response.PayloadString);  // Hello World!
        }
    }
    class HelloWorldResource : Resource
    {
        // use "helloworld" as the path of this resource
        public HelloWorldResource() : base("helloworld")
        {
            // set a friendly title
            Attributes.Title = "GET a friendly greeting!";
        }

        // override this method to handle GET requests
        protected override void DoGet(CoapExchange exchange)
        {
            // now we get a request, respond it
            exchange.Respond("Hello World!");
        }
    }
}
