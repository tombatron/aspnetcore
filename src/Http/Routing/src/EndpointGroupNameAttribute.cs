﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.Http;

 namespace Microsoft.AspNetCore.Routing
 {
    /// <summary>
    /// Specifies the endpoint group name in <see cref="Microsoft.AspNetCore.Http.Endpoint.Metadata"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EndpointGroupNameAttribute : Attribute, IEndpointGroupNameMetadata
    {
        /// <summary>
        /// Initializes an instance of the <see cref="EndpointGroupNameAttribute"/>.
        /// </summary>
        /// <param name="endpointGroupName">The endpoint group name.</param>
        public EndpointGroupNameAttribute(string endpointGroupName)
        {
            if (endpointGroupName == null)
            {
                throw new ArgumentNullException(nameof(endpointGroupName));
            }

            EndpointGroupName = endpointGroupName;
        }

        /// <inheritdoc />
        public string EndpointGroupName { get; }
    }
 }