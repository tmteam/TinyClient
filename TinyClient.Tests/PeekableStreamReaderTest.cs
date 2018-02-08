using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using TinyClient.Response;

namespace TinyClient.Tests
{
    [TestFixture]
    public class PeekableStreamReaderTest
    {
        [Test]
        public void Dispose_originStreamDisposes()
        {
            var stream = new MemoryStream();
            var reader = new PeekableStreamReader(stream);

            reader.Dispose();

            Assert.Throws<ObjectDisposedException>(() => stream.Write(new byte[3],0,3));
        }

        [Test]
        public void ReaderIsEmpty_EndOfStreamEqualsTrue()
        {
            var reader = new PeekableStreamReader(AsStream(""));
            Assert.IsTrue(reader.EndOfStream);
        }

        [Test]
        public void ReaderIsEmpty_PeekLineReturnsNull()
        {
            var reader = new PeekableStreamReader(AsStream(""));
            Assert.IsNull(reader.PeekLine());
        }

        [Test]
        public void ReaderIsEmpty_ReadLineThrows()
        {
            var reader = new PeekableStreamReader(AsStream(""));
            Assert.IsNull(reader.PeekLine());
        }

        [Test]
        public void ReaderContainsSingleLine_ReadLine_EndOfStreamEqualsTrue()
        {
            var reader = new PeekableStreamReader(AsStream("theLine"));
            reader.ReadLine();
            Assert.IsTrue(reader.EndOfStream);
        }
        [Test]
        public void ReaderContainsSingleLine_PeekLine_EndOfStreamEqualsFalse()
        {
            var reader = new PeekableStreamReader(AsStream("theLine"));
            reader.PeekLine();
            Assert.IsFalse(reader.EndOfStream);
        }

        [Test]
        public void ReaderContainsSingleLine_PeekLineAndReadLineReturnsTheSame()
        {
            var line = "theLine";
            var reader = new PeekableStreamReader(AsStream(line));
            Assert.Multiple(() =>
            {
                Assert.AreEqual(line, reader.PeekLine());
                Assert.AreEqual(line, reader.ReadLine());
            });
        }

        [Test]
        public void TwoLines_TwoPeeksReturnsTheSame()
        {
            var line = "theLine";
            var reader = new PeekableStreamReader(AsStream(line));
            Assert.Multiple(() =>
            {
                Assert.AreEqual(line, reader.PeekLine());
                Assert.AreEqual(line, reader.PeekLine());
            });
        }
        [Test]
        public void TwoLines_PeeksAndReadsScenario()
        {
            var line1 = "theLine";
            var line2 = "theLine2";
            var reader = new PeekableStreamReader(AsStream(line1+"\r\n"+line2));
            Assert.AreEqual(line1, reader.PeekLine());
            Assert.AreEqual(line1, reader.PeekLine());
            Assert.AreEqual(line1, reader.ReadLine());

            Assert.AreEqual(line2, reader.PeekLine());
            Assert.AreEqual(line2, reader.PeekLine());
            Assert.AreEqual(line2, reader.ReadLine());

            Assert.IsTrue(reader.EndOfStream);
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

    }
}
