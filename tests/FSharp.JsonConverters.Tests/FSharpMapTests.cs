using System;
using System.Text.Json;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace FSharp.JsonConverters.Tests
{
    public class FSharpMapTests
    {
        

        private static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { new FSharpMapConverter(true) }
        };
        
        [Test]
        public void SimpleString()
        {
            var opt = MapModule.OfArray(new [] { Tuple.Create("1", 1), Tuple.Create("2", 1), Tuple.Create("3", 1) });
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<FSharpMap<string, int>>(json, Options);
            Assert.AreEqual(opt, back);
        }
        
        public class Test1
        {
            public string F1 { get; set; }
            public FSharpMap<string, int> F2 { get; set; }
            public int F3 { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj is Test1 a)
                {
                    return a.F1 == F1 && object.Equals(a.F2, F2) && a.F3 == F3;
                }

                return false;
            }
        }
        
        [Test]
        public void InObject()
        {
            var opt = new Test1 { F1 = "123", F2 = MapModule.OfArray(new [] { Tuple.Create("1", 1), Tuple.Create("2", 1), Tuple.Create("3", 1) }), F3 = 7 };
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<Test1>(json, Options);
            Assert.AreEqual(opt, back);
            
        }
        
        [Test]
        public void Simple()
        {
            var opt = MapModule.OfArray(new [] { Tuple.Create(1, 1), Tuple.Create(2, 1), Tuple.Create(3, 1) });
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<FSharpMap<int, int>>(json, Options);
            Assert.AreEqual(opt, back);
        }
        
        public class Test2
        {
            public string F1 { get; set; }
            public FSharpMap<int, int> F2 { get; set; }
            public int F3 { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj is Test2 a)
                {
                    return a.F1 == F1 && object.Equals(a.F2, F2) && a.F3 == F3;
                }

                return false;
            }
        }
        
        [Test]
        public void InObject1()
        {
            var opt = new Test2 { F1 = "123", F2 = MapModule.OfArray(new [] { Tuple.Create(1, 1), Tuple.Create(2, 1), Tuple.Create(3, 1) }), F3 = 7 };
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<Test2>(json, Options);
            Assert.AreEqual(opt, back);
            
        }
    }
}