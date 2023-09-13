﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.PowerFx.Core.Localization;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Syntax;

namespace Microsoft.PowerFx.Core.Errors
{
    // TASK: 67034: Cleanup: Eliminate StringIds.
    internal sealed class TexlError : BaseError, IRuleError
    {
        private readonly List<string> _nameMapIDs;

        // Node may be null.
        public readonly TexlNode Node;

        // Tok will always be non-null.
        public readonly Token Tok;

        // TextSpan for the rule error.
        public override Span TextSpan { get; }

        public override IEnumerable<string> SinkTypeErrors => _nameMapIDs;

        public TexlError(Token tok, DocumentErrorSeverity severity, ErrorResourceKey errKey, params object[] args)
            : base(null, null, DocumentErrorKind.AXL, severity, errKey, args)
        {
            Contracts.AssertValue(tok);

            Tok = tok;
            TextSpan = new Span(tok.VerifyValue().Span.Min, tok.VerifyValue().Span.Lim, tok.VerifyValue().Span.BaseIndex);

            _nameMapIDs = new List<string>();
        }

        public TexlError(TexlNode node, DocumentErrorSeverity severity, ErrorResourceKey errKey, params object[] args)
            : base(null, null, DocumentErrorKind.AXL, severity, errKey, args)
        {
            Contracts.AssertValue(node);
            Contracts.AssertValue(node.Token);

            Node = node;
            Tok = node.Token;
            TextSpan = node.GetTextSpan();

            _nameMapIDs = new List<string>();
        }

        IRuleError IRuleError.Clone(Span span)
        {
            return new TexlError(this.Tok.Clone(span), this.Severity, this.ErrorResourceKey, this.MessageArgs);
        }

        public void MarkSinkTypeError(DName name)
        {
            Contracts.AssertValid(name);

            Contracts.Assert(!_nameMapIDs.Contains(name.Value));
            _nameMapIDs.Add(name.Value);
        }

        internal override void FormatCore(StringBuilder sb)
        {
            Contracts.AssertValue(sb);

            // $$$ can't use current culture
            sb.AppendFormat(CultureInfo.CurrentCulture, TexlStrings.FormatSpan_Min_Lim(), Tok.Span.Min, Tok.Span.Lim);

            if (Node != null)
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, TexlStrings.InfoNode_Node(), Node.ToString());
            }
            else
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, TexlStrings.InfoTok_Tok(), Tok.ToString());
            }

            sb.AppendFormat(CultureInfo.CurrentCulture, TexlStrings.FormatErrorSeparator());
            base.FormatCore(sb);
        }
    }
}
