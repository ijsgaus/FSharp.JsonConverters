using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.FSharp.Core;
using NUnit.Framework;

namespace FSharp.JsonConverters.Tests
{
    public class FSharpOptionTests
    {
        private static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { new FSharpOptionConverter() }
        };
        
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Simple()
        {
            var opt = FSharpOption<string>.Some("123");
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<FSharpOption<string>>(json, Options);
            Assert.AreEqual(opt, back);
            opt = FSharpOption<string>.None;
            json = JsonSerializer.Serialize(opt, Options);
            back = JsonSerializer.Deserialize<FSharpOption<string>>(json, Options);
            Assert.AreEqual(opt, back);
        }
        
        [Test]
        public void Array()
        {
            var opt = new [] { FSharpOption<string>.Some("123") };
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<FSharpOption<string>[]>(json, Options);
            Assert.AreEqual(opt, back);
            opt = new [] { FSharpOption<string>.None };
            json = JsonSerializer.Serialize(opt, Options);
            back = JsonSerializer.Deserialize<FSharpOption<string>[]>(json, Options);
            Assert.AreEqual(opt, back);
        }

        public class Test
        {
            public string F1 { get; set; }
            public FSharpOption<int> F2 { get; set; }
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
        public void Object()
        {
            var opt = new Test { F1 = "123", F2 = FSharpOption<int>.Some(5), F3 = 7 };
            var json = JsonSerializer.Serialize(opt, Options);
            var back = JsonSerializer.Deserialize<Test>(json, Options);
            Assert.AreEqual(opt, back);
            opt = new Test { F1 = "123", F2 = FSharpOption<int>.None, F3 = 7 };
            json = JsonSerializer.Serialize(opt, Options);
            back = JsonSerializer.Deserialize<Test>(json, Options);
            Assert.AreEqual(opt, back);
        }
        
    }
}