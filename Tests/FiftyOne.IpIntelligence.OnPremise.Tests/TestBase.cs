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

using FiftyOne.IpIntelligence.TestHelpers;
using FiftyOne.Pipeline.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Constants = FiftyOne.IpIntelligence.TestHelpers.Constants;

namespace FiftyOne.IpIntelligence.OnPremise.Tests
{
    [TestCategory("IpIntelligence")]
    [TestCategory("OnPremise")]
    public class TestsBase
    {
        private static object _lock = new object();

        protected WrapperOnPremise Wrapper { get; private set; } = null;
        protected IpAddressGenerator IpAddresses { get; private set; }

        private static readonly bool ShouldSaveMemory = 
            (IntPtr.Size == 4) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        [TestInitialize]
        public void Init()
        {
            // If the test is running in x86 then we need to take some 
            // extra precautions to prevent occasionally running out
            // of memory.
            if (ShouldSaveMemory)
            {
                // Ensure that only one integration test is running at once.
                Monitor.Enter(_lock);
                // Force garbage collection
                GC.Collect();
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            Wrapper?.Dispose();
            if (ShouldSaveMemory)
            {
                Monitor.Exit(_lock);
            }
        }

        protected void TestInitialize(PerformanceProfiles profile)
        {
            Wrapper = new WrapperOnPremise(
                TestHelpers.Utils.GetFilePath(Constants.IPI_DATA_FILE_NAME),
                profile);
            IpAddresses = new IpAddressGenerator(
                TestHelpers.Utils.GetFilePath(Constants.IP_FILE_NAME));
        }

    }
}
