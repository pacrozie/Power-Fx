﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

extern alias PfxCore;

using System;
using System.Collections.Generic;
using PfxCore.Microsoft.PowerFx.Core.Localization;
using PfxCore.Microsoft.PowerFx.Syntax;
using Xunit;

namespace Microsoft.PowerFx.Core.Tests
{
    // Do static analysis to look for potential threading issues. 
    public class ThreadingTests : PowerFxTest
    {
        [Fact]
        public void CheckFxCore()
        {
            var asm = typeof(TexlNode).Assembly;

            var bugsFieldType = new HashSet<Type>
            {            
                // make these fields readonly
                typeof(TexlStrings.StringGetter),
                typeof(ErrorResourceKey),
            };

            var bugNames = new HashSet<string>
            {
                // Threading.InterlockedIncrement.
                "VersionHash._hashStarter",

                // readonly arrays / dictionary - is there an IReadOnly type to changes these too instead? 
                "LazyList`1.Empty",
                "DType._kindToSuperkindMapping",
                "DTypeSpecParser._types",
                "BuiltinFunctionsCore._library",
                "BuiltinFunctionsCore._testOnlyLibrary",
                "BuiltinFunctionsCore._featureGateFunctions",
                "ArgumentSuggestions._languageCodeSuggestions",
                "IntellisenseProvider.SuggestionHandlers",
                "DateAddFunction.SubDayStringList",
                "PrettyPrintVisitor.BinaryPrecedence",
                "ErrorResource.ErrorResourceTagToReswSuffix",
                "DelegationCapability._binaryOpToDelegationCapabilityMap",
                "DelegationCapability._unaryOpToDelegationCapabilityMap",
                "DelegationCapability._operatorToDelegationCapabilityMap",
                "ODataFunctionMappings.BinaryOpToOperatorMap",
                "ODataFunctionMappings.UnaryOpToOperatorMap",
                       
                // Potential bugs, Need more review...
                "ArgumentSuggestions.CustomFunctionSuggestionProviders",
                "TrackingProvider.Instance",

                "DataTypeInfo.NoValidFormat", // returns arrays 
                "DataTypeInfo.AllowedValuesOnly",
                "DataTypeInfo._validDataFormatsPerDKind",

                "FeatureFlags._stringInterpolation",
                "FeatureFlags._inTests",
                "EmptyEnumerator`1._instance",
                "Contracts._assertFailExCtor",
                "CurrentLocaleInfo.<CurrentLocaleName>k__BackingField",
                "CurrentLocaleInfo.<CurrentUILanguageName>k__BackingField",
                "CurrentLocaleInfo.<CurrentLCID>k__BackingField",
                "StringResources.<ExternalStringResources>k__BackingField",
                "StringResources.<ShouldThrowIfMissing>k__BackingField",
                "DelegationCapability.maxSingleCapabilityValue",
                "BuiltinFunctionsCore.OtherKnownFunctions"
            };

            AnalyzeThreadSafety.CheckStatics(asm, bugsFieldType, bugNames);
        }
    }
}
