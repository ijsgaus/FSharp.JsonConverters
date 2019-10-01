using System.Text.Json;
using NUnit.Framework;
using FSharp.Tests.Types;
using Microsoft.FSharp.Core;

namespace FSharp.JsonConverters.Tests
{
    public class UnionTest
    {
        private static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { new FSharpOptionConverter(), 
                           new TupleAsMapConverter(), 
                           new FSharpUnionConverter() }
        };
        
        private static JsonSerializerOptions OptionsCamelCase = new JsonSerializerOptions
        {
            Converters = { new FSharpOptionConverter(), 
                new TupleAsMapConverter(), 
                new FSharpUnionConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        private static JsonSerializerOptions OptionsCamelCaseInsensitive = new JsonSerializerOptions
        {
            Converters = { new FSharpOptionConverter(), 
                new TupleAsMapConverter(), 
                new FSharpUnionConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        [Test]
        public void Erased()
        {
            Helper.MakeSimpleTest(ErasedUnion.NewErasedUnion(5), Options);
            Helper.MakeSimpleTest(ErasedUnion.NewErasedUnion(5), OptionsCamelCase);
            Helper.MakeSimpleTest(ErasedUnion.NewErasedUnion(5), OptionsCamelCaseInsensitive);
            
            Helper.MakeObjectTest(ErasedUnion.NewErasedUnion(5), Options);
            Helper.MakeObjectTest(ErasedUnion.NewErasedUnion(5), OptionsCamelCase);
            Helper.MakeObjectTest(ErasedUnion.NewErasedUnion(5), OptionsCamelCaseInsensitive);
            
            Helper.MakeSimpleTest(ErasedUnion1.NewErasedUnion1(5,"123"), Options);
            Helper.MakeSimpleTest(ErasedUnion1.NewErasedUnion1(5,"123"), OptionsCamelCase);
            Helper.MakeSimpleTest(ErasedUnion1.NewErasedUnion1(5,"123"), OptionsCamelCaseInsensitive);
            
            Helper.MakeObjectTest(ErasedUnion1.NewErasedUnion1(5,"123"), Options);
            Helper.MakeObjectTest(ErasedUnion1.NewErasedUnion1(5,"123"), OptionsCamelCase);
            Helper.MakeObjectTest(ErasedUnion1.NewErasedUnion1(5,"123"), OptionsCamelCaseInsensitive);
            
            Helper.MakeSimpleTest(ErasedUnion2.NewErasedUnion2(true,"123"), Options);
            Helper.MakeSimpleTest(ErasedUnion2.NewErasedUnion2(true,"123"), OptionsCamelCase);
            Helper.MakeSimpleTest(ErasedUnion2.NewErasedUnion2(true,"123"), OptionsCamelCaseInsensitive);
            
            Helper.MakeObjectTest(ErasedUnion2.NewErasedUnion2(true,"123"), Options);
            Helper.MakeObjectTest(ErasedUnion2.NewErasedUnion2(true,"123"), OptionsCamelCase);
            Helper.MakeObjectTest(ErasedUnion2.NewErasedUnion2(true,"123"), OptionsCamelCaseInsensitive);
        }
        
        [Test]
        public void String()
        {
            Helper.MakeSimpleTest(StringUnion.Value1, Options);
            Helper.MakeSimpleTest(StringUnion.Value2, OptionsCamelCase);
            Helper.MakeSimpleTest(StringUnion.Value1, OptionsCamelCaseInsensitive);
            
            Helper.MakeObjectTest(StringUnion.Value1, Options);
            Helper.MakeObjectTest(StringUnion.Value2, OptionsCamelCase);
            Helper.MakeObjectTest(StringUnion.Value1, OptionsCamelCaseInsensitive);
        }
        
        [Test]
        public void Other()
        {
            Helper.MakeSimpleTest(OtherUnion.NoValue, Options);
            Helper.MakeSimpleTest(OtherUnion.NewSingle(FSharpOption<string>.None), Options);
            Helper.MakeSimpleTest(OtherUnion.NewTuple(45, "qwe"), Options);
            Helper.MakeSimpleTest(OtherUnion.NewNamed(45, FSharpOption<string>.None), Options);
            
            Helper.MakeSimpleTest(OtherUnion.NoValue, OptionsCamelCase);
            Helper.MakeSimpleTest(OtherUnion.NewSingle(FSharpOption<string>.None), OptionsCamelCase);
            Helper.MakeSimpleTest(OtherUnion.NewTuple(45, "qwe"), OptionsCamelCase);
            Helper.MakeSimpleTest(OtherUnion.NewNamed(45, FSharpOption<string>.None), OptionsCamelCase);
            
            Helper.MakeSimpleTest(OtherUnion.NoValue, OptionsCamelCaseInsensitive);
            Helper.MakeSimpleTest(OtherUnion.NewSingle(FSharpOption<string>.None), OptionsCamelCaseInsensitive);
            Helper.MakeSimpleTest(OtherUnion.NewTuple(45, "qwe"), OptionsCamelCaseInsensitive);
            Helper.MakeSimpleTest(OtherUnion.NewNamed(45, FSharpOption<string>.None), OptionsCamelCaseInsensitive);
            
            Helper.MakeObjectTest(OtherUnion.NoValue, Options);
            Helper.MakeObjectTest(OtherUnion.NewSingle(FSharpOption<string>.None), Options);
            Helper.MakeObjectTest(OtherUnion.NewTuple(45, "qwe"), Options);
            Helper.MakeObjectTest(OtherUnion.NewNamed(45, FSharpOption<string>.None), Options);
            
            Helper.MakeObjectTest(OtherUnion.NoValue, OptionsCamelCase);
            Helper.MakeObjectTest(OtherUnion.NewSingle(FSharpOption<string>.None), OptionsCamelCase);
            Helper.MakeObjectTest(OtherUnion.NewTuple(45, "qwe"), OptionsCamelCaseInsensitive);
            Helper.MakeObjectTest(OtherUnion.NewNamed(45, FSharpOption<string>.None), OptionsCamelCase);
            
            Helper.MakeObjectTest(OtherUnion.NoValue, OptionsCamelCaseInsensitive);
            Helper.MakeObjectTest(OtherUnion.NewSingle(FSharpOption<string>.None), OptionsCamelCaseInsensitive);
            Helper.MakeObjectTest(OtherUnion.NewTuple(45, "qwe"), OptionsCamelCaseInsensitive);
            Helper.MakeObjectTest(OtherUnion.NewNamed(45, FSharpOption<string>.None), OptionsCamelCaseInsensitive);
            
            
        }
        
        
        
    }
}