#region License
// Copyright (c) 2020 Jens Eisenbach
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using BricklinkSharp.Client.Json;
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class EndpointWireReadTests
{
    private static readonly Uri Url = new("https://api.bricklink.com/api/store/v1/items/part/3001");

    private static Endpoint<T> Get<T>() => Endpoint<T>.Get(ApiResourceType.Item, "items/part/3001");

    private static Endpoint<T> Create<T>(HttpVerb verb) => verb switch
    {
        HttpVerb.Get => Endpoint<T>.Get(ApiResourceType.Item, "items"),
        HttpVerb.Post => Endpoint<T>.Post(ApiResourceType.Item, "items"),
        HttpVerb.Put => Endpoint<T>.Put(ApiResourceType.Item, "items"),
        _ => Endpoint<T>.Delete(ApiResourceType.Item, "items")
    };

    private static T Read<T>(Endpoint<T> endpoint, string body, int httpStatusCode = 200) =>
        EndpointWire.ReadResponse(endpoint, Url, HttpMethod.Get, httpStatusCode, body);

    [Test]
    public void Should_ThrowInvalidResponse_When_BodyIsNotJson()
    {
        var exception = Assert.Throws<BricklinkInvalidResponseException>(
            () => Read(Get<Widget>(), "<html>Too many requests</html>", 429))!;

        ClassicAssert.IsInstanceOf<BricklinkException>(exception);
        ClassicAssert.AreEqual(429, exception.HttpStatusCode);
        StringAssert.Contains("Too many requests", exception.Message);
        ClassicAssert.IsNotNull(exception.InnerException);
    }

    [TestCase("")]
    [TestCase("[]")]
    [TestCase("{\"data\":{}}")]
    [TestCase("{\"meta\":\"x\"}")]
    [TestCase("{\"meta\":{\"code\":\"x\"}}")]
    public void Should_ThrowInvalidResponse_When_MetaIsMissingOrMalformed(string body)
    {
        Assert.Throws<BricklinkInvalidResponseException>(() => Read(Get<Widget>(), body));
    }

    [Test]
    public void Should_ThrowHttpError_When_MetaCodeDiffersFromExpected()
    {
        var exception = Assert.Throws<BricklinkHttpErrorException>(() => Read(Get<Widget>(), Fixtures.Envelope(404)))!;

        ClassicAssert.AreEqual(404, exception.ReceivedStatusCode);
        ClassicAssert.AreEqual(200, exception.ExpectedStatusCode);
        ClassicAssert.AreEqual("d", exception.Description);
        ClassicAssert.AreEqual("m", exception.RawMessage);
        ClassicAssert.AreEqual(Url.AbsoluteUri, exception.RequestUrl);
        ClassicAssert.AreEqual(HttpMethod.Get, exception.HttpMethod);
    }

    [Test]
    public void Should_MapObject_When_DataIsObject()
    {
        var widget = Read(Get<Widget>(), Fixtures.Envelope(200, "{\"name\":\"brick\",\"size\":3}"));

        ClassicAssert.AreEqual("brick", widget.Name);
        ClassicAssert.AreEqual(3, widget.Size);
    }

    [TestCase(null)]
    [TestCase("null")]
    [TestCase("{}")]
    public void Should_ThrowNoDataReceived_When_ObjectExpectedButDataIsEmpty(string? data)
    {
        Assert.Throws<BricklinkNoDataReceivedException>(() => Read(Get<Widget>(), Fixtures.Envelope(200, data)));
    }

    [TestCase("\"text\"")]
    [TestCase("42")]
    [TestCase("[1]")]
    public void Should_ThrowUnexpectedDataKind_When_ObjectExpectedButDataIsNotObject(string data)
    {
        var exception = Assert.Throws<BricklinkUnexpectedDataKindException>(
            () => Read(Get<Widget>(), Fixtures.Envelope(200, data)))!;

        ClassicAssert.AreEqual("Object", exception.ExpectedDataKind);
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_DataCannotBeMappedToType()
    {
        Assert.Throws<BricklinkInvalidResponseException>(
            () => Read(Get<Widget>(), Fixtures.Envelope(200, "{\"size\":\"not a number\"}")));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_ConverterRejectsValue()
    {
        Assert.Throws<BricklinkInvalidResponseException>(
            () => Read(Get<TypedWidget>(), Fixtures.Envelope(200, "{\"type\":\"BOGUS\"}")));
    }

    [Test]
    public void Should_MapArray_When_DataIsArray()
    {
        var widgets = Read(Get<Widget[]>(), Fixtures.Envelope(200, "[{\"name\":\"a\"},{\"name\":\"b\"}]"));

        CollectionAssert.AreEqual(new[] { "a", "b" }, widgets.Select(widget => widget.Name));
    }

    [Test]
    public void Should_MapList_When_ResultTypeIsList()
    {
        var widgets = Read(Get<List<Widget>>(), Fixtures.Envelope(200, "[{\"name\":\"a\"}]"));

        ClassicAssert.AreEqual(1, widgets.Count);
    }

    [Test]
    public void Should_ReturnEmptyArray_When_ArrayTypeAndDataIsEmptyArray()
    {
        ClassicAssert.IsEmpty(Read(Get<Widget[]>(), Fixtures.Envelope(200, "[]")));
    }

    [Test]
    public void Should_ThrowNoDataReceived_When_NonEmptyArrayRequiredButDataIsEmptyArray()
    {
        var endpoint = Endpoint<Widget[]>.Get(ApiResourceType.Item, "items/part/3001", ResponseShape.ArrayNonEmpty);

        Assert.Throws<BricklinkNoDataReceivedException>(() => Read(endpoint, Fixtures.Envelope(200, "[]")));
    }

    [Test]
    public void Should_ThrowUnexpectedDataKind_When_ArrayExpectedButDataIsObject()
    {
        var exception = Assert.Throws<BricklinkUnexpectedDataKindException>(
            () => Read(Get<Widget[]>(), Fixtures.Envelope(200, "{\"name\":\"x\"}")))!;

        ClassicAssert.AreEqual("Array", exception.ExpectedDataKind);
    }

    [Test]
    public void Should_ThrowNoDataReceived_When_ArrayExpectedButDataIsAbsent()
    {
        Assert.Throws<BricklinkNoDataReceivedException>(() => Read(Get<Widget[]>(), Fixtures.Envelope(200)));
    }

    [TestCase(null)]
    [TestCase("null")]
    [TestCase("{}")]
    public void Should_Succeed_When_NoDataExpectedAndDataIsEmpty(string? data)
    {
        var endpoint = Endpoint<NoData>.Delete(ApiResourceType.Inventory, "inventories/1");

        Assert.DoesNotThrow(() => Read(endpoint, Fixtures.Envelope(204, data), 204));
    }

    [Test]
    public void Should_ThrowEmptyDataExpected_When_NoDataExpectedButObjectHasContent()
    {
        var endpoint = Endpoint<NoData>.Delete(ApiResourceType.Inventory, "inventories/1");

        Assert.Throws<BricklinkEmptyDataExpectedException>(
            () => Read(endpoint, Fixtures.Envelope(204, "{\"name\":\"x\"}"), 204));
    }

    [Test]
    public void Should_ThrowUnexpectedDataKind_When_NoDataExpectedButDataIsArray()
    {
        var endpoint = Endpoint<NoData>.Delete(ApiResourceType.Inventory, "inventories/1");

        Assert.Throws<BricklinkUnexpectedDataKindException>(
            () => Read(endpoint, Fixtures.Envelope(204, "[1]"), 204));
    }

    [TestCase(HttpVerb.Get, 200)]
    [TestCase(HttpVerb.Put, 200)]
    [TestCase(HttpVerb.Post, 201)]
    public void Should_AcceptDefaultMetaCodeForVerb_When_ObjectExpected(HttpVerb verb, int code)
    {
        var widget = Read(Create<Widget>(verb), Fixtures.Envelope(code, "{\"name\":\"x\"}"));

        ClassicAssert.AreEqual("x", widget.Name);
    }

    [TestCase(HttpVerb.Put, 204)]
    [TestCase(HttpVerb.Delete, 204)]
    [TestCase(HttpVerb.Post, 201)]
    public void Should_AcceptDefaultMetaCodeForVerb_When_NoDataExpected(HttpVerb verb, int code)
    {
        Assert.DoesNotThrow(() => Read(Create<NoData>(verb), Fixtures.Envelope(code)));
    }

    [Test]
    public void Should_UseExplicitMetaCode_When_ExpectedMetaCodeIsSet()
    {
        var endpoint = Endpoint<Widget>.Get(ApiResourceType.Item, "items/part/3001", expectedMetaCode: 202);

        ClassicAssert.AreEqual("x", Read(endpoint, Fixtures.Envelope(202, "{\"name\":\"x\"}")).Name);
    }

    [Test]
    public void Should_ThrowInvalidOperation_When_NoDataShapeUsedWithOtherResultType()
    {
        var endpoint = Endpoint<Widget>.Get(ApiResourceType.Item, "items/part/3001", ResponseShape.NoData);

        Assert.Throws<InvalidOperationException>(() => Read(endpoint, Fixtures.Envelope(200)));
    }

    [Test]
    public void Should_ApplyMapAfterReading_When_EndpointIsMapped()
    {
        var endpoint = Get<Widget>().Map(widget => widget.Name!.ToUpperInvariant());

        ClassicAssert.AreEqual("X", Read(endpoint, Fixtures.Envelope(200, "{\"name\":\"x\"}")));
    }

    [Test]
    public void Should_NotInvokeMap_When_ReadFails()
    {
        var invoked = false;
        var endpoint = Get<Widget>().Map(widget =>
        {
            invoked = true;
            return widget;
        });

        Assert.Throws<BricklinkHttpErrorException>(() => Read(endpoint, Fixtures.Envelope(404)));
        ClassicAssert.IsFalse(invoked);
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_MetaHasNoCode()
    {
        Assert.Throws<BricklinkInvalidResponseException>(() => Read(Get<Widget>(), "{\"meta\":{}}"));
    }

    private sealed class TypedWidget
    {
        [JsonPropertyName("type"), JsonConverter(typeof(ItemTypeStringConverter))]
        public ItemType Type { get; set; }
    }

    private sealed class Widget
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("size")]
        public int? Size { get; set; }
    }
}
