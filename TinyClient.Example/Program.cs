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
            
            //Custom way:
            var customClient = HttpClient
                .Create("http://myHost.io")
                .WithKeepAlive(true)
                .WithRequestTimeout(TimeSpan.FromSeconds(1))
                .WithRequestMiddleware((r) => r.AddCustomHeader("sentBy", "customHeader"))
                .WithResponseMiddleware((r) => {
                        if (r.StatusCode != HttpStatusCode.OK)
                            throw new FormatException("Request failed with error: " + r.StatusCode);
                    }
                ).Build();
            
            //request uri is http://myHost.io/search?text=What+up&attributes=all
            var customRequest = HttpClientRequest
                .Create(HttpMethod.Post, "/search")
                .AddUriParam("text", "What up")
                .AddUriParam("attributes", "all")
                .AddCustomHeader("nugetPackage", "tinyClient")
                .AddCustomHeader("_SessionId", "42")
                .SetKeepAlive(true)
                .SetTimeout(TimeSpan.FromSeconds(5))
                .SetContent(new JsonContent(new MyRequestVM {Name = "Cartman"}));

            var response = customClient.Send(customRequest);
            var textResponse = response as HttpResponse<string>;
            Console.WriteLine(textResponse.Content);
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
