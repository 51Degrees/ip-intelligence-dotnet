/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2025 51 Degrees Mobile Experts Limited, Davidson House,
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

using FiftyOne.IpIntelligence.TestHelpers.FlowElements;
using FiftyOne.Pipeline.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Core.FlowElements
{
    [TestClass]
    [TestCategory("Core")]
    [TestCategory("EvidenceKeys")]
    public class EvidenceKeysOnPremiseCoreTests : TestsBase
    {
        private static IEnumerable<object[]> ProfilesToTest
            => TestHelpers.Constants.TestableProfiles
            .Where(x => x != PerformanceProfiles.BalancedTemp)
            .Select(x => new object[] { x });

        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        public void EvidenceKeys_OnPremise_Core_ContainsIpAddress(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            EvidenceKeyTests.ContainsIpAddress(Wrapper);
        }
        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        public void EvidenceKeys_OnPremise_Core_CaseInsensitiveKeys(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            EvidenceKeyTests.CaseInsensitiveKeys(Wrapper);
        }
    }
}
