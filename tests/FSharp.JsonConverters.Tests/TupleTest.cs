using System;
using System.Text.Json;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace FSharp.JsonConverters.Tests
{
    public class TupleTest
    {
        private static JsonSerializerOptions Options1 = new JsonSerializerOptions
        {
            Converters = { new TupleAsMapConverter() }
        };
        
        private static JsonSerializerOptions Options2 = new JsonSerializerOptions
        {
            Converters = { new TupleAsArrayConverter() }
        };
        
        [Test]
        public void SimpleTuple() => Helper.MakeSimpleTest(Tuple.Create(1, "456", true), Options1);
        
        
        [Test]
        public void TupleInObject() => Helper.MakeObjectTest(Tuple.Create(1, "456", true), Options1);
        
        [Test]
        public void SimpleTupleArray() => Helper.MakeSimpleTest(Tuple.Create(1, "456", true), Options2);
        
        
        [Test]
        public void TupleInObjectArray() => Helper.MakeObjectTest(Tuple.Create(1, "456", true), Options2);
        
        [Test]
        public void SimpleValueTuple() => Helper.MakeSimpleTest((1, "456", true), Options1);
        
        
        [Test]
        public void ValueTupleInObject() => Helper.MakeObjectTest((1, "456", true), Options1);
        
        [Test]
        public void SimpleValueTupleArray() => Helper.MakeSimpleTest((1, "456", true), Options2);
        
        
        [Test]
        public void ValueTupleInObjectArray() => Helper.MakeObjectTest((1, "456", true), Options2);
    }
}