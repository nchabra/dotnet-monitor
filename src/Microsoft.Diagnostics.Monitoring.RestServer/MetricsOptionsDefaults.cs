﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if UNITTEST
namespace Microsoft.Diagnostics.Monitoring.UnitTests.Options
#else
namespace Microsoft.Diagnostics.Monitoring.RestServer
#endif
{
    internal class MetricsOptionsDefaults
    {
        public const bool Enabled = true;

        public const int UpdateIntervalSeconds = 10;

        public const int MetricCount = 3;

        public const bool IncludeDefaultProviders = true;
    }
}
