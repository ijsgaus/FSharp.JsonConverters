using System.Text.Json;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace FSharp.JsonConverters.Tests
{
    public class FSharpSetTest
    {
        private static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { new FSharpSetConverter() }
        };

        [Test]
        public void Simple() => Helper.MakeSimpleTest(SetModule.OfArray(new[] {1, 2, 3}), Options);
        
        
        [Test]
        public void InObject() => Helper.MakeObjectTest(SetModule.OfArray(new[] {1, 2, 3}), Options);
    }
}