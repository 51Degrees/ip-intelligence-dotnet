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

using FiftyOne.IpIntelligence.OnPremise.Tests.Data;
using FiftyOne.IpIntelligence.TestHelpers.FlowElements;
using FiftyOne.Pipeline.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Core.FlowElements
{
    [TestClass]
    [TestCategory("Core")]
    [TestCategory("Process")]
    public class ProcessOnPremiseCoreTests : TestsBase
    {
        private static IEnumerable<object[]> ProfilesToTest
            => TestHelpers.Constants.TestableProfiles
            .Where(x => x != PerformanceProfiles.BalancedTemp)
            .Select(x => new object[] { x });

        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        public void Process_OnPremise_Core_NoEvidence(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            ProcessTests.NoEvidence(Wrapper, new DataValidatorOnPremise(Wrapper.Engine));
        }

        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        public void Process_OnPremise_Core_EmptyIpAddress(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            ProcessTests.EmptyIpAddress(Wrapper, new DataValidatorOnPremise(Wrapper.Engine));
        }

        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        public void Process_OnPremise_Core_NoHeaders(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            ProcessTests.NoHeaders(Wrapper, new DataValidatorOnPremise(Wrapper.Engine));
        }

        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        public void Process_OnPremise_Core_NoUsefulHeaders(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            ProcessTests.NoUsefulHeaders(Wrapper, new DataValidatorOnPremise(Wrapper.Engine));
        }

        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        public void Process_OnPremise_Core_CaseInsensitiveKeys(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            ProcessTests.CaseInsensitiveEvidenceKeys(Wrapper, new DataValidatorOnPremise(Wrapper.Engine));
        }

        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        [Ignore("IDs are not finalized yet.")]
        public void Process_OnPremise_Core_MetaDataService_DefaultProfilesIds(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            ProcessTests.MetaDataService_DefaultProfilesIds(Wrapper);
        }

        // TODO: This loads all profiles into memory and does not free quick enough
        // causing memory to be used up quickly. Comment out until we find a
        // better solution. Potentially we can move all of these pieces to the 
        // unmanaged layer.
        //
        // [DataTestMethod]
        // [DynamicData(nameof(ProfilesToTest))]
        // public void Process_OnPremise_Core_MetaDataService_ComponentIdForProfile(PerformanceProfiles profile)
        // {
        //     TestInitialize(profile);
        //     ProcessTests.MetaDataService_ComponentIdForProfile(Wrapper);
        // }

        [DataTestMethod]
        [DynamicData(nameof(ProfilesToTest))]
        [Ignore("IDs are not finalized yet.")]
        public void Process_OnPremise_Core_MetaDataService_DefaultProfileIdForComponent(PerformanceProfiles profile)
        {
            TestInitialize(profile);
            ProcessTests.MetaDataService_DefaultProfileIdForComponent(Wrapper);
        }
    }
}
