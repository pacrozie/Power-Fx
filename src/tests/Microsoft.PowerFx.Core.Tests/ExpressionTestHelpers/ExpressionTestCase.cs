// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Microsoft.PowerFx.Core.Tests
{
    // Wrap a test case for calling from xunit. 
    public class ExpressionTestCase : TestCase, IXunitSerializable
    {
        private readonly string _engineName = null;

        // List of locales to run the tests against.
        public const string TestedLocales = "en-US;fr-FR;de-DE;pt-BR;ja-JP";

        // Normally null. Set if the test discovery infrastructure needs to send a notice to the test runner. 
        public string FailMessage;

        public ExpressionTestCase() 
            : this("en-US")
        {
            throw new NotImplementedException();
        }

        public ExpressionTestCase(string locale)
            : base(locale)
        {
            _engineName = "-";
        }

        public ExpressionTestCase(string engineName, string locale)
            : base(locale)
        {
            _engineName = engineName;
        }

        public ExpressionTestCase(string engineName, TestCase test)
            : this(engineName, test.Locale)
        {
            Input = test.Input;
            Expected = test.Expected;
            SourceFile = test.SourceFile;
            SourceLine = test.SourceLine;
            SetupHandlerName = test.SetupHandlerName;            
        }

        public static ExpressionTestCase Fail(string message)
        {
            return new ExpressionTestCase("en-US")
            {
                FailMessage = message
            };
        }

        public override string ToString()
        {
            var str = $"{Path.GetFileName(SourceFile)} : [{base.Culture.Name}] {SourceLine.ToString("000")} - {Input} = {Expected}";

            if (!string.IsNullOrEmpty(SetupHandlerName))
            {
                str += " - Setup: " + SetupHandlerName;
            }

            return str;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            try
            {
                Expected = info.GetValue<string>("expected");
                Input = info.GetValue<string>("input");
                SourceFile = info.GetValue<string>("sourceFile");
                SourceLine = info.GetValue<int>("sourceLine");
                SetupHandlerName = info.GetValue<string>("setupHandlerName");
                FailMessage = info.GetValue<string>("failMessage");
                Locale = info.GetValue<string>("locale");
            }
            catch (Exception e)
            {
                FailMessage = $"Failed to deserialized test {e.Message}";
            }
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("expected", Expected, typeof(string));
            info.AddValue("input", Input, typeof(string));
            info.AddValue("sourceFile", SourceFile, typeof(string));
            info.AddValue("sourceLine", SourceLine, typeof(int));
            info.AddValue("setupHandlerName", SetupHandlerName, typeof(string));
            info.AddValue("failMessage", FailMessage, typeof(string));
            info.AddValue("locale", Locale, typeof(string));
        }
    }
}
