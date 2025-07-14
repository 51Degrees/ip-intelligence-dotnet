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

using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using FiftyOne.Caching;
using FiftyOne.IpIntelligence.TestHelpers;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Constants = FiftyOne.IpIntelligence.TestHelpers.Constants;

namespace FiftyOne.IpIntelligence.Tests.Core
{
    [TestClass]
    public class IpiTests
    {
        private static readonly IEnumerable<bool> AllBools = new bool[] { false, true };

        private static object _lock = new object();
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
            if (ShouldSaveMemory)
            {
                Monitor.Exit(_lock);
            }
        }

        private static IEnumerable<object[]> TestParams
            => from profile in Constants.TestableProfiles
               from useCache in AllBools
               from useLazyLoading in AllBools
               from multiThreaded in AllBools
               select new object[] {
                   Constants.IPI_DATA_FILE_NAME,
                   profile, 
                   useCache, 
                   useLazyLoading, 
                   multiThreaded,
               };

        public static string DisplayNameForTestCase(MethodInfo methodInfo, object[] data)
        {
            var profile = (PerformanceProfiles)data[1];
            var useCache = (((bool)data[2]) ? "" : "No") + "Cache";
            var useLazyLoading = (((bool)data[3]) ? "" : "No") + "LazyLoad";
            var multiThreaded = (((bool)data[4]) ? "Multi" : "Single") + "Thread";
            return $"Ipi-{profile}_{useCache}_{useLazyLoading}_{multiThreaded}";
        }

        private static IpAddressGenerator IP_ADDRESSES = new IpAddressGenerator(
            TestHelpers.Utils.GetFilePath(Constants.IP_FILE_NAME));

        [DataTestMethod]
        [DynamicData(nameof(TestParams), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void Ipi_AllConfigurations_100_IpAddresses(
            string datafileName,
            PerformanceProfiles performanceProfile,
            bool useCache,
            bool useLazyLoading,
            bool multiThreaded)
        {
            TestOnPremise_AllConfigurations_100_IpAddresses(datafileName,
                performanceProfile,
                useCache,
                useLazyLoading,
                multiThreaded);
        }

        /// <summary>
        /// This test will create an IP Intelligence pipeline based on the
        /// supplied parameters, process 1000 IP addresses.
        /// The various parameters allow the test to be run for many
        /// different configurations.
        /// </summary>
        /// <param name="datafileName">
        /// The filename of the data file to use for IP Intelligence.
        /// </param>
        /// <param name="performanceProfile">
        /// The performance profile to use.
        /// </param>
        /// <param name="useCache">
        /// Whether or not to use a c# cache results cache in the pipeline.
        /// </param>
        /// <param name="useLazyLoading">
        /// Whether or not to use the lazy loading feature.
        /// </param>
        /// <param name="multiThreaded">
        /// Whether to use a single thread or multiple threads when 
        /// passing user agents to the pipeline for processing.
        /// </param>
        public void TestOnPremise_AllConfigurations_100_IpAddresses(
            string datafileName,
            PerformanceProfiles performanceProfile,
            bool useCache,
            bool useLazyLoading,
            bool multiThreaded)
        {
            var datafile = TestHelpers.Utils.GetFilePath(datafileName);
            var updateService = new Mock<IDataUpdateService>();

            // Configure the pipeline builder based on the 
            // parameters passed to this method.
            var builder = new IpiPipelineBuilder(
                new NullLoggerFactory(), null, updateService.Object)
                .UseOnPremise(datafile.FullName, null, false)
                .SetPerformanceProfile(performanceProfile)
                .SetShareUsage(false)
                .SetDataFileSystemWatcher(false);
            if (useCache)
            {
                builder.UseResultsCache();
            }
            if (useLazyLoading)
            {
                builder.UseLazyLoading();
            }

            CancellationTokenSource cancellationSource = new CancellationTokenSource();

            using (var pipeline = builder.Build())
            {
                var options = new ParallelOptions()
                {
                    CancellationToken = cancellationSource.Token,
                    // The max parallelism is limited to 8 when the
                    // multiThreaded flag is enabled.
                    // This is because, if it is not limited, the lazy 
                    // loading tests will start all requests almost
                    // immediately and then some will take so long to
                    // complete that they exceed the configured timeout.
                    MaxDegreeOfParallelism = (multiThreaded ? 8 : 1)
                };


                // Create a parallel loop to actually process the IP addresses.
                Parallel.ForEach(IP_ADDRESSES.GetRandomIpAddresses(100), options,
                    (ipaddress) =>
                    {
                        // Create the flow data instance
                        var flowData = pipeline.CreateFlowData();

                        // Add the IP address to the flow data 
                        // and process it
                        flowData.AddEvidence(
                                Pipeline.Core.Constants.EVIDENCE_QUERY_PREFIX +
                                Pipeline.Core.Constants.EVIDENCE_SEPERATOR + "ip", ipaddress)
                            .Process();

                        // Make sure no errors occurred. If any did then
                        // cancel the parallel process.
                        if (flowData.Errors != null)
                        {
                            Assert.AreEqual(0, flowData.Errors.Count,
                                $"Expected no errors but got {flowData.Errors.Count}" +
                                $":{Environment.NewLine}{ReportErrors(flowData.Errors)}");
                            cancellationSource.Cancel();
                        }

                        // Get the IP data instance and access the
                        // Countries property to ensure we can get 
                        // data out.
                        var ipData = flowData.Get<IIpIntelligenceData>();
                        var result = ipData.RegisteredCountry;
                    });
            }
        }

        /// <summary>
        /// Private method to present the given list of FlowError 
        /// instances as a single string.
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static string ReportErrors(IList<IFlowError> errors)
        {
            StringBuilder result = new StringBuilder();
            foreach (var error in errors)
            {
                result.AppendLine($"Error in element '{error.FlowElement.GetType().Name}'");
                AddExceptionToMessage(result, error.ExceptionData);
            }
            return result.ToString();
        }

        private static void AddExceptionToMessage(StringBuilder message, Exception ex, int depth = 0)
        {
            AddToMessage(message, $"{ex.GetType().Name} - {ex.Message}", depth);
            AddToMessage(message, $"{ex.StackTrace}", depth);
            if (ex.InnerException != null)
            {
                AddExceptionToMessage(message, ex.InnerException, depth++);
            }
        }

        private static void AddToMessage(StringBuilder message, string textToAdd, int depth)
        {
            for (int i = 0; i < depth; i++)
            {
                message.Append("   ");
            }
            message.AppendLine(textToAdd);
        }
    }
}
