/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2026 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 *
 * If using the Work as, or as part of, a network application, by
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading,
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

using System.Globalization;
using System.Net;

namespace FiftyOne.IpIntelligence.Shared
{
    /// <summary>
    /// Parses IP address evidence values. Front ends commonly supply the
    /// client address with a port suffix ("203.0.113.9:54321",
    /// "[2001:db8::9]:443"), so values are accepted in bare, port
    /// suffixed, and bracketed forms. An IPv6 address with an
    /// unbracketed port suffix is inherently ambiguous and parses as an
    /// address including the trailing group.
    /// </summary>
    public static class IpEvidenceValue
    {
        /// <summary>
        /// Try to parse the evidence value as an IP address, accepting
        /// bare, port suffixed, and bracketed forms.
        /// </summary>
        /// <param name="value">The raw evidence value.</param>
        /// <param name="address">The parsed address, or null.</param>
        /// <returns>True when an address was parsed.</returns>
        public static bool TryParse(string value, out IPAddress address)
        {
            address = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            var candidate = value.Trim();
            if (candidate[0] == '[')
            {
                // Bracketed IPv6 - "[2001:db8::9]" or "[2001:db8::9]:443".
                var close = candidate.IndexOf(']');
                if (close < 2 ||
                    (close != candidate.Length - 1 &&
                    IsPortSuffix(candidate, close + 1) == false))
                {
                    return false;
                }
                return IPAddress.TryParse(
                    candidate.Substring(1, close - 1),
                    out address);
            }
            if (IPAddress.TryParse(candidate, out address))
            {
                return true;
            }
            // IPv4 with a port suffix has exactly one colon.
            var colon = candidate.IndexOf(':');
            if (colon > 0 &&
                colon == candidate.LastIndexOf(':') &&
                IsPortSuffix(candidate, colon) &&
                IPAddress.TryParse(candidate.Substring(0, colon), out address))
            {
                return true;
            }
            address = null;
            return false;
        }

        /// <summary>
        /// True when the value from <paramref name="colonIndex"/> onward is
        /// a colon followed by a decimal port number.
        /// </summary>
        private static bool IsPortSuffix(string value, int colonIndex)
        {
            return colonIndex < value.Length - 1 &&
                value[colonIndex] == ':' &&
                ushort.TryParse(
                    value.Substring(colonIndex + 1),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out _);
        }
    }
}
