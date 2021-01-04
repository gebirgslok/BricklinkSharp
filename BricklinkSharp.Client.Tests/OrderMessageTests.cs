using System;
using NUnit.Framework;

namespace BricklinkSharp.Client.Tests
{
    public class OrderMessageTests
    {
        [Test]
        public void Ctor_ShouldAllowNullSubject()
        {
            var now = DateTime.Now;
            var orderMessage = new OrderMessage
            {
                Body = "Hello world",
                DateSent = now,
                From = "Foo",
                To = "Bar",
                Subject = null
            };

            Assert.AreEqual("Hello world", orderMessage.Body);
            Assert.AreEqual("Foo", orderMessage.From);
            Assert.AreEqual("Bar", orderMessage.To);
            Assert.IsTrue(DateTime.Now >= now);
            Assert.IsNull(orderMessage.Subject);
        }
    }
}
