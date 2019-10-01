using System.Text.Json;
using NUnit.Framework;

namespace FSharp.JsonConverters.Tests
{
    public static class Helper
    {
        public static void MakeSimpleTest<T>(T value, JsonSerializerOptions options)
        {
            var json = JsonSerializer.Serialize(value, options);
            var back = JsonSerializer.Deserialize<T>(json, options);
            Assert.AreEqual(value, back);
        }
        
        public static void MakeObjectTest<T>(T value, JsonSerializerOptions options)
        {
            var obj = new TestObject<T>
            {
                F1 = "Test string",
                F2 = value,
                F3 = 45
            };
            var json = JsonSerializer.Serialize(obj, options);
            var back = JsonSerializer.Deserialize<TestObject<T>>(json, options);
            Assert.AreEqual(obj, back);
        }
        
        public class TestObject<T>
        {
            public string F1 { get; set; }
            public T F2 { get; set; }
            public int F3 { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj is TestObject<T> a)
                {
                    return a.F1 == F1 && object.Equals(a.F2, F2) && a.F3 == F3;
                }
                return false;
            }
        } 
        
        
        
    }
}