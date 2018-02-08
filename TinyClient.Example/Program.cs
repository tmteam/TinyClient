using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var client = new HttpClient("http://myHost.io");
            //Simple request:
            var received = client.PostAndReceiveJson<MyAnswerVm>("/getMyAnswer", new MyRequestVM { Name = "Bender"});
            Console.WriteLine(received);
            //need not to close connection

            //Custom request
            //request uri is http://myHost.io/getMyAnswer/?text=What+up&attributes=all
            var customRequest = HttpClientRequest
                .Create(HttpMethod.Post, "/search")
                .AddUriParam("text", "What up")
                .AddUriParam("attributes", "all")
                .AddCustomHeader("nugetPackage", "tinyClient")
                .AddCustomHeader("_SessionId", "42")
                .SetKeepAlive(true)
                .SetTimeout(TimeSpan.FromSeconds(5))
                .SetContent(new JsonContent(new MyRequestVM {Name = "Cartman"}));

            var response = client.Send(customRequest);
            var textResponse = response as HttpChannelResponse<string>;
            Console.WriteLine(textResponse.Content);
        }
    }
    public class MyAnswerVm { }
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
