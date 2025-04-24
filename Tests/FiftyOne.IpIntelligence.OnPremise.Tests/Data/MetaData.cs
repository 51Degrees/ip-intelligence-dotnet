/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2025 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
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

using FiftyOne.IpIntelligence.OnPremise.Tests.Data;
using FiftyOne.IpIntelligence.TestHelpers.Data;
using FiftyOne.Pipeline.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Core.Data
{
    [TestClass]
    [TestCategory("Core")]
    [TestCategory("MetaData")]
    public class MetaDataOnPremiseCoreTests : TestsBase
    {
        private const int TEST_DELAY_MS = 3000;

        private static IEnumerable<object[]> ProfilesToTest
            => TestHelpers.Constants.TestableProfiles
            .Where(x => x != PerformanceProfiles.BalancedTemp)
            .Select(x => new object[] { x });

        public static string DisplayNameForTestCase(MethodInfo methodInfo, object[] data)
            => $"{methodInfo.Name}_{(PerformanceProfiles)data[0]}";

        // Note that the 'ReloadMemory' tests are split into separate methods
        // rather than using the 'DataTestMethod' approach because they 
        // often fail due to running out of memory.
        // This way, they do not run into the same problem for some reason.

        [TestMethod]
        [DynamicData(nameof(ProfilesToTest), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void MetaData_OnPremise_Core_Reload(PerformanceProfiles profile)
        {
            Task.Delay(TEST_DELAY_MS).Wait();
            TestInitialize(profile);
            MetaDataTests test = new MetaDataTests();
            test.Reload(Wrapper, new MetaDataHasher(), profile);
        }

        [TestMethod]
        [DynamicData(nameof(ProfilesToTest), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void MetaData_OnPremise_Core_ReloadMemory(PerformanceProfiles profile)
        {
            Task.Delay(TEST_DELAY_MS).Wait();
            TestInitialize(PerformanceProfiles.LowMemory);
            MetaDataTests test = new MetaDataTests();
            test.ReloadMemory(Wrapper, new MetaDataHasher(), PerformanceProfiles.LowMemory);
        }
    }
}
