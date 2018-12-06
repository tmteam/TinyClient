using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using TinyClient;
using TinyClient.Client;

namespace HttpClientChannel.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //Simple way:
            var client = new HttpClient("http://myHost.io");
            var received = client.PostAndReceiveJson<MyAnswerVm>(
                query: "getMyAnswer", 
                jsonSerializeableContent:  new MyRequestVM { Name = "Bender"});
            Console.WriteLine(received.Name);
            //need no to close connection
            
            //Fluent way:
            var answer = HttpClient
                .Create("localhost:8080")
                //Create the client
                .Build() 
                //Send and receive
                .SendJsonPost("users", new MyRequestVM {Name = "Bender"})
                //Throw if status code not in [200-299]
                .ThrowIfNot2xx()
                //Cast answer from json to MyAnswerVm or throw
                .GetJsonObject<MyAnswerVm>();

            Console.WriteLine(answer.Name);
            
            //hardcore way:
            var customClient = HttpClient
                .Create("http://myHost.io")
                .WithKeepAlive(true)
                .WithCustomDecoder(ClientEncoders.Deflate)
                .WithRequestTimeout(TimeSpan.FromSeconds(1))
                .WithRequestMiddleware((r) => r.AddCustomHeader("sentBy", "MasterOfHardcore"))
                .WithResponseMiddleware((r) => {
                    if (r.StatusCode != HttpStatusCode.OK)
                        throw new FormatException("Request failed with error: " + r.StatusCode);
                })
                .Build();
            
            //request uri is http://myHost.io/search?text=What+up&attributes=all
            var customRequest = HttpClientRequest
                .Create(HttpMethod.Post, "/search")
                .AddUriParam("text", "What up")
                .AddUriParam("attributes", "all")
                .AddCustomHeader("nugetPackage", "tinyClient")
                .AddCustomHeader("_SessionId", "42")
                .AddContentEncoder(ClientEncoders.Deflate)
                .SetKeepAlive(true)
                .SetTimeout(TimeSpan.FromSeconds(5))
                .SetContent(new JsonContent(new MyRequestVM {Name = "Cartman"}));

            var textResponse = customClient
                    .Send(customRequest) as HttpResponse<string>;

            Console.WriteLine(textResponse?.Content);
        }
    }

    public class MyAnswerVm
    {
        public string Name { get; }
    }
    public class MyRequestVM
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Surname { get; set; }
        [JsonProperty]
        public int Age { get; set; }
    }
}
