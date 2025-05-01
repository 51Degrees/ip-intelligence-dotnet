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

using FiftyOne.Common.TestHelpers;
using FiftyOne.IpIntelligence.Cloud.Data;
using FiftyOne.IpIntelligence.Cloud.FlowElements;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Engines.TestHelpers;
using FiftyOne.Pipeline.CloudRequestEngine.Data;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;

namespace FiftyOne.IpIntelligence.Cloud.Tests
{
    [TestClass]
    public class IpIntelligenceCloudEngineTests
    {
        private IPipeline _pipeline;
        private const string _resource_key_env_variable = "51D_RESOURCE_KEY";

        [TestInitialize]
        public void Init()
        {

        }

        /// <summary>
        /// Perform a simple gross error check by calling the cloud service
        /// with a single IP address and validating the IP data is correct.
        /// This is an integration test that uses the live cloud service
        /// so any problems with that service could affect the result
        /// of this test.
        /// </summary>
        [TestMethod]
        public void CloudIntegrationTest()
        {
            var resourceKey = System.Environment.GetEnvironmentVariable(
                _resource_key_env_variable);
        
            if (resourceKey != null)
            {
                _pipeline = new IpiPipelineBuilder(
                    new LoggerFactory(), new System.Net.Http.HttpClient())
                    .UseCloud(resourceKey)
                    .Build();
                var data = _pipeline.CreateFlowData();
                data.AddEvidence("query.client-ip-51d",
                    "185.28.167.78");
                data.Process();
        
                var ipData = data.Get<IIpIntelligenceData>();
                Assert.IsNotNull(ipData);
                Assert.IsTrue(ipData.CountryCode.HasValue);
            }
            else
            {
                Assert.Inconclusive($"No resource key supplied in " +
                    $"environment variable '{_resource_key_env_variable}'");
            }
        }


        // TODO - The commented out section below is more how we should be testing. 
        // i.e. not relying on the cloud request engine actually making a request.
        // Unfortunatley, everything its too tightly coupled for this to work right now.
        // 
        // private IpiCloudEngine _engine;
        // private TestLoggerFactory _loggerFactory;
        // private Mock<IPipeline> _pipeline;
        // 
        // private int _maxWarnings = 0;
        // private int _maxErrors = 0;
        // private string _testJson = "";
        // 
        // [TestInitialize]
        // public void Init()
        // {
        //     _loggerFactory = new TestLoggerFactory();
        //     _engine = new IpiCloudEngine(
        //         _loggerFactory.CreateLogger<IpiCloudEngine>(),
        //         CreateIpData);
        //     _pipeline = new Mock<IPipeline>();
        // }
        // 
        // private IpDataCloud CreateIpData(IPipeline pipeline, FlowElementBase<IpDataCloud, IAspectPropertyMetaData> engine)
        // {
        //     return new IpDataCloud(
        //         _loggerFactory.CreateLogger<IpDataCloud>(),
        //         pipeline,
        //         (IAspectEngine)engine,
        //         MissingPropertyService.Instance);
        // }
        // private CloudRequestData CreateTestData(IPipeline pipeline)
        // {
        //     var data = new CloudRequestData(_loggerFactory.CreateLogger<CloudRequestData>(), pipeline, null);
        //     data.JsonResponse = _testJson;
        //     return data;
        // }
        // 
        // [TestCleanup]
        // public void Cleanup()
        // {
        //     _loggerFactory.AssertMaxErrors(_maxErrors);
        //     _loggerFactory.AssertMaxWarnings(_maxWarnings);
        // }
        // 
        // [TestMethod]
        // public void TestMethod1()
        // {
        //     _testJson = @"{
        //         ""ip"": {
        //         ""IpRangeStart"": null,
        //         ""IpRangeEnd"": null,
        //         ""AverageLocation"": null,
        //         ""LocationBoundNorthWest"": null,
        //         ""LocationBoundSouthEast"": null,
        //         ""CountryCode"": null,
        //         ""RegionName"": null,
        //         ""StateName"": null,
        //         ""CountyName"": null,
        //         ""CityName"": null,
        //         ""TimezoneName"": null,
        //         ""IspName"": null,
        //         ""IspCountryCode"": null,
        //         ""ConnectivityType"": null,
        //         ""MCC"": null,
        //         ""MNC"": null,
        //         ""ASNumber"": null,
        //         ""AbuseContactName"": null,
        //         ""AbuseContactEmail"": null,
        //         ""AbuseContactPhone"": null,
        //         ""NetworkId"": null,
        //       },
        //       ""nullValueReasons"": {
        //         ""IpRangeStart"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""IpRangeEnd"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""AverageLocation"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""LocationBoundNorthWest"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""LocationBoundSouthEast"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""CountryCode"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""RegionName"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""StateName"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""CountyName"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""CityName"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""TimezoneName"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""IspName"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""IspCountryCode"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""ConnectivityType"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""MCC"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""MNC"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""ASNumber"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""AbuseContactName"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""AbuseContactEmail"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""AbuseContactPhone"": ""The results are empty. This is probably because we don't have this data in our database."",
        //         ""NetworkId"": ""The results are empty. This is probably because we don't have this data in our database."",
        //       }
        //     }";
        //     TestPipeline pipeline = new TestPipeline(_pipeline.Object);
        //     TestFlowData testData = new TestFlowData(_loggerFactory.CreateLogger<TestFlowData>(), pipeline);
        // 
        //     testData.GetOrAdd("cloudrequestdata", CreateTestData);
        //     _engine.Process(testData);
        // 
        //     var result = testData.Get<IIpIntelligenceData>();
        //     Assert.IsNotNull(result);
        // }
    }
}
