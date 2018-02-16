
# TinyClient
Http client for .Net 4.0 framework (without bcl)
```
PM> Install-Package TinyClient
```

- No bcl
- Batching
- Fluent style


# Easy start example:

```
var client = new HttpClient("http://myHost.io");
var received = client.PostAndReceiveJson<MyAnswerVm>(
    query: "getMyAnswer", 
    jsonSerializeableContent:  new MyRequestVM { Name = "Bender"});
Console.WriteLine(received.Name);
//need no to close connection
``` 

# A little more fluent example:
``` 
var answer = client
    .Create("localhost:8080")
    .Build()
    .SendJsonPost("users", new MyRequestVM {Name = "Bender"})
    .ThrowIfFailed()
    .GetJsonObject<MyAnswerVm>();

Console.WriteLine(answer.Name);
``` 

# Masters of hardcore example:
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
  
# Exception handling:

Request-response process produces different results:

1) Response is succesfully ([200-299] http codes)
2) Server error: (non succesfully http codes)
	- You can catch an exception here 
	- Or do you want to get response with bad status in usual flow?
3) Serialization or deserialization error
4) Connection errors (host not found, invalid url etc...)
5) Tiny client timeout error

## Exceptional way
```
try
{
	var response = client
		//throws WebException (connection errors) or TinyTimeoutException:WebException
		.SendGet("users")
		//throws TinyWebException: WebException (status errors)
		.ThrowIfFailed()
		//throws InvalidDataException 
		.GetJsonObject<MyAnswerVm>();
		
	//handle the response	
}

catch(WebException e){
	if (e is TinyHttpException){
		var errorResponse = e.HttpResponse;
		//Do something with errorResponse
	}
	else if(e is TinyTimeoutException){
		//...
	}
	else{
		//Do something with connection problems
	}
}
catch(InvalidDataException e) {
	//omg, the server is going crazy!
}
````

## Error code way

```
try
{
	var response = client
		//throws WebException (connection errors) or TimeoutException
		.SendGet("users");
		
	if (response.IsSuccesfully()) {
		//handle the response
	} else {
		//handle bad status code
	}
}
catch(Exception e){
		//...
}
```  
