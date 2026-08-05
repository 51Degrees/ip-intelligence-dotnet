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
using System.Net.Sockets;

namespace FiftyOne.IpIntelligence.Engine.OnPremise
{
    /// <summary>
    /// Parses the client IP address out of an evidence value.
    /// <para>
    /// Client IP evidence does not arrive in one shape. An edge may forward
    /// it as a comma-separated list, and a connection-derived value routinely
    /// carries a port (`82.132.237.238:34947`). Bare
    /// <see cref="IPAddress.TryParse(string, out IPAddress)"/> rejects both,
    /// so the same request could resolve elsewhere in the pipeline while this
    /// engine saw no usable IP. This is the single tolerant parser used for
    /// all client IP evidence.
    /// </para>
    /// </summary>
    internal static class ClientIpParser
    {
        /// <summary>
        /// Reads an IP address from a raw client-ip evidence value.
        /// </summary>
        /// <param name="rawValue">
        /// The evidence value. May be null, may carry a port, and may be a
        /// comma-separated forwarded list of which the first entry is used.
        /// </param>
        /// <param name="address">The address read from the value.</param>
        /// <param name="addressText">
        /// The canonical textual form of <paramref name="address"/>, for
        /// passing to services that take a string. Always canonical, never
        /// the raw input: .NET accepts an IPv6 zone index ("fe80::1%1")
        /// which the native engine's parser rejects, so forwarding the raw
        /// text would reintroduce the native INCORRECT_IP_ADDRESS_FORMAT
        /// abort this parser exists to prevent. The zone index is stripped
        /// rather than kept: it has no meaning to an IP lookup.
        /// </param>
        /// <returns>True when an address was read.</returns>
        public static bool TryParse(
            string rawValue,
            out IPAddress address,
            out string addressText)
        {
            address = null;
            addressText = null;
            if (string.IsNullOrEmpty(rawValue))
            {
                return false;
            }

            // An edge may forward a chain; the first entry is the client.
            var listSeparatorIndex = rawValue.IndexOf(',');
            var candidate = listSeparatorIndex < 0
                ? rawValue.Trim()
                : rawValue.Substring(0, listSeparatorIndex).Trim();
            if (candidate.Length == 0)
            {
                return false;
            }

            // Brackets are excluded from the fast path below deliberately.
            // On some runtimes IPAddress.TryParse accepts the bracketed IPv6
            // form and silently discards any port on it, so "[2001:db8::1]:443"
            // would parse as an address while the text stayed bracketed and
            // ported. Anything bracketed is handled explicitly and reported
            // in canonical form.
            if (candidate[0] == '[')
            {
                return TryParseBracketed(candidate, out address, out addressText);
            }

            // A bare address. The text reported is the canonical form, not
            // the supplied text - see the addressText doc for why.
            if (IsDottedQuadOrIpV6(candidate) &&
                IPAddress.TryParse(candidate, out address))
            {
                address = StripZoneIndex(address);
                addressText = address.ToString();
                return true;
            }

            // Falls here when an IPv4 address carries a port, which
            // IPAddress.TryParse rejects. An unbracketed IPv6 address keeps
            // its colons: without brackets a trailing ":443" is
            // indistinguishable from part of the address, so no port
            // splitting is attempted for IPv6.
            var portSeparatorIndex = candidate.LastIndexOf(':');
            if (portSeparatorIndex > 0 &&
                IsValidPort(candidate.Substring(portSeparatorIndex + 1)))
            {
                var addressPart = candidate.Substring(0, portSeparatorIndex);
                if (IsDottedQuadOrIpV6(addressPart) &&
                    IPAddress.TryParse(addressPart, out address) &&
                    address.AddressFamily == AddressFamily.InterNetwork)
                {
                    addressText = address.ToString();
                    return true;
                }
            }

            address = null;
            return false;
        }

        /// <summary>
        /// Parses "[address]" or "[address]:port" where the bracketed
        /// address is IPv6.
        /// </summary>
        private static bool TryParseBracketed(
            string candidate,
            out IPAddress address,
            out string addressText)
        {
            address = null;
            addressText = null;

            var closingBracketIndex = candidate.IndexOf(']');
            if (closingBracketIndex < 2)
            {
                return false;
            }

            var afterBracket = candidate.Substring(closingBracketIndex + 1);
            if (afterBracket.Length > 0 &&
                (afterBracket[0] != ':' ||
                IsValidPort(afterBracket.Substring(1)) == false))
            {
                return false;
            }

            var bracketedAddress = candidate.Substring(1, closingBracketIndex - 1);
            if (IPAddress.TryParse(bracketedAddress, out address) &&
                address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                address = StripZoneIndex(address);
                addressText = address.ToString();
                return true;
            }

            address = null;
            return false;
        }

        /// <summary>
        /// Returns the address without its IPv6 zone index ("fe80::1%1" ->
        /// "fe80::1"). A zone index identifies the local interface the
        /// address is reachable through; it has no meaning to an IP lookup,
        /// and the native engine's parser rejects the '%' character.
        /// </summary>
        private static IPAddress StripZoneIndex(IPAddress address)
        {
            return address.AddressFamily == AddressFamily.InterNetworkV6 &&
                address.ScopeId != 0
                ? new IPAddress(address.GetAddressBytes())
                : address;
        }

        /// <summary>
        /// True when the text is a plain dotted-quad IPv4 address, or holds
        /// a ':' and so can only be an IPv6 value. .NET additionally accepts
        /// inet_aton IPv4 forms - a bare number ("3232235521" -> 192.168.0.1),
        /// fewer than four parts ("1.2.3"), and octal octets ("012.1.2.3" ->
        /// 10.1.2.3, which the native engine's atoi reads as 12.1.2.3). None
        /// of those are a client IP: accepting them lets numeric junk count
        /// as a valid address and outrank a real one, and resolves a
        /// different address than the native lookup.
        /// </summary>
        private static bool IsDottedQuadOrIpV6(string text)
        {
            if (text.IndexOf(':') >= 0)
            {
                return true;
            }

            var octets = text.Split('.');
            if (octets.Length != 4)
            {
                return false;
            }

            foreach (var octet in octets)
            {
                if (octet.Length == 0 || octet.Length > 3 ||
                    (octet.Length > 1 && octet[0] == '0'))
                {
                    return false;
                }
                foreach (var character in octet)
                {
                    if (character < '0' || character > '9')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsValidPort(string portText)
        {
            // NumberStyles.None rejects signs and whitespace, so only a
            // plain digit run within ushort range passes.
            return portText.Length > 0 &&
                ushort.TryParse(
                    portText,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out _);
        }
    }
}
