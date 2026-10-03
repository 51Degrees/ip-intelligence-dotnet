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

using FiftyOne.IpIntelligence.Engine.OnPremise.Data;
using FiftyOne.IpIntelligence.Engine.OnPremise.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Data
{
    [TestClass]
    public class UTF8StringSwigExtensionsTests
    {
        [TestMethod]
        public void ToUTF8String_CopiesCompleteMultibyteValue()
        {
            const string expected = "Päijät-Häme المنطقة الشرقية";
            using (var source = new UTF8StringSwig(
                Encoding.UTF8.GetBytes(expected)))
            {
                Assert.AreEqual(expected, source.ToUTF8String());
            }
        }

        [TestMethod]
        public void ToUTF8String_EmptyValueIsEmpty()
        {
            using (var source = new UTF8StringSwig())
            {
                Assert.AreEqual(string.Empty, source.ToUTF8String());
            }
        }
    }
}