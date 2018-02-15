using System.IO;
using System.Text;
using NUnit.Framework;
using TinyClient.Helpers;
using TinyClient.Response;

namespace TinyClient.Tests
{
    [TestFixture]
    public class BatchParseHelperTest
    {
        [TestCase("\r\n{0}")]
        [TestCase("\r\n\r\n{0}")]
        [TestCase("\r\n\r\n{0}\r\n")]
        [TestCase("\r\n  \r\n \r\n\r\n{0}")]
        [TestCase("{0}")]
        [TestCase("{0}\r\n")]
        [TestCase("{0}\r\nSomeOtherString")]
        [TestCase(" \r\n{0}\r\nSomeOtherString")]
        public void GetFirstNonEmptyLine_StringExists_returnsString(string template)
        {
            string val = "foo";
            var searched = GetReaderFor(string.Format(template, val)).ReadFirstNonEmptyLine();
            Assert.AreEqual(val, searched);
        }
        [TestCase("\r\n")]
        [TestCase(" \r\n")]
        [TestCase("")]
        [TestCase("\r\n ")]
        public void GetFirstNonEmptyLine_StringNotExists_returnsNull(string str)
        {
            var searched = GetReaderFor(str).ReadFirstNonEmptyLine();
            Assert.IsNull(searched);
        }

        [TestCase("HTTP/1.1 200 OK",200)]
        [TestCase("HTTP/1.1 200 some words", 200)]
        [TestCase(" HTTP/1.1 200 some words", 200)]
        public void GetResultCodeOrThrow_StringIsCorrect_SpecifiedCodeFound(string str, int code)
        {
            var actualCode = BatchSerializeHelper.GetResultCodeOrThrow(str);
            Assert.AreEqual(code, actualCode);
        }

        [TestCase("ATTP/1.1 200 OK")]
        [TestCase("Bttp 200 some words")]
        [TestCase("HTTP/1.1 some words 200")]
        public void GetResultCodeOrThrow_StringIsInvalid_Throws(string str)
        {
            Assert.Throws<InvalidDataException>(()=> BatchSerializeHelper.GetResultCodeOrThrow(str));
        }

        [TestCase("","")]
        [TestCase("", "lalala")]
        [TestCase("input1", "")]
        [TestCase("before", "after")]
        [TestCase("before1\r\nbefore2", "")]
        [TestCase("before1\r\nbefore2", "after1\r\nafter2")]
        public void ReadUntilBoundary_boundaryIsNotLast_ResultsAreCorrect(string beforeBoundary, string afterBoundary)
        {
            string boundary = "theBoundary";
            var text = $"{beforeBoundary}\r\n--{boundary}\r\n{afterBoundary}";
            var actual = BatchSerializeHelper.ReadUntilBoundaryOrThrow(GetReaderFor(text), boundary);
            Assert.AreEqual(beforeBoundary, actual);
        }
        [Test]
        public void ReadUntilBoundary_boundaryIsLast_ResultsAreCorrect()
        {
            string boundary = "theBoundary";
            string content = "before1\r\nbefore2";
            var text = $"{content}\r\n--{boundary}--\r\nsome string after";
            var actual = BatchSerializeHelper.ReadUntilBoundaryOrThrow(GetReaderFor(text), boundary);
            Assert.AreEqual(content, actual);
        }

        [Test]
        public void ReadUntilBoundary_boundaryNotExist_throws()
        {
            string content = "before1\r\nbefore2";

            Assert.Throws<InvalidDataException>(
                ()=> BatchSerializeHelper.ReadUntilBoundaryOrThrow(GetReaderFor(content),"someBoundary"));
        }

        [TestCase(" multipart/mixed; boundary=\"myCustomBoundary\"", "myCustomBoundary")]
        [TestCase("multipart/mixed;  boundary = \"myCustomBoundary\"", "myCustomBoundary")]
        public void GetBoundaryStringOrThrow_SearchesForSpecifiedBoundary(string contentTypeString, string boundary)
        {
            var actual = BatchSerializeHelper.GetBoundaryStringOrThrow(contentTypeString);
            Assert.AreEqual(boundary, actual);
        }

        [TestCase(" multipart/mAxed; boundary=\"myCustomBoundary\"")]
        [TestCase("multipart/mixed;  = \"myCustomBoundary\"")]
        [TestCase("multipart/mixed;  ")]
        [TestCase(" boundary=\"myCustomBoundary\"")]
        [TestCase("boundary=\"myCustomBoundary\"")]
        [TestCase("multipart/mixed;")]
        public void GetBoundaryStringOrThrow_ContentTypeStringIsInvalid_Throws(string contentTypeString)
        {
            Assert.Throws<InvalidDataException>(()=> BatchSerializeHelper.GetBoundaryStringOrThrow(contentTypeString));
        }

        private PeekableStreamReader GetReaderFor(string content)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            stream.Position = 0;
            return new PeekableStreamReader( new StreamReader(stream, Encoding.UTF8));
        }
    }
}
