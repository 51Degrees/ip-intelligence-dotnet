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

using FiftyOne.Common.TestHelpers;
using FiftyOne.IpIntelligence.Cloud.Data;
using FiftyOne.IpIntelligence.Cloud.FlowElements;
using FiftyOne.IpIntelligence.Translation.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FiftyOne.IpIntelligence.CloudTests;

[TestClass]
public class CloudTranslationEngineTests
{
    private IPipeline _pipeline;
    private const string _resource_key_env_variable = "51D_RESOURCE_KEY";

    /// <summary>
    /// Perform a simple gross error check by calling the cloud service
    /// with a single IP address and validating the translation data is correct.
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

            var translationData = data.Get<ICountryCodeTranslationData>();
            Assert.IsNotNull(translationData);
            Assert.IsTrue(translationData.CountryNamesGeographical.HasValue);
        }
        else
        {
            Assert.Inconclusive($"No resource key supplied in " +
                $"environment variable '{_resource_key_env_variable}'");
        }
    }

    /// <summary>
    /// Verify that the IpiCloudTranslationEngine HasLoadedProperties 
    /// returns true since it uses locally defined properties.
    /// </summary>
    [TestMethod]
    public void HasLoadedProperties()
    {
        var loggerFactory = new TestLoggerFactory();
        var engine = new IpiCloudTranslationEngine(
            loggerFactory.CreateLogger<IpiCloudTranslationEngine>(),
            CreateTranslationData);

        Assert.IsTrue(engine.HasLoadedProperties);
    }

    private CloudCountryCodeTranslationData CreateTranslationData(
        IPipeline pipeline,
        FlowElementBase<CloudCountryCodeTranslationData, IAspectPropertyMetaData> engine)
    {
        return new CloudCountryCodeTranslationData(
            new TestLoggerFactory().CreateLogger<CloudCountryCodeTranslationData>(),
            pipeline,
            (IAspectEngine)engine);
    }
}
