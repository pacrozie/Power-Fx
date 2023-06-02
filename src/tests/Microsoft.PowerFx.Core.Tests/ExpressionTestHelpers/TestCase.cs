// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;

namespace Microsoft.PowerFx.Core.Tests
{
    /// <summary>
    ///  Describe a test case from the .txt file.
    /// </summary>
    public class TestCase
    {
        // Formula string to run 
        public string Input
        {
            get => _input;

            set
            {
                OriginalInput = value;
                _input = AdjustExpression(value);
            }
        }

        private string AdjustExpression(string str)
        {
            // Basic replacements for numbers and comma/semincolon separators
            // This is fine as expressions where this won't work will use #LOCALIZED_EXPRESSION
            if (Culture.NumberFormat.NumberDecimalSeparator == ",")
            {
                str = str.Replace(";", ";;");
                str = str.Replace(',', ';');

                Regex rex = new Regex(@"-?\d+\.\d+", RegexOptions.IgnoreCase);
                str = rex.Replace(str, m => m.Value.Replace(".", ","));
            }

            return str;
        }

        public void AppendInput(string str)
        {
            OriginalInput += str;
            _input += AdjustExpression(str);
        }

        internal string _input;

        public string OriginalInput;

        // Expected Result, indexed by runner name
        public string Expected;

        // Location from source file. 
        public string SourceFile;
        public int SourceLine;
        public string SetupHandlerName;

        private static readonly Dictionary<string, CultureInfo> _cultureCache = new ();

        public TestCase(string locale)
        {
            Locale = locale;
        }

        public string Locale
        {
            get => _locale;

            set
            {
                _locale = value;

                if (_cultureCache.TryGetValue(_locale, out CultureInfo culture))
                {
                    Culture = culture;
                }
                else
                {
                    Culture = new CultureInfo(_locale);
                    _cultureCache.Add(_locale, Culture);
                }
            }
        }

        private string _locale;

        public CultureInfo Culture { get; private set; }

        // For diagnostics, save the orginal location
        public string OverrideFrom;

        public bool IsOverride => OverrideFrom != null;

        // Mark that the test is getting overriden with a new expected result. 
        // This enables per-engine customizations.
        public void MarkOverride(TestCase newTest)
        {
            Locale = newTest.Locale;
            OverrideFrom = $"{newTest.SourceFile}:{newTest.SourceLine}";
            Expected = newTest.Expected;
            SourceFile = newTest.SourceFile;
            SourceLine = newTest.SourceLine;
        }

        // Uniquely identity this test case. 
        // This is very useful for when another file needs to override the results. 
        public string GetUniqueId(string file)
        {
            // Inputs are case sensitive, so the overall key must be case sensitive. 
            // But filenames are case insensitive, so canon them to lowercase.
            var fileKey = file ?? Path.GetFileName(SourceFile);
            return fileKey.ToLowerInvariant() + ":" + Culture.Name.ToLowerInvariant() + ":" + Input;
        }

        public override string ToString()
        {
            return $"[{Culture.Name}] {Path.GetFileName(SourceFile)}:{SourceLine}: {Input}";
        }
    }
}
