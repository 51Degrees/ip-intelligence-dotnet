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

using FiftyOne.IpIntelligence.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace FiftyOne.IpIntelligence.Tests.Core
{
    [TestClass]
    public class IpEvidenceValueTests
    {
        [TestMethod]
        [DataRow("85.118.2.126", "85.118.2.126")]
        [DataRow("85.118.2.126:53169", "85.118.2.126")]
        [DataRow("85.118.2.126:0", "85.118.2.126")]
        [DataRow(" 85.118.2.126:443 ", "85.118.2.126")]
        [DataRow("2001:db8::9", "2001:db8::9")]
        [DataRow("[2001:db8::9]", "2001:db8::9")]
        [DataRow("[2001:db8::9]:443", "2001:db8::9")]
        [DataRow("::1", "::1")]
        public void TryParse_AcceptedForms(string value, string expected)
        {
            Assert.IsTrue(IpEvidenceValue.TryParse(value, out var address),
                $"'{value}' should parse.");
            Assert.AreEqual(IPAddress.Parse(expected), address);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("not-an-ip")]
        [DataRow("85.118.2.126:banana")]
        [DataRow("85.118.2.126:99999")]
        [DataRow("85.118.2.126:")]
        [DataRow("300.1.2.3:80")]
        [DataRow("[2001:db8::9")]
        [DataRow("[2001:db8::9]443")]
        [DataRow("[2001:db8::9]:")]
        [DataRow("[]:443")]
        [DataRow("1.2.3.4, 5.6.7.8")]
        public void TryParse_RejectedForms(string value)
        {
            Assert.IsFalse(IpEvidenceValue.TryParse(value, out var address),
                $"'{value}' should not parse.");
            Assert.IsNull(address);
        }

        /// <summary>
        /// An IPv6 address with an unbracketed port suffix is inherently
        /// ambiguous and parses as an address including the trailing
        /// group. Documented behaviour, pinned here.
        /// </summary>
        [TestMethod]
        public void TryParse_UnbracketedIpv6WithPort_ParsesAsAddress()
        {
            Assert.IsTrue(IpEvidenceValue.TryParse("2001:db8::9:443", out var address));
            Assert.AreEqual(IPAddress.Parse("2001:db8::9:443"), address);
        }
    }
}
