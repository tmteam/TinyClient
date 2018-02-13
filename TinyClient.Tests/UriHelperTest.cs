using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework.Internal;
using NUnit.Framework;
using TinyClient.Helpers;

namespace TinyClient.Tests
{
    [TestFixture]
    public class UriHelperTest
    {
        [TestCase("/")]
        [TestCase("/test/")]
        [TestCase("test/")]
        [TestCase("/test")]
        [TestCase("//test")]
        [TestCase("test//")]
        public void Combine_SingleValue_returnsEqualToInput(string input)
        {
            Assert.AreEqual(input, UriHelper.Combine(input));
        }


        [TestCase("/","test","/test")]
        [TestCase("/test/","/again/","/test/again/")]
        [TestCase("test/","again","test/again")]
        [TestCase("/test","again/","/test/again/")]
        [TestCase("//test","again/","//test/again/")]
        [TestCase("test//","//again","test/again")]
        public void Combine_TwoValues_returnsExpected(string input1, string input2, string expected)
        {
            Assert.AreEqual(expected, UriHelper.Combine(input1,input2));
        }


        [TestCase("/", "test", "/test","/test/test")]
        [TestCase("/test/", "/again/", "/test/again/", "/test/again/test/again/")]
        [TestCase("test/", "again", "test/again","test/again/test/again")]
        [TestCase("/test", "again/", "/test/again/","/test/again/test/again/")]
        [TestCase("//test", "again/", "//test/again/","//test/again/test/again/")]
        [TestCase("test//", "//again", "test//again","test/again/test//again")]
        public void Combine_ThreeValues_returnsExpected(string input1, string input2, string input3, string expected)
        {
            Assert.AreEqual(expected, UriHelper.Combine(input1, input2, input3));
        }

    }
}
