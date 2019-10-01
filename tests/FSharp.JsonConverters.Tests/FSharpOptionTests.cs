using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using NUnit.Framework;

namespace FSharp.JsonConverters.Tests
{
    public class FSharpOptionTests
    {
        private static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { new FSharpOptionConverter(), new FSharpListConverter() }
        };
        
        [Test]
        public void Simple()
        {
            Helper.MakeSimpleTest(FSharpOption<string>.Some("123"), Options);
            Helper.MakeSimpleTest(FSharpOption<string>.None, Options);
        }
        
        [Test]
        public void List()
        {
            var opt = ListModule.OfArray( new [] { FSharpOption<string>.Some("123"), FSharpOption<string>.None  });
            Helper.MakeSimpleTest(opt, Options);
            Helper.MakeObjectTest(opt, Options);
        }

        
        
        [Test]
        public void Object()
        {
            Helper.MakeObjectTest(FSharpOption<string>.Some("123"), Options);
            Helper.MakeObjectTest(FSharpOption<string>.None, Options);
        }
        
    }
}