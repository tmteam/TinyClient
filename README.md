
# TinyClient
Http client for .Net 4.0 framework (without bcl)
```
PM> Install-Package TinyClient
```

- No bcl
- Batching
- Fluent style


#Easy start example:

```
var client = new HttpClient("http://myHost.io");
var received = client.PostAndReceiveJson<MyAnswerVm>(
    query: "getMyAnswer", 
    jsonSerializeableContent:  new MyRequestVM { Name = "Bender"});
Console.WriteLine(received.Name);
//need no to close connection
``` 

#A little more fluent example:
``` 
var answer = client
    .Create("localhost:8080")
    .Build()
    .SendJsonPost("users", new MyRequestVM {Name = "Bender"})
    .ThrowIfFailed()
    .GetJsonObject<MyAnswerVm>();

Console.WriteLine(answer.Name);
``` 

#Masters of hardcore example:
```
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
```  
  
