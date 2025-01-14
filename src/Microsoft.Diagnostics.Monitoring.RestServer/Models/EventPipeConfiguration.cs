﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Microsoft.Diagnostics.Monitoring.RestServer.Models
{
    public class EventPipeConfiguration
    {
        [JsonPropertyName("providers")]
        [Required, MinLength(1)]
        public EventPipeProvider[] Providers { get; set; }

        [JsonPropertyName("requestRundown")]
        public bool RequestRundown { get; set; } = true;

        [JsonPropertyName("bufferSizeInMB")]
        [Range(1, 1024)]
        public int BufferSizeInMB { get; set; } = 256;
    }
}
