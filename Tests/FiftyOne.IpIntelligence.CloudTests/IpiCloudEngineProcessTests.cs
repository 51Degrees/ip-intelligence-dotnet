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
using FiftyOne.Pipeline.CloudRequestEngine.Data;
using FiftyOne.Pipeline.CloudRequestEngine.FlowElements;
using FiftyOne.Pipeline.CloudRequestEngine.Services;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.TypedMap;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.CloudTests;

/// <summary>
/// Unit tests for <see cref="IpiCloudEngine"/> processing, driven by a stub
/// <see cref="ICloudRequestEngine"/> so no live cloud request is made.
/// </summary>
[TestClass]
public class IpiCloudEngineProcessTests
{
    /// <summary>
    /// A failed cloud request leaves the JsonResponse null. The engine must
    /// not throw (the failure is already reported by the CloudRequestEngine),
    /// in particular not the misleading PipelineConfigurationException
    /// diagnosing the pipeline ordering, and its element data must still be
    /// present in the flow data so downstream elements can run.
    /// </summary>
    [TestMethod]
    public void Process_NullJsonResponse_DoesNotThrow()
    {
        using var pipeline = BuildPipeline(null);
        using var data = pipeline.CreateFlowData();

        data.Process();

        AssertProcessedGracefully(data);
    }

    /// <summary>
    /// Same as the null case for an empty JsonResponse.
    /// </summary>
    [TestMethod]
    public void Process_EmptyJsonResponse_DoesNotThrow()
    {
        using var pipeline = BuildPipeline(string.Empty);
        using var data = pipeline.CreateFlowData();

        data.Process();

        AssertProcessedGracefully(data);
    }

    private static void AssertProcessedGracefully(IFlowData data)
    {
        if (data.Errors != null)
        {
            foreach (var error in data.Errors)
            {
                Assert.IsFalse(
                    error.ShouldThrow,
                    $"No throwing error should be recorded when the cloud " +
                    $"request failed, but found: {error.ExceptionData}");
            }
        }
        Assert.IsTrue(
            data.TryGetValue(
                new TypedKey<IElementData>("ip"), out _),
            "The ip element data should be added even when the cloud " +
            "request failed.");
    }

    /// <summary>
    /// A populated response is deserialized into the aspect data as before.
    /// </summary>
    [TestMethod]
    public void Process_PopulatedJsonResponse_PopulatesProperties()
    {
        var json = @"{
            ""ip"": {
                ""asnname"": ""Example Networks""
            }
        }";
        using var pipeline = BuildPipeline(json);
        using var data = pipeline.CreateFlowData();

        data.Process();

        var ipData = data.Get<IIpIntelligenceData>();
        Assert.IsNotNull(ipData);
        Assert.IsTrue(ipData.AsnName.HasValue);
        Assert.AreEqual("Example Networks", ipData.AsnName.Value);
    }

    /// <summary>
    /// Build a pipeline containing a stub cloud request engine returning the
    /// given JSON response followed by the engine under test. Process
    /// exceptions are suppressed, matching how consumers configure cloud
    /// pipelines, so the tests can inspect the recorded errors instead.
    /// </summary>
    private static IPipeline BuildPipeline(string json)
    {
        return new PipelineBuilder(new LoggerFactory())
            .AddFlowElement(new StubRequestEngine(json, IpPublicProperties()))
            .AddFlowElement(NewEngine())
            .SetSuppressProcessExceptions(true)
            .Build();
    }

    /// <summary>
    /// The metadata the cloud advertises for the "ip" section.
    /// </summary>
    private static IReadOnlyDictionary<string, ProductMetaData>
        IpPublicProperties()
    {
        var properties = new List<PropertyMetaData>
        {
            new PropertyMetaData { Name = "AsnName", Type = "String" },
        };
        return new Dictionary<string, ProductMetaData>
        {
            { "ip", new ProductMetaData { Properties = properties } },
        };
    }

    private static IpiCloudEngine NewEngine()
    {
        return new IpiCloudEngine(
            new TestLoggerFactory().CreateLogger<IpiCloudEngine>(),
            CreateIpData);
    }

    private static IpDataCloud CreateIpData(
        IPipeline pipeline,
        FlowElementBase<IpDataCloud, IAspectPropertyMetaData> engine)
    {
        return new IpDataCloud(
            new TestLoggerFactory().CreateLogger<IpDataCloud>(),
            pipeline,
            (IAspectEngine)engine,
            MissingPropertyServiceCloud.Instance);
    }

    /// <summary>
    /// Minimal cloud request engine that advertises a fixed set of properties
    /// and returns a fixed JSON response, so the engine under test can be
    /// driven without contacting the cloud.
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
}
