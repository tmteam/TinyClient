using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var dto = new SomeDto
            {
                Name = "vasa",
                Surname = "rocky",
                Age = 43
            };

            var request = HttpClientRequest
                .CreateJsonPost("api/test")
                .SetContent(new JsonContent(dto));

            Stopwatch sw = new Stopwatch();


            
            var client = new HttpClient("http://demo7875676.mockable.io", keepAlive: true);

            client.Timeout = TimeSpan.FromSeconds(1);
            for (int j = 0; j < 1000; j++)
            {
                sw.Restart();
                var ans = client.SendGet("/gettest/");
                sw.Stop();
                Console.WriteLine("res: " + sw.ElapsedMilliseconds);
                Thread.Sleep(10000);
            }
            Console.ReadLine();
        }
    }
    public class SomeDto
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Surname { get; set; }
        [JsonProperty]
        public int Age { get; set; }
    }
}
