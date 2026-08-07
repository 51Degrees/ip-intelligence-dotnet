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

using FiftyOne.IpIntelligence.Engine.OnPremise;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace FiftyOne.IpIntelligence.OnPremise.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ClientIpParser"/>. These need no data file
    /// and no native engine.
    /// </summary>
    [TestClass]
    [TestCategory("IpIntelligence")]
    [TestCategory("OnPremise")]
    [TestCategory("IpEcho")]
    public class ClientIpParserTests
    {
        [DataTestMethod]
        // Bare addresses - the text comes back in canonical form.
        [DataRow("1.2.3.4", "1.2.3.4", "1.2.3.4")]
        [DataRow("2001:db8::1", "2001:db8::1", "2001:db8::1")]
        [DataRow("2001:DB8::1", "2001:db8::1", "2001:db8::1")]
        // A shape .NET accepts but the native engine's parser rejects: the
        // canonical text is what keeps it usable. The zone index is
        // stripped, as it has no meaning to a lookup.
        [DataRow("fe80::1%1", "fe80::1", "fe80::1")]
        // IPv4-mapped IPv6 stays in v6 form, so a v4 client arriving this
        // way echoes as IpV6 with Ip NoValue, and is looked up in v6 form.
        // Pinned so the choice is deliberate; unmapping it would be a
        // parser change, not a test change.
        [DataRow("::ffff:8.8.8.8", "::ffff:8.8.8.8", "::ffff:8.8.8.8")]
        [DataRow("[::ffff:8.8.8.8]:443", "::ffff:8.8.8.8", "::ffff:8.8.8.8")]
        // IPv4 with a port.
        [DataRow("82.132.237.238:34947", "82.132.237.238", "82.132.237.238")]
        [DataRow("1.2.3.4:0", "1.2.3.4", "1.2.3.4")]
        [DataRow("1.2.3.4:65535", "1.2.3.4", "1.2.3.4")]
        // Bracketed IPv6, with and without a port.
        [DataRow("[2001:db8::1]:443", "2001:db8::1", "2001:db8::1")]
        [DataRow("[2001:db8::1]", "2001:db8::1", "2001:db8::1")]
        // Forwarded list - first entry, trimmed.
        [DataRow("1.2.3.4, 5.6.7.8", "1.2.3.4", "1.2.3.4")]
        [DataRow(" 2001:db8::1 , 5.6.7.8", "2001:db8::1", "2001:db8::1")]
        [DataRow("82.132.237.238:34947, 10.0.0.1", "82.132.237.238", "82.132.237.238")]
        // Surrounding whitespace.
        [DataRow(" 1.2.3.4 ", "1.2.3.4", "1.2.3.4")]
        public void TryParse_ValidValue_ReturnsAddress(
            string rawValue,
            string expectedAddress,
            string expectedText)
        {
            var parsed = ClientIpParser.TryParse(
                rawValue, out var address, out var addressText);

            Assert.IsTrue(parsed, $"'{rawValue}' should parse.");
            Assert.AreEqual(IPAddress.Parse(expectedAddress), address);
            Assert.AreEqual(expectedText, addressText);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("not-an-ip")]
        // Out-of-range octet - the value from the original report.
        [DataRow("82.12.343.23")]
        // inet_aton shapes .NET would parse but which are not client IPs:
        // a bare number, fewer than four parts, and an octal octet the
        // native engine would read as decimal.
        [DataRow("12345")]
        [DataRow("3232235521")]
        [DataRow("1.2.3")]
        [DataRow("012.1.2.3")]
        [DataRow("12345:80")]
        // Port out of range, signed, or empty.
        [DataRow("1.2.3.4:65536")]
        [DataRow("1.2.3.4:-1")]
        [DataRow("1.2.3.4:")]
        [DataRow("1.2.3.4:port")]
        // An unbracketed IPv6 address never has a port split off it, so a
        // trailing port-like group makes the whole value unparseable rather
        // than silently reinterpreting the address.
        [DataRow("2001:db8::1:notaport")]
        // Bracket forms that are not "[ipv6]" or "[ipv6]:port".
        [DataRow("[1.2.3.4]:80")]
        [DataRow("[2001:db8::1")]
        [DataRow("[]")]
        [DataRow("[2001:db8::1]443")]
        [DataRow("[2001:db8::1]:")]
        // A list whose first entry is empty is not rescued by later entries;
        // the first entry is the client, and it is missing.
        [DataRow(", 1.2.3.4")]
        public void TryParse_InvalidValue_ReturnsFalseAndNulls(string rawValue)
        {
            var parsed = ClientIpParser.TryParse(
                rawValue, out var address, out var addressText);

            Assert.IsFalse(parsed, $"'{rawValue}' should not parse.");
            Assert.IsNull(address);
            Assert.IsNull(addressText);
        }

        [TestMethod]
        public void TryParse_UnbracketedIpV6_KeepsItsColons()
        {
            // "2001:db8::1:443" is itself a valid IPv6 address; the parser
            // must not strip a "port" from an unbracketed IPv6 value.
            var parsed = ClientIpParser.TryParse(
                "2001:db8::1:443", out var address, out var addressText);

            Assert.IsTrue(parsed);
            Assert.AreEqual(IPAddress.Parse("2001:db8::1:443"), address);
            Assert.AreEqual("2001:db8::1:443", addressText);
        }
    }
}
