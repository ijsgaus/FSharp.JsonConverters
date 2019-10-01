using System.Collections.Generic;
using System.Text.Json;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using NUnit.Framework;

namespace FSharp.JsonConverters.Tests
{
    public class FSharpListTest
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { new FSharpListConverter() }
        };

        private static readonly FSharpList<int> Sample = ListModule.OfArray(new[] {1, 2, 3}); 
        
        [Test]
        public void Simple() => Helper.MakeSimpleTest(Sample, Options);


        [Test]
        public void InObject() => Helper.MakeObjectTest(Sample, Options);
        
    }
}