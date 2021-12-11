// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using Microsoft.Diagnostics.Monitoring.EventPipe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.WebApi
{
    internal static class PrometheusDataModel
    {
        private const char SeperatorChar = '_';

        private static readonly Dictionary<string, string> KnownUnits = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {string.Empty, string.Empty},
            {"count", string.Empty},
            {"B", "bytes" },
            {"MB", "bytes" },
            {"%", "ratio" },
        };

        public static string Normalize(string metricProvider, string metric, string unit, double value, out string metricValue)
        {
            string baseUnit = null;
            if ((unit != null) && (!KnownUnits.TryGetValue(unit, out baseUnit)))
            {
                baseUnit = unit;
            }
            if (string.Equals(unit, "MB", StringComparison.OrdinalIgnoreCase))
            {
                value *= 1_000_000; //Note that the metric uses MB not MiB
            }
            metricValue = value.ToString(CultureInfo.InvariantCulture);

            bool hasUnit = !string.IsNullOrEmpty(baseUnit);

            //The +1's account for separators
            //CONSIDER Can we optimize with Span/stackalloc here instead of using StringBuilder?
            StringBuilder builder = new StringBuilder(metricProvider.Length + metric.Length + (hasUnit ? baseUnit.Length + 1 : 0) + 1);

            NormalizeString(builder, metricProvider, isProvider: true);
            builder.Append(SeperatorChar);
            NormalizeString(builder, metric, isProvider: false);
            if (hasUnit)
            {
                builder.Append(SeperatorChar);
                NormalizeString(builder, baseUnit, isProvider: false);
            }

            return builder.ToString();
        }

        private static void NormalizeString(StringBuilder builder, string entity, bool isProvider)
        {
            //TODO We don't have any labels in the current metrics implementation, but may need to add support for it
            //for tags in the new dotnet metrics. Labels have some additional restrictions.

            bool allInvalid = true;
            for (int i = 0; i < entity.Length; i++)
            {
                if (IsValidChar(entity[i], i == 0))
                {
                    allInvalid = false;
                    builder.Append(isProvider ? char.ToLowerInvariant(entity[i]) : entity[i]);
                }
                else if (!isProvider)
                {
                    builder.Append(SeperatorChar);
                }
            }

            //CONSIDER Completely invalid providers such as '!@#$' will become '_'. Should we have a more obvious value for this?
            if (allInvalid && isProvider)
            {
                builder.Append(SeperatorChar);
            }
        }
        private static bool IsValidChar(char c, bool isFirst)
        {
            if (c > 'z')
            {
                return false;
            }

            if (c == SeperatorChar)
            {
                return true;
            }

            if (isFirst)
            {
                return char.IsLetter(c);
            }
            return char.IsLetterOrDigit(c);
        }

        public static string GetPrometheusMetric(ICounterPayload metric, out string metricValue)
        {
            string unitSuffix = string.Empty;

            if ((metric.Unit != null) && (!KnownUnits.TryGetValue(metric.Unit, out unitSuffix)))
            {
                //TODO The prometheus data model does not allow certain characters. Units we are not expecting could cause a scrape failure.
                unitSuffix = "_" + metric.Unit;
            }

            double value = metric.Value;
            if (string.Equals(metric.Unit, "MB", StringComparison.OrdinalIgnoreCase))
            {
                value *= 1_000_000; //Note that the metric uses MB not MiB
            }

            ParseMetricName(metric.Name, out var metricName, out var metricLabels);

            metricValue = value.ToString(CultureInfo.InvariantCulture);
            if (metricLabels != null)
            {
                metricValue = string.Concat(metricLabels, " ", metricValue);
            }

            return FormattableString.Invariant($"{metric.Provider.Replace(".", string.Empty).ToLowerInvariant()}_{metricName.Replace('-', '_')}{unitSuffix}");
        }

        private static void ParseMetricName(string value, out string name, out string labels)
        {
            name = value;
            labels = null;

            var labelsStartIndex = value.IndexOf('{');
            if (labelsStartIndex < 0 || labelsStartIndex == value.Length - 1)
                return;

            name = value.Substring(0, labelsStartIndex);

            var labelsEndIndex = value.IndexOf('}', labelsStartIndex + 1);
            if (labelsEndIndex < 0)
                return;

            var labelsLength = labelsEndIndex - labelsStartIndex + 1;
            if (labelsLength > 2)
                labels = value.Substring(labelsStartIndex, labelsLength);
        }
    }
}
