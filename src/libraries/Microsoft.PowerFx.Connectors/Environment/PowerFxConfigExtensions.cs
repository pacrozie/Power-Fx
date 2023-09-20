﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.OpenApi.Models;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Core.Functions;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx
{
    [ThreadSafeImmutable]
    public static class ConfigExtensions
    {
        /// <summary>
        /// Add functions for each operation in the <see cref="OpenApiDocument"/>. 
        /// Functions names will be 'functionNamespace.operationName'.
        /// Functions are invoked via rest via the httpClient. The client must handle authentication. 
        /// </summary>
        /// 
        /// <param name="config">Config to add the functions to.</param>
        /// <param name="connectorSettings">Connector settings containing Namespace, NumberIsFloat and MaxRows to be returned.</param>        
        /// <param name="openApiDocument">An API document. This can represent multiple formats, including Swagger 2.0 and OpenAPI 3.0.</param>
        /// <param name="globalValues">Global constant values, like connectionId.</param>
        public static IReadOnlyList<ConnectorFunction> AddActionConnector(this PowerFxConfig config, ConnectorSettings connectorSettings, OpenApiDocument openApiDocument, IReadOnlyDictionary<string, FormulaValue> globalValues = null)
        {
            if (openApiDocument == null)
            {
                throw new ArgumentNullException(nameof(openApiDocument));
            }

            (List<ConnectorFunction> connectorFunctions, List<ConnectorTexlFunction> texlFunctions) = OpenApiParser.Parse(connectorSettings, openApiDocument, globalValues);
            foreach (TexlFunction function in texlFunctions)
            {
                config.AddFunction(function);
            }

            return connectorFunctions;
        }

        /// <summary>
        /// Add functions for each operation in the <see cref="OpenApiDocument"/>. 
        /// Functions names will be 'functionNamespace.operationName'.
        /// Functions are invoked via rest via the httpClient. The client must handle authentication. 
        /// </summary>
        /// <param name="config">Config to add the functions to.</param>
        /// <param name="namespace">Namespace name.</param>
        /// <param name="openApiDocument">An API document. This can represent multiple formats, including Swagger 2.0 and OpenAPI 3.0.</param>
        /// <param name="globalValues">Global constant values, like connectionId.</param>
        /// <returns></returns>
        public static IReadOnlyList<ConnectorFunction> AddActionConnector(this PowerFxConfig config, string @namespace, OpenApiDocument openApiDocument, IReadOnlyDictionary<string, FormulaValue> globalValues = null)
        {
            return config.AddActionConnector(new ConnectorSettings(@namespace), openApiDocument, globalValues);
        }

        /// <summary>
        /// Add functions for each operation in the <see cref="OpenApiDocument"/>. 
        /// Functions names will be 'functionNamespace.operationName'.
        /// Functions are invoked via rest via the httpClient. The client must handle authentication. 
        /// </summary>
        /// <param name="config">Config to add the functions to.</param>
        /// <param name="namespace">Namespace name.</param>
        /// <param name="openApiDocument">An API document. This can represent multiple formats, including Swagger 2.0 and OpenAPI 3.0.</param>
        /// <param name="connectionId">ConnectionId.</param>
        /// <returns></returns>
        public static IReadOnlyList<ConnectorFunction> AddPowerPlatformActionConnector(this PowerFxConfig config, string @namespace, OpenApiDocument openApiDocument, string connectionId)
        {
            return config.AddActionConnector(new ConnectorSettings(@namespace), openApiDocument, new ReadOnlyDictionary<string, FormulaValue>(new Dictionary<string, FormulaValue>() { { "connectionId", FormulaValue.New(connectionId) } }));
        }

        public static async Task<ConnectorTableValue> AddTabularConnector(this PowerFxConfig config, string tableName, OpenApiDocument openApiDocument, IReadOnlyDictionary<string, FormulaValue> globalValues, HttpClient client, CancellationToken cancellationToken)
        {
            return await config.AddTabularConnector(new ConnectorSettings($"_tbl_{tableName}"), tableName, openApiDocument, globalValues, client, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<ConnectorTableValue> AddTabularConnector(this PowerFxConfig config, ConnectorSettings connectorSettings, string tableName, OpenApiDocument openApiDocument, IReadOnlyDictionary<string, FormulaValue> globalValues, HttpClient client, CancellationToken cancellationToken)
        {
            IReadOnlyList<ConnectorFunction> tabularFunctions = config.AddActionConnector(connectorSettings, openApiDocument, globalValues);

            // Retrieve table schema with dynamic intellisense on 'GetItem' function            
            ConnectorFunction getItem = tabularFunctions.First(f => f.Name.Contains("GetItem") && !f.Name.Contains("GetItems")); // SQL: GetItemV2 (not GetItemsV2)
            ConnectorType tableSchema = await getItem.GetConnectorReturnTypeAsync(globalValues.Select(kvp => new NamedValue(kvp)).ToArray(), new TempConnectorContext(client), cancellationToken).ConfigureAwait(false);

            return tableSchema == null
                ? throw new InvalidOperationException("Cannot determine table schema")
                : new ConnectorTableValue(tableName, tabularFunctions, tableSchema.FormulaType as RecordType);
        }

        private class TempConnectorContext : BaseRuntimeConnectorContext
        {
            private readonly HttpMessageInvoker _invoker;

            internal TempConnectorContext(HttpMessageInvoker invoker) { _invoker = invoker; }

            public override TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;

            public override HttpMessageInvoker GetInvoker(string @namespace) => _invoker;
        }
    }
}
