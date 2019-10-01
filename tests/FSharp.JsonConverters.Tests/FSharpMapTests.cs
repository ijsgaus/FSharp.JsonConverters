using System;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace FSharp.JsonConverters.Tests
{
    public class FSharpMapTests
    {
        

        private static JsonSerializerOptions Options1 = new JsonSerializerOptions
        {
            Converters = { new FSharpMapConverter(), new TupleAsMapConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
        
        private static JsonSerializerOptions Options2 = new JsonSerializerOptions
        {
            Converters = { new FSharpMapConverter(), new TupleAsArrayConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        [Test]
        public void SimpleString() =>
            Helper.MakeSimpleTest(
                MapModule.OfArray(new[] {Tuple.Create("1", 1), Tuple.Create("2", 1), Tuple.Create("3", 1)}), Options1);
        
        
        [Test]
        public void InObject() =>
            Helper.MakeObjectTest(
                MapModule.OfArray(new[] {Tuple.Create("1", 1), Tuple.Create("2", 1), Tuple.Create("3", 1)}), Options1);


        [Test]
        public void SimpleTuples() =>
            Helper.MakeSimpleTest(
                MapModule.OfArray(new[] {Tuple.Create(1, 1), Tuple.Create(2, 1), Tuple.Create(3, 1)}), Options1);
        
            
        [Test]
        public void ObjectTuples() =>
            Helper.MakeObjectTest(
                MapModule.OfArray(new[] {Tuple.Create(1, 1), Tuple.Create(2, 1), Tuple.Create(3, 1)}), Options1);
        
        
        [Test]
        public void SimpleArray() =>
            Helper.MakeSimpleTest(
                MapModule.OfArray(new[] {Tuple.Create(1, 1), Tuple.Create(2, 1), Tuple.Create(3, 1)}), Options2);
        
            
        [Test]
        public void ObjectArray() =>
            Helper.MakeObjectTest(
                MapModule.OfArray(new[] {Tuple.Create(1, 1), Tuple.Create(2, 1), Tuple.Create(3, 1)}), Options2);


        
    }
}