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
        public void Simple()
        {
            var opt = SetModule.OfArray(new [] { 1, 2, 3 });
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<FSharpSet<int>>(json, Options);
            Assert.AreEqual(opt, back);
        }
        
        public class Test
        {
            public string F1 { get; set; }
            public FSharpSet<int> F2 { get; set; }
            public int F3 { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj is Test a)
                {
                    return a.F1 == F1 && object.Equals(a.F2, F2) && a.F3 == F3;
                }

                return false;
            }
        }
        
        [Test]
        public void InObject()
        {
            var opt = new Test { F1 = "123", F2 = SetModule.OfArray(new [] { 1, 2, 3 }), F3 = 7 };
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<Test>(json, Options);
            Assert.AreEqual(opt, back);
            
        }
    }
}