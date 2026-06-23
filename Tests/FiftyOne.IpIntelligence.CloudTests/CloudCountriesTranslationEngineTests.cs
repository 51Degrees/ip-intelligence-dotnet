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
using FiftyOne.IpIntelligence.Translation;
using FiftyOne.IpIntelligence.Translation.Data;
using FiftyOne.Pipeline.CloudRequestEngine.Data;
using FiftyOne.Pipeline.CloudRequestEngine.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.IpIntelligence.CloudTests;

[TestClass]
public class CloudCountriesTranslationEngineTests
{
    private IPipeline _pipeline;
    // The aligned environment variable name is checked first. The legacy
    // names are retained for backwards compatibility.
    private const string _resource_key_env_variable = "_51DEGREES_RESOURCE_KEY";
    private const string _legacy_resource_key_env_variable = "51D_RESOURCE_KEY";
    private const string _super_resource_key_env_variable = "SUPER_RESOURCE_KEY";

    /// <summary>
    /// Perform a simple gross error check by calling the cloud service
    /// with a single IP address and validating the translation data is correct.
    /// This is an integration test that uses the live cloud service
    /// so any problems with that service could affect the result
    /// of this test. The default IpiPipelineBuilder does not include the
    /// translation engine (neither cloud nor on-premise builders wire the
    /// translation engines automatically), so the pipeline is assembled here
    /// with a CloudRequestEngine followed by the engine under test.
    /// </summary>
    [TestMethod]
    public void CloudIntegrationTest()
    {
        var resourceKey = System.Environment.GetEnvironmentVariable(
            _resource_key_env_variable)
            ?? System.Environment.GetEnvironmentVariable(
                _legacy_resource_key_env_variable)
            ?? System.Environment.GetEnvironmentVariable(
                _super_resource_key_env_variable);

        if (resourceKey != null)
        {
            var loggerFactory = new LoggerFactory();
            _pipeline = new PipelineBuilder(loggerFactory)
                .AddFlowElement(
                    new CloudRequestEngineBuilder(
                        loggerFactory, new System.Net.Http.HttpClient())
                    .SetResourceKey(resourceKey)
                    .Build())
                .AddFlowElement(
                    new CloudCountriesTranslationEngineBuilder(loggerFactory)
                    .Build())
                .Build();
            var data = _pipeline.CreateFlowData();
            data.AddEvidence("query.client-ip-51d",
                "185.28.167.78");
            data.Process();

            var translationData = data.Get<ICountriesTranslationData>();
            Assert.IsNotNull(translationData);
            Assert.IsTrue(translationData.CountryNamesGeographicalTranslated.HasValue);
        }
        else
        {
            Assert.Inconclusive($"No resource key supplied in " +
                $"environment variable '{_resource_key_env_variable}'");
        }
    }

    /// <summary>
    /// The cloud engine is the counterpart of the on-premise
    /// CountriesTranslationEngine, so it must consume the same element-data
    /// key ("countrynamestranslated").
    /// </summary>
    [TestMethod]
    public void ElementDataKey_IsCountryNamesTranslated()
    {
        Assert.AreEqual(
            Constants.CountryNamesTranslatedKey,
            NewEngine().ElementDataKey);
    }

    /// <summary>
    /// The engine is metadata-driven: it loads its property list from the cloud
    /// request engine's metadata. GetPropertyType must map the cloud type
    /// strings to the correct .NET types - "WeightedString" to a weighted list
    /// and "Array" to a plain string list.
    /// </summary>
    [TestMethod]
    public void Properties_MappedFromCloudMetadata()
    {
        using var pipeline = BuildPipeline("{}", CountriesPublicProperties());
        var props = pipeline.GetElement<CloudCountriesTranslationEngine>()
            .Properties.ToDictionary(p => p.Name, p => p.Type);

        // Metadata-driven engines expose the concrete AspectPropertyValue<>
        // type (matching IpiCloudEngine), not the interface.
        var weighted =
            typeof(AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>);
        var plain =
            typeof(AspectPropertyValue<IReadOnlyList<string>>);

        Assert.AreEqual(6, props.Count);
        Assert.AreEqual(weighted, props["CountryNamesGeographicalTranslated"]);
        Assert.AreEqual(weighted, props["CountryNamesPopulationTranslated"]);
        Assert.AreEqual(plain, props["CountryNamesGeographicalAllTranslated"]);
        Assert.AreEqual(plain, props["CountryNamesPopulationAllTranslated"]);
        Assert.AreEqual(plain, props["CountryCodesGeographicalAll"]);
        Assert.AreEqual(plain, props["CountryCodesPopulationAll"]);
    }

    /// <summary>
    /// Process a cloud JSON response and confirm both a weighted property and a
    /// plain string-list property are populated correctly from the
    /// "countrynamestranslated" section.
    /// </summary>
    [TestMethod]
    public void ProcessEngine_PopulatesWeightedAndPlainProperties()
    {
        var json = @"{
            ""countrynamestranslated"": {
                ""CountryNamesGeographicalTranslated"": [
                    { ""Value"": ""Germany"", ""RawWeighting"": 65535 }
                ],
                ""CountryCodesGeographicalAll"": [ ""DE"", ""FR"" ]
            }
        }";

        var result = Process(json);

        Assert.IsNotNull(result);

        Assert.IsTrue(result.CountryNamesGeographicalTranslated.HasValue);
        var weighted = result.CountryNamesGeographicalTranslated.Value;
        Assert.AreEqual(1, weighted.Count);
        Assert.AreEqual("Germany", weighted[0].Value);

        Assert.IsTrue(result.CountryCodesGeographicalAll.HasValue);
        CollectionAssert.AreEqual(
            new[] { "DE", "FR" },
            result.CountryCodesGeographicalAll.Value.ToList());
    }

    /// <summary>
    /// When the resource key does not grant the translation properties the cloud
    /// returns no metadata for them. Like IpiCloudEngine and
    /// DeviceDetectionCloudEngine, the engine must then fail fast at pipeline
    /// build time with a clear PipelineException rather than starting and
    /// failing later per request.
    /// </summary>
    [TestMethod]
    public void Build_WithoutTranslationProperties_FailsFast()
    {
        var ex = Assert.ThrowsExactly<PipelineException>(() =>
            BuildPipeline("{}", new Dictionary<string, ProductMetaData>()));

        StringAssert.Contains(ex.Message, Constants.CountryNamesTranslatedKey);
    }

    /// <summary>
    /// Build a pipeline containing a stub cloud request engine (supplying the
    /// given property metadata and JSON response) followed by the engine under
    /// test.
    /// </summary>
    private IPipeline BuildPipeline(
        string json,
        IReadOnlyDictionary<string, ProductMetaData> publicProperties)
    {
        return new PipelineBuilder(new LoggerFactory())
            .AddFlowElement(new StubRequestEngine(json, publicProperties))
            .AddFlowElement(NewEngine())
            .Build();
    }

    /// <summary>
    /// Run the engine inside a real pipeline against a supplied cloud JSON
    /// response and return the populated translation data. A stub
    /// <see cref="ICloudRequestEngine"/> placed ahead of the engine supplies
    /// the metadata and JSON, mirroring the live CloudRequestEngine.
    /// </summary>
    private ICountriesTranslationData Process(string json)
    {
        using var pipeline = BuildPipeline(json, CountriesPublicProperties());
        var data = pipeline.CreateFlowData();
        data.Process();
        return data.Get<ICountriesTranslationData>();
    }

    /// <summary>
    /// The metadata the cloud advertises for the "countrynamestranslated"
    /// section: two weighted lists and four plain string ("Array") lists.
    /// </summary>
    private static IReadOnlyDictionary<string, ProductMetaData>
        CountriesPublicProperties()
    {
        var properties = new List<PropertyMetaData>
        {
            new PropertyMetaData { Name = "CountryNamesGeographicalTranslated", Type = "WeightedString" },
            new PropertyMetaData { Name = "CountryNamesPopulationTranslated", Type = "WeightedString" },
            new PropertyMetaData { Name = "CountryNamesGeographicalAllTranslated", Type = "Array" },
            new PropertyMetaData { Name = "CountryNamesPopulationAllTranslated", Type = "Array" },
            new PropertyMetaData { Name = "CountryCodesGeographicalAll", Type = "Array" },
            new PropertyMetaData { Name = "CountryCodesPopulationAll", Type = "Array" },
        };
        return new Dictionary<string, ProductMetaData>
        {
            { Constants.CountryNamesTranslatedKey, new ProductMetaData { Properties = properties } },
        };
    }

    /// <summary>
    /// Minimal cloud request engine that advertises a fixed set of properties
    /// and returns a fixed JSON response, so the engine under test can be driven
    /// without contacting the cloud.
    /// </summary>
    private class StubRequestEngine
        : AspectEngineBase<CloudRequestData, IAspectPropertyMetaData>,
            ICloudRequestEngine
    {
        private readonly string _json;

        public StubRequestEngine(
            string json,
            IReadOnlyDictionary<string, ProductMetaData> publicProperties)
            : base(new LoggerFactory().CreateLogger<StubRequestEngine>(), CreateData)
        {
            _json = json;
            PublicProperties = publicProperties;
        }

        private static CloudRequestData CreateData(
            IPipeline pipeline,
            FlowElementBase<CloudRequestData, IAspectPropertyMetaData> element) =>
            new CloudRequestData(null, pipeline, element as IAspectEngine);

        public IReadOnlyDictionary<string, ProductMetaData> PublicProperties
        { get; }

        public override string DataSourceTier => "";
        public override string ElementDataKey => "cloud";
        public override IEvidenceKeyFilter EvidenceKeyFilter =>
            new EvidenceKeyFilterWhitelist(new List<string>());
        public override IList<IAspectPropertyMetaData> Properties =>
            new List<IAspectPropertyMetaData>();

        protected override void ProcessEngine(
            IFlowData data, CloudRequestData aspectData) =>
            aspectData.JsonResponse = _json;

        protected override void UnmanagedResourcesCleanup() { }
    }

    private CloudCountriesTranslationEngine NewEngine()
    {
        return new CloudCountriesTranslationEngine(
            new TestLoggerFactory().CreateLogger<CloudCountriesTranslationEngine>(),
            CreateTranslationData);
    }

    private CloudCountriesTranslationData CreateTranslationData(
        IPipeline pipeline,
        FlowElementBase<CloudCountriesTranslationData, IAspectPropertyMetaData> engine)
    {
        return new CloudCountriesTranslationData(
            new TestLoggerFactory().CreateLogger<CloudCountriesTranslationData>(),
            pipeline,
            (IAspectEngine)engine);
    }
}
