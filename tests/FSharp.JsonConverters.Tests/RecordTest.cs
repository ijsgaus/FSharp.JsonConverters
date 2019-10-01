using System;
using System.Text.Json;
using NUnit.Framework;
using FSharp.Tests.Types;
using Microsoft.FSharp.Core;

namespace FSharp.JsonConverters.Tests
{
    public class RecordTest
    {
        private static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { new FSharpOptionConverter(), new FSharpRecordConverter() }
        };
        
        private static JsonSerializerOptions OptionsCamelCase = new JsonSerializerOptions
        {
            Converters = { new FSharpOptionConverter(), new FSharpRecordConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        private static JsonSerializerOptions OptionsCamelCaseInsensitive = new JsonSerializerOptions
        {
            Converters = { new FSharpOptionConverter(), new FSharpRecordConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        
        [Test]
        public void SimpleRec1()
        {
            Helper.MakeSimpleTest(new TestRecord("123", FSharpOption<int>.None), Options);
            Helper.MakeSimpleTest(new TestRecord("123", FSharpOption<int>.None), OptionsCamelCase);
            Helper.MakeSimpleTest(new TestRecord("123", FSharpOption<int>.None), OptionsCamelCaseInsensitive);
        }

        [Test]
        public void SimpleRec2()
        {
            Helper.MakeSimpleTest(new TestRecord("123", FSharpOption<int>.Some(5)), Options);
            Helper.MakeSimpleTest(new TestRecord("123", FSharpOption<int>.Some(5)), OptionsCamelCase);
            Helper.MakeSimpleTest(new TestRecord("123", FSharpOption<int>.Some(5)), OptionsCamelCaseInsensitive);
        }

        [Test]
        public void SimpleRec3()
        {
            Helper.MakeSimpleTest(new TestRecordRec("123",
                FSharpOption<TestRecordRec>.Some(new TestRecordRec("456", FSharpOption<TestRecordRec>.None))), Options);
            Helper.MakeSimpleTest(new TestRecordRec("123",
                FSharpOption<TestRecordRec>.Some(new TestRecordRec("456", FSharpOption<TestRecordRec>.None))), OptionsCamelCase);
            Helper.MakeSimpleTest(new TestRecordRec("123",
                FSharpOption<TestRecordRec>.Some(new TestRecordRec("456", FSharpOption<TestRecordRec>.None))), OptionsCamelCaseInsensitive);
        }


        [Test]
        public void ObjectRec1() => Helper.MakeObjectTest(new TestRecord("123", FSharpOption<int>.None), Options);
        
        [Test]
        public void ObjectRec2() => Helper.MakeObjectTest(new TestRecord("123", FSharpOption<int>.Some(5)), Options);
        
        [Test]
        public void ObjectRec3() => Helper.MakeObjectTest(new TestRecordRec("123", 
            FSharpOption<TestRecordRec>.Some(new TestRecordRec("456", FSharpOption<TestRecordRec>.None))), Options);
        
        
    }
}