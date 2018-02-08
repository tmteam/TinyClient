using System;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using TinyClient.Helpers;

namespace TinyClient.Tests
{
    [TestFixture]
    public class JsonContentTest
    {
        [Test]
        public void CreateWithContent_WriteToStream_ContentSerializedCorrect()
        {
            var content = new SomeDto()
            {
                Name = "asdqw",
                Surname = "qqqq",
                Age = 42
            };
            var jsonRequest = new JsonContent(content);
            var request = jsonRequest.GetDataFor(FakeUri);

            var stream = new MemoryStream();
            stream.Write(request);
            stream.Position = 0;

            var reader = new StreamReader(stream);
            var deserialized = JsonHelper.Deserialize<SomeDto>(reader.ReadToEnd());
            
            content.AssertIsEqualTo(deserialized);
        }
        private Uri FakeUri => new UriBuilder{Query = "lalal"}.Uri;
    }

    public class SomeDto
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Surname { get; set; }
        [JsonProperty]
        public int Age { get; set; }

        public void AssertIsEqualTo(SomeDto dto)
        {
            NUnit.Framework.Assert.Multiple(() =>
            {
                Assert.AreEqual(Name, dto.Name);
                Assert.AreEqual(Age, dto.Age);
                Assert.AreEqual(Surname, dto.Surname);
            });
        }
    }
}
