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

using System.Net;
using System.Net.Sockets;

namespace FiftyOne.IpIntelligence.Shared
{
    /// <summary>
    /// Parses IP address evidence values the same way the native engine
    /// does, so the managed layer and the native lookup always agree on
    /// which address a value contains. The native parser reads a single
    /// address from the start of the value and stops at the first break
    /// character (comma, space, slash, closing bracket, newline, or a
    /// colon once the address is known to be IPv4). Everything after the
    /// break is ignored, which is what makes port suffixes
    /// ("203.0.113.9:54321", "[2001:db8::9]:443"), forwarded chains
    /// (first entry) and CIDR ranges (prefix address) parse.
    /// One deliberate divergence: IPv4 is gated on a canonical dotted
    /// quad, so legacy forms that would be reinterpreted ("85.118.2",
    /// "192.168.015.1", "0x7f.0.0.1") fail here rather than resolving
    /// to a different address than the sender meant.
    /// </summary>
    public static class IpEvidenceValue
    {
        /// <summary>
        /// Characters at which the native parser stops reading an
        /// address and ignores the remainder of the value.
        /// </summary>
        private static readonly char[] BreakChars =
            new[] { ',', ' ', '/', ']', '\n' };

        /// <summary>
        /// Try to parse the evidence value as the IP address the native
        /// engine would read from it. When this fails the native engine
        /// would reject the value too, aborting its evidence walk, so a
        /// failing value must not be handed to it.
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
                // Bracketed IPv6 - the native parser skips the opening
                // bracket and stops at the closing one.
                var close = candidate.IndexOf(']');
                return TryParseIpv6(
                    close > 0
                        ? candidate.Substring(1, close - 1)
                        : candidate.Substring(1),
                    out address);
            }

            var breakIndex = candidate.IndexOfAny(BreakChars);
            var token = breakIndex >= 0
                ? candidate.Substring(0, breakIndex)
                : candidate;

            var dot = token.IndexOf('.');
            var colon = token.IndexOf(':');
            if (colon >= 0 && (dot < 0 || colon < dot))
            {
                // A colon before any dot marks IPv6, which also covers
                // the IPv4 mapped form.
                return TryParseIpv6(token, out address);
            }
            if (colon >= 0)
            {
                // In an IPv4 value the native parser treats a colon as
                // the end of the address, conventionally a port suffix.
                token = token.Substring(0, colon);
            }
            return TryParseIpv4Strict(token, out address);
        }

        /// <summary>
        /// Parse an IPv4 token gated on a canonical dotted quad. The
        /// round trip rejects the legacy inet_aton notations (octal or
        /// hex components, fewer than four parts, a bare integer) that
        /// IPAddress.TryParse would silently reinterpret as a different
        /// address.
        /// </summary>
        private static bool TryParseIpv4Strict(
            string token,
            out IPAddress address)
        {
            address = null;
            if (token.Length > 0 &&
                IPAddress.TryParse(token, out var parsed) &&
                parsed.AddressFamily == AddressFamily.InterNetwork &&
                parsed.ToString() == token)
            {
                address = parsed;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse an IPv6 token. A zone index is rejected, the native
        /// parser treats '%' as an invalid character, and so must
        /// anything that is not an IPv6 address, a bracketed IPv4 for
        /// example.
        /// </summary>
        private static bool TryParseIpv6(string token, out IPAddress address)
        {
            address = null;
            if (token.Length > 0 &&
                token.IndexOf('%') < 0 &&
                IPAddress.TryParse(token, out var parsed) &&
                parsed.AddressFamily == AddressFamily.InterNetworkV6)
            {
                address = parsed;
                return true;
            }
            return false;
        }
    }
}
