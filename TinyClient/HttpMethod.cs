namespace TinyClient
{
    public class HttpMethod
    {
        public static HttpMethod Get { get; } = new HttpMethod("GET");
        public static HttpMethod Put { get; } = new HttpMethod("PUT");
        public static HttpMethod Post { get; } = new HttpMethod("POST");
        public static HttpMethod Delete { get; } = new HttpMethod("DELETE");
        public static HttpMethod Head { get; } = new HttpMethod("HEAD");

        private HttpMethod(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override string ToString() => Name;
    }
}