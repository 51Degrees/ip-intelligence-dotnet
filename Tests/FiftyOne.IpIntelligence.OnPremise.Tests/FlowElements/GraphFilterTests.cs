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
using FiftyOne.IpIntelligence.Engine.OnPremise.Data;
using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.IpIntelligence.TestHelpers;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Caching;
using FiftyOne.Pipeline.Engines.Configuration;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Constants = FiftyOne.IpIntelligence.TestHelpers.Constants;
using Messages = FiftyOne.IpIntelligence.Engine.OnPremise.Messages;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.FlowElements
{
    /// <summary>
    /// An engine that forwards a fixed set of required property indexes to
    /// the filtered ProcessEngine, standing in for a caller that knows which
    /// properties it will read. It declares that it filters graphs, as such
    /// a subclass must.
    /// </summary>
    internal class FilteredIpiEngine : IpiOnPremiseEngine
    {
        /// <summary>
        /// Indexes passed to the filtered overload on every request. Null
        /// evaluates every graph, an empty array evaluates none.
        /// </summary>
        public int[] Indexes { get; set; }

        internal FilteredIpiEngine(
            ILoggerFactory loggerFactory,
            Func<IPipeline,
                FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise> ipDataFactory,
            string tempDataFilePath)
            : base(loggerFactory, ipDataFactory, tempDataFilePath)
        {
        }

        protected override bool FiltersGraphs => true;

        protected override void ProcessEngine(IFlowData data, IIpDataOnPremise ipData)
        {
            ProcessEngine(data, ipData, Indexes);
        }
    }

    /// <summary>
    /// A subclass that calls the filtered ProcessEngine without declaring
    /// that it filters graphs, which the engine must refuse.
    /// </summary>
    internal class UndeclaredFilterIpiEngine : IpiOnPremiseEngine
    {
        /// <summary>
        /// Indexes passed to the filtered overload on every request.
        /// </summary>
        public int[] Indexes { get; set; }

        internal UndeclaredFilterIpiEngine(
            ILoggerFactory loggerFactory,
            Func<IPipeline,
                FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise> ipDataFactory,
            string tempDataFilePath)
            : base(loggerFactory, ipDataFactory, tempDataFilePath)
        {
        }

        protected override void ProcessEngine(IFlowData data, IIpDataOnPremise ipData)
        {
            ProcessEngine(data, ipData, Indexes);
        }
    }

    /// <summary>
    /// Builder for a test engine, identical to the standard builder except
    /// for the engine type it creates. It keeps the engine it created so a
    /// test can check what happened to it when the build fails.
    /// </summary>
    internal class TestIpiEngineBuilder<TEngine>
        : IpiOnPremiseEngineBuilderBase<TEngine>
        where TEngine : IpiOnPremiseEngine
    {
        private readonly Func<ILoggerFactory, Func<IPipeline,
                FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise>, string, TEngine>
            _engineFactory;

        /// <summary>
        /// The engine created by the last build, whether or not the build
        /// completed.
        /// </summary>
        public TEngine CreatedEngine { get; private set; }

        public TestIpiEngineBuilder(
            ILoggerFactory loggerFactory,
            Func<ILoggerFactory, Func<IPipeline,
                FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise>, string, TEngine> engineFactory)
            : base(loggerFactory, null)
        {
            _engineFactory = engineFactory;
        }

        protected override TEngine CreateEngine(
            ILoggerFactory loggerFactory,
            Func<IPipeline,
                FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise> ipDataFactory,
            string tempDataFilePath)
        {
            CreatedEngine = _engineFactory(
                loggerFactory,
                ipDataFactory,
                tempDataFilePath);
            return CreatedEngine;
        }
    }

    /// <summary>
    /// Tests for the filtered ProcessEngine overload and the
    /// RequiredPropertyIndexes map that feeds it.
    /// </summary>
    [TestClass]
    [TestCategory("Core")]
    [TestCategory("GraphFilter")]
    public class GraphFilterTests
    {
        private static readonly TestLoggerFactory _logger = new TestLoggerFactory();

        // A public address from the evidence file that ships with the data.
        private const string IpAddress = "50.154.29.201";

        private FilteredIpiEngine _engine;
        private IPipeline _pipeline;

        /// <summary>
        /// The enterprise file when present, otherwise the Lite file the
        /// native tests use, so the tests run on either. Looked up by walking
        /// up from the test assembly to the data folders the repository
        /// keeps, because a recursive search of the whole engine folder can
        /// time out once native build output is present.
        /// </summary>
        private static FileInfo DataFile()
        {
            var names = new[] { Constants.IPI_DATA_FILE_NAME, "51Degrees-LiteV41.ipi" };
            var folders = new[]
            {
                "ip-intelligence-data",
                Path.Combine(
                    "FiftyOne.IpIntelligence.Engine.OnPremise",
                    "ip-intelligence-cxx",
                    "ip-intelligence-data")
            };
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                foreach (var name in names)
                {
                    foreach (var folder in folders)
                    {
                        var candidate = new FileInfo(Path.Combine(current.FullName, folder, name));
                        if (candidate.Exists)
                        {
                            return candidate;
                        }
                    }
                }
                current = current.Parent;
            }
            Assert.Inconclusive("No IP intelligence data file was found.");
            return null;
        }

        [TestInitialize]
        public void Init()
        {
            _engine = NewBuilder(FilteredEngine)
                .Build(DataFile().FullName, false);
            _pipeline = new PipelineBuilder(_logger).AddFlowElement(_engine).Build();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _pipeline?.Dispose();
            _engine?.Dispose();
        }

        private IIpDataOnPremise Detect()
        {
            return Detect(_pipeline);
        }

        private static IIpDataOnPremise Detect(IPipeline pipeline)
        {
            var data = pipeline.CreateFlowData();
            data.AddEvidence("query.client-ip", IpAddress);
            data.Process();
            return data.Get<IIpDataOnPremise>();
        }

        /// <summary>
        /// Every required property's values as text, so two results can be
        /// compared even after the cache has handed back the same instance.
        /// </summary>
        private static Dictionary<string, string> Snapshot(
            IpiOnPremiseEngine engine,
            IIpDataOnPremise ip)
        {
            return engine.RequiredPropertyIndexes.Keys.ToDictionary(
                name => name,
                name =>
                {
                    var values = ip.GetValues(name);
                    return values.HasValue ?
                        string.Join("|", values.Value) :
                        "no value";
                },
                StringComparer.OrdinalIgnoreCase);
        }

        private static void AssertSameValues(
            Dictionary<string, string> expected,
            Dictionary<string, string> actual)
        {
            Assert.HasCount(expected.Count, actual);
            foreach (var pair in expected)
            {
                Assert.AreEqual(pair.Value, actual[pair.Key], pair.Key);
            }
        }

        private static FilteredIpiEngine FilteredEngine(
            ILoggerFactory loggerFactory,
            Func<IPipeline,
                FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise> ipDataFactory,
            string tempDataFilePath)
        {
            return new FilteredIpiEngine(
                loggerFactory,
                ipDataFactory,
                tempDataFilePath);
        }

        private static UndeclaredFilterIpiEngine UndeclaredEngine(
            ILoggerFactory loggerFactory,
            Func<IPipeline,
                FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise> ipDataFactory,
            string tempDataFilePath)
        {
            return new UndeclaredFilterIpiEngine(
                loggerFactory,
                ipDataFactory,
                tempDataFilePath);
        }

        /// <summary>
        /// A builder for a test engine with the settings every test here
        /// uses. Balanced rather than the in memory default, so a test does
        /// not hold the whole data file, which is several gigabytes for the
        /// enterprise file.
        /// </summary>
        private static TestIpiEngineBuilder<TEngine> NewBuilder<TEngine>(
            Func<ILoggerFactory, Func<IPipeline,
                FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise>, string, TEngine> engineFactory)
            where TEngine : IpiOnPremiseEngine
        {
            var builder = new TestIpiEngineBuilder<TEngine>(
                _logger,
                engineFactory);
            builder
                .SetPerformanceProfile(PerformanceProfiles.Balanced)
                .SetAutoUpdate(false)
                .SetDataFileSystemWatcher(false);
            return builder;
        }

        /// <summary>
        /// Builds a standard engine, which does not filter graphs, with a
        /// results cache configured on the builder, which is how a
        /// deployment turns the cache on, and runs the test against it.
        /// Cache hits are flagged on the results.
        /// </summary>
        private static void WithStandardEngineCache(
            Action<IpiOnPremiseEngine, IPipeline> test,
            LazyLoadingConfiguration lazyLoading = null)
        {
            var builder = new IpiOnPremiseEngineBuilder(_logger)
                .SetPerformanceProfile(PerformanceProfiles.Balanced)
                .SetAutoUpdate(false)
                .SetDataFileSystemWatcher(false)
                .SetCache(new CacheConfiguration() { Size = 10 })
                .SetCacheHitOrMiss(true);
            if (lazyLoading != null)
            {
                builder.SetLazyLoading(lazyLoading);
            }
            using (var engine = builder.Build(DataFile().FullName, false))
            using (var pipeline = new PipelineBuilder(_logger)
                .AddFlowElement(engine)
                .Build())
            {
                test(engine, pipeline);
            }
        }

        /// <summary>
        /// True if the exception, or one it wraps, is an
        /// InvalidOperationException with the message given. Matched on the
        /// message so another InvalidOperationException, such as
        /// ObjectDisposedException, does not count.
        /// </summary>
        private static bool IsRefusal(Exception exception, string message)
        {
            if (exception == null)
            {
                return false;
            }
            if (exception is InvalidOperationException &&
                exception.Message == message)
            {
                return true;
            }
            if (exception is AggregateException aggregate &&
                aggregate.InnerExceptions.Any(
                    inner => IsRefusal(inner, message)))
            {
                return true;
            }
            return IsRefusal(exception.InnerException, message);
        }

        /// <summary>
        /// Names of properties in the required list, one per component, in
        /// the order the components appear. Metric properties are not in the
        /// native list and are left out, as are properties that are mandatory
        /// with a default value, because those read as the default when their
        /// component produced no profile, exactly as for an unmatched
        /// component today.
        /// </summary>
        private List<string> OnePropertyPerComponent()
        {
            var map = _engine.RequiredPropertyIndexes;
            return _engine.Properties
                .Where(p => map.ContainsKey(p.Name) && p.Component != null)
                .Where(p => p.Mandatory == false || p.DefaultValue == null)
                .GroupBy(p => p.Component.Name)
                .Select(g => g.First().Name)
                .ToList();
        }

        [TestMethod]
        public void GraphFilter_RequiredPropertyIndexes_OnePerBuiltProperty()
        {
            var map = _engine.RequiredPropertyIndexes;
            Assert.IsNotEmpty(map);
            var first = map.Keys.First();
            Assert.IsTrue(map.ContainsKey(first.ToUpperInvariant()), "Lookup must ignore case.");
            Assert.HasCount(map.Count, map.Values.Distinct().ToList(), "Indexes must be distinct.");
            Assert.IsTrue(map.Values.All(i => i >= 0 && i < map.Count));
        }

        [TestMethod]
        public void GraphFilter_Null_MatchesEveryIndex()
        {
            var names = OnePropertyPerComponent();
            _engine.Indexes = null;
            var fromNull = Detect();
            // Passing every index must give the same answer as passing null.
            _engine.Indexes = _engine.RequiredPropertyIndexes.Values.ToArray();
            var fromAll = Detect();
            foreach (var name in names)
            {
                var a = fromAll.GetValues(name);
                var n = fromNull.GetValues(name);
                Assert.AreEqual(a.HasValue, n.HasValue, name);
                if (a.HasValue && n.HasValue)
                {
                    CollectionAssert.AreEqual(a.Value.ToList(), n.Value.ToList(), name);
                }
            }
        }

        [TestMethod]
        public void GraphFilter_OneProperty_GivesOnlyItsComponent()
        {
            var map = _engine.RequiredPropertyIndexes;
            var byComponent = _engine.Properties
                .Where(p => map.ContainsKey(p.Name) && p.Component != null)
                .GroupBy(p => p.Component.Name)
                .ToList();
            if (byComponent.Count < 2)
            {
                Assert.Inconclusive("The data file has properties on one component only.");
            }
            var kept = byComponent[0].First().Name;
            _engine.Indexes = null;
            var all = Snapshot(_engine, Detect());
            _engine.Indexes = new int[0];
            var none = Snapshot(_engine, Detect());
            _engine.Indexes = new[] { map[kept] };
            var some = Snapshot(_engine, Detect());
            // Every property on the evaluated component matches the
            // unfiltered detection.
            foreach (var property in byComponent[0])
            {
                Assert.AreEqual(all[property.Name], some[property.Name], property.Name);
            }
            // Every property on another component reads as it does when no
            // graph is evaluated, which is no value or its mandatory default.
            var skipped = byComponent.Skip(1)
                .SelectMany(g => g)
                .Select(p => p.Name)
                .ToList();
            foreach (var name in skipped)
            {
                Assert.AreEqual(none[name], some[name], name);
            }
            Assert.IsTrue(skipped.Any(name => all[name] != none[name]),
                "The address must give another component a value when " +
                "unfiltered, or the checks above prove nothing.");
        }

        [TestMethod]
        public void GraphFilter_Empty_GivesNoValue()
        {
            _engine.Indexes = null;
            var all = Snapshot(_engine, Detect());
            _engine.Indexes = new int[0];
            var ip = Detect();
            var none = Snapshot(_engine, ip);
            // Properties without a mandatory default have no value.
            foreach (var name in OnePropertyPerComponent())
            {
                Assert.IsFalse(ip.GetValues(name).HasValue, name);
            }
            // Properties with one read as that default, so compare with the
            // unfiltered detection to show the graphs were skipped.
            Assert.IsTrue(all.Keys.Any(name => all[name] != none[name]),
                "No value changed when every graph was skipped.");
            // Indexes that are all out of range are ignored, which leaves the
            // same as an empty array.
            _engine.Indexes = new[] { -1, _engine.RequiredPropertyIndexes.Count };
            AssertSameValues(none, Snapshot(_engine, Detect()));
        }

        /// <summary>
        /// A cache set directly on an engine that filters graphs is refused,
        /// so a filtered result can never be stored and served to a caller
        /// that needs every property.
        /// </summary>
        [TestMethod]
        public void GraphFilter_SetCache_RefusedOnFilteringEngine()
        {
            var refusal = Assert.ThrowsExactly<InvalidOperationException>(
                () => _engine.SetCache(new DefaultFlowCache(
                    new CacheConfiguration() { Size = 10 })));
            Assert.AreEqual(
                Messages.ExceptionGraphFilterWithCache,
                refusal.Message);
        }

        /// <summary>
        /// A cache configured on the builder, which is how a deployment turns
        /// the cache on, fails the build, and the engine the build had
        /// created is disposed so its native data is released.
        /// </summary>
        [TestMethod]
        public void GraphFilter_BuilderCache_RefusedAndEngineDisposed()
        {
            var builder = NewBuilder(FilteredEngine);
            builder.SetCache(new CacheConfiguration() { Size = 10 });
            var refusal = Assert.ThrowsExactly<InvalidOperationException>(
                () => builder.Build(DataFile().FullName, false));
            Assert.AreEqual(
                Messages.ExceptionGraphFilterWithCache,
                refusal.Message);
            Assert.IsNotNull(builder.CreatedEngine);
            Assert.IsTrue(builder.CreatedEngine.IsDisposed,
                "The engine from the failed build must be disposed.");
        }

        /// <summary>
        /// The combination that used to poison the cache: a cache and lazy
        /// loading. With lazy loading the pipeline caches the result before
        /// ProcessEngine runs, so the engine must be refused at build time,
        /// before any request can reach the cache.
        /// </summary>
        [TestMethod]
        public void GraphFilter_BuilderCacheWithLazyLoading_Refused()
        {
            var builder = NewBuilder(FilteredEngine);
            builder
                .SetCache(new CacheConfiguration() { Size = 10 })
                .SetLazyLoading(new LazyLoadingConfiguration(60000));
            var refusal = Assert.ThrowsExactly<InvalidOperationException>(
                () => builder.Build(DataFile().FullName, false));
            Assert.AreEqual(
                Messages.ExceptionGraphFilterWithCache,
                refusal.Message);
            Assert.IsTrue(builder.CreatedEngine.IsDisposed);
        }

        /// <summary>
        /// A subclass that filters without declaring it could be given a
        /// cache, so its filtered requests are refused. Its unfiltered
        /// requests still work.
        /// </summary>
        [TestMethod]
        public void GraphFilter_UndeclaredFilter_Refused()
        {
            using (var engine = NewBuilder(UndeclaredEngine)
                .Build(DataFile().FullName, false))
            using (var pipeline = new PipelineBuilder(_logger)
                .AddFlowElement(engine)
                .Build())
            {
                engine.Indexes = new[] { engine.RequiredPropertyIndexes.Values.First() };
                var data = pipeline.CreateFlowData();
                data.AddEvidence("query.client-ip", IpAddress);
                // The pipeline collects element exceptions and rethrows them
                // together, so look inside the aggregate for the refusal.
                var aggregate = Assert.ThrowsExactly<AggregateException>(
                    () => data.Process());
                Assert.IsTrue(
                    IsRefusal(aggregate, Messages.ExceptionGraphFilterNotDeclared),
                    "Expected the refusal, got: " + aggregate);

                engine.Indexes = null;
                _engine.Indexes = null;
                AssertSameValues(
                    Snapshot(_engine, Detect()),
                    Snapshot(engine, Detect(pipeline)));
            }
        }

        /// <summary>
        /// An engine that does not filter keeps its results cache.
        /// </summary>
        [TestMethod]
        public void GraphFilter_StandardEngine_ServedFromCache()
        {
            _engine.Indexes = null;
            var expected = Snapshot(_engine, Detect());
            WithStandardEngineCache((engine, pipeline) =>
            {
                var first = Detect(pipeline);
                // The cache hands back the same instance and flags it, so
                // read the first answer before the second request.
                Assert.IsFalse(first.CacheHit);
                AssertSameValues(expected, Snapshot(engine, first));
                var second = Detect(pipeline);
                Assert.IsTrue(second.CacheHit,
                    "The second request should be served from the cache.");
                AssertSameValues(expected, Snapshot(engine, second));
            });
        }

        /// <summary>
        /// An engine that does not filter keeps its results cache with lazy
        /// loading, the combination the filtering engine now refuses.
        /// </summary>
        [TestMethod]
        public void GraphFilter_StandardEngine_LazyLoadingServedFromCache()
        {
            _engine.Indexes = null;
            var expected = Snapshot(_engine, Detect());
            WithStandardEngineCache((engine, pipeline) =>
            {
                var name = engine.RequiredPropertyIndexes.Keys.First();
                var first = Detect(pipeline);
                // Processing runs on a task. The indexer waits for it, which
                // GetValues does not, so read through it before comparing.
                Assert.IsNotNull(first[name]);
                Assert.IsFalse(first.CacheHit);
                AssertSameValues(expected, Snapshot(engine, first));
                var second = Detect(pipeline);
                Assert.IsNotNull(second[name]);
                Assert.IsTrue(second.CacheHit,
                    "The second request should be served from the cache.");
                AssertSameValues(expected, Snapshot(engine, second));
            },
            // A long wait so the task always finishes first. A wait that times
            // out without a cancellation token fails inside the pipeline with
            // "Nullable object must have a value" rather than a timeout, see
            // 51Degrees/pipeline-dotnet#437.
            new LazyLoadingConfiguration(60000));
        }
    }
}
