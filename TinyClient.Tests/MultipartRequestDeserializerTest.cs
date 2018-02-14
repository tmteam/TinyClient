using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using TinyClient.Response;

namespace TinyClient.Tests
{
    //https://www.w3.org/Protocols/rfc1341/7_2_Multipart.html

    [TestFixture]
    public class MultipartRequestDeserializerTest
    {
        [Test]
        public void SingleAnswer_DeserializerIsCorrect()
        {
            var boundary = "batch-1";

            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "--batch-1--";

            var typedDeserializer = DeserializeAnswer(
                new[] { new TextResponseDeserialaizer() }, boundary, answerContent);
            Assert.AreEqual(1, typedDeserializer.Content.Length);
            Assert.AreEqual(1, typedDeserializer.Content.OfType<HttpResponse<string>>().Count());
        }
        [Test]
        public void SingleAnswer_closeBoundaryIsWrong_DeserializeThrows()
        {
            var boundary = "batch-1";
            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "--batch-1";

            var deserializer = new MultipartRequestDeserializer(new[] { new TextResponseDeserialaizer()});
            Assert.Throws<InvalidDataException>(()=>deserializer.Deserialize(GetFakeInfo(boundary), AsStream(answerContent)));

        }

        [Test]
        public void SingleAnswer_closeBoundaryMissed_DeserializeThrows()
        {
            var boundary = "batch-1";
            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                "someContent";

            var deserializer = new MultipartRequestDeserializer(new[] { new TextResponseDeserialaizer() });
            Assert.Throws<InvalidDataException>(() => deserializer.Deserialize(GetFakeInfo(boundary), AsStream(answerContent)));
        }

        [Test]
        public void AnswerIsEmpty_NoDeserializers_DeserializedIsEmpty()
        {
            var deserialized = DeserializeAnswer(new IResponseDeserializer[0], "someBoundary", "");
            Assert.IsNotNull(deserialized);
            Assert.IsEmpty(deserialized.Content);
        }

        [Test]
        public void AnswerIsEmpty_SingleDeserializer_Throws()
        {
            var deserializer = new MultipartRequestDeserializer(
                new[] { new TextResponseDeserialaizer() });

            Assert.Throws<InvalidDataException>(() => deserializer.Deserialize(GetFakeInfo("boundary"), AsStream("")));
        }



        [Test]
        public void Single204Answer_Deserialize_ResponseStatusIsCorrect()
        {
            var boundary = "batch-1";
            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "--batch-1--";

            var response =
                DeserializeAnswer(new[] { new TextResponseDeserialaizer() }, boundary, answerContent).Content[0] as
                    HttpResponse<string>;

            Assert.AreEqual(204, (int) response.StatusCode);
        }

        


        [Test]
        public void SingleAnswerWithContent_Deserialize_ResponseDataIsCorrect()
        {
            var boundary = "batch-1";
            var data = "myCustomContent";

            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                 data + 
                                "\r\n" +
                                "--batch-1--";
            //var request = HttpClientRequest
            //    .CreateGet()
            //    .SetMultipartContent(HttpClientRequest.CreateJsonPost(new object()));

            var response =
                DeserializeAnswer(new[] { new TextResponseDeserialaizer()}, boundary, answerContent).Content[0] as
                    HttpResponse<string>;

            Assert.AreEqual(data, response.Content);
        }

        [Test]
        public void TwoAnswers_Deserialize_ResponseStatusesAreCorrect()
        {
            var boundary = "batch-1";
            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 200 OK\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "--batch-1--";

            var responses = DeserializeAnswer(
                    new[]
                    {
                        new TextResponseDeserialaizer(),
                        new TextResponseDeserialaizer()
                    },
                    boundary,
                    answerContent).Content
                .OfType<HttpResponse<string>>()
                .ToArray();

            Assert.AreEqual(2, responses.Length);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(204, (int) responses[0].StatusCode);
                Assert.AreEqual(200, (int) responses[1].StatusCode);
            });
        }


        [Test]
        public void TwoAnswersWithContent_Deserialize_ResponseDatasAreCorrect()
        {
            var boundary = "batch-1";
            var data1 = "42";
            var data2 = "Foo";


            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                data1 + "\r\n" +
                                "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                data2 + "\r\n" +
                                "--batch-1--";
            var responses =
                DeserializeAnswer(
                        new[]
                        {
                            new TextResponseDeserialaizer(),
                            new TextResponseDeserialaizer()
                        },
                        boundary,
                        answerContent).Content
                    .OfType<HttpResponse<string>>()
                    .ToArray();

            Assert.AreEqual(data1, responses[0].Content);
            Assert.AreEqual(data2, responses[1].Content);
        }
        [Test]
        public void Deserialize_AnswersCountIsLessThanDeserializersCount_throws()
        {
            var boundary = "batch-1";

            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" + "\r\n" +
                                "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +"\r\n" +
                                "--batch-1--";
            var deserializer = new MultipartRequestDeserializer(new[]
            {
                new TextResponseDeserialaizer(),
                new TextResponseDeserialaizer(),
                new TextResponseDeserialaizer()
            });
            var stream = AsStream(answerContent);
            Assert.Throws<InvalidDataException>(() => deserializer.Deserialize(GetFakeInfo(boundary), stream));
        }

        [Test]
        public void Deserialize_AnswersCountIsGreaterThanDeserializesCount_throws()
        {
            var boundary = "batch-1";

            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" + "\r\n" +
                                "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" + "\r\n" +
                                "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" + "\r\n" +
                                "--batch-1--";
            var deserializer = new MultipartRequestDeserializer(new[] { new TextResponseDeserialaizer(), new TextResponseDeserialaizer() });

            Assert.Throws<InvalidDataException>(() => deserializer.Deserialize(GetFakeInfo(boundary), AsStream(answerContent)));
        }

        [Test]
        public void Deserialize_ManyAnswersAndCloseBoundaryMissed_throws()
        {
            var boundary = "batch-1";

            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" + "\r\n" +
                                "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" + "\r\n";
            var deserializer = new MultipartRequestDeserializer(
                new[] { new TextResponseDeserialaizer(), new TextResponseDeserialaizer() });

            Assert.Throws<InvalidDataException>(() => deserializer.Deserialize(GetFakeInfo(boundary), AsStream(answerContent)));
        }

        [Test]
        public void ManyAnswers_ResponseLengthIsCorrect()
        {
            var boundary = "batch-1";
            var answerContent = "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "--batch-1\r\n" +
                                "Content-Type: application/http; msgtype = response\r\n" +
                                "HTTP/1.1 204 No Content\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "--batch-1--";

            var typedDeserializer = DeserializeAnswer(new[]
            {
                new TextResponseDeserialaizer(),
                new TextResponseDeserialaizer(),
                new TextResponseDeserialaizer(),
            }, boundary, answerContent);
            Assert.AreEqual(3, typedDeserializer.Content.Length);
            Assert.AreEqual(3, typedDeserializer.Content.OfType<HttpResponse<string>>().Count());
        }


        private ResponseInfo GetFakeInfo(string boundary)
        {
            return new ResponseInfo(FakeUri, new []
            {
                new KeyValuePair<string, string>("Content-Type",$"multipart/mixed; boundary=\"{boundary}\""), 
            }, HttpStatusCode.OK);
        }


        private HttpResponse<IHttpResponse[]> DeserializeAnswer(IResponseDeserializer[] deserializers,
            string boundary, string answerContent)
        {
            var deserializer = new MultipartRequestDeserializer(deserializers);
            var stream = AsStream(answerContent);

            var deserialized = deserializer.Deserialize(GetFakeInfo(boundary), stream);

            Assert.IsNotNull(deserialized);
            Assert.IsInstanceOf<HttpResponse<IHttpResponse[]>>(deserialized);

            return deserialized as HttpResponse<IHttpResponse[]>;
        }

        private static MemoryStream AsStream(string answerContent)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(answerContent);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        private Uri FakeUri => new UriBuilder { Query = "lalal" }.Uri;


    }
}