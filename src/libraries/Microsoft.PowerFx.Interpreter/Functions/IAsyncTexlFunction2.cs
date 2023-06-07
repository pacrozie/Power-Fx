﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx.Interpreter;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx.Core.Functions
{
    // A Texl function capable of async invokes. 
    internal interface IAsyncTexlFunction2
    {
        Task<FormulaValue> InvokeAsync(IRuntimeContext context, FormulaValue[] args);
    }
}