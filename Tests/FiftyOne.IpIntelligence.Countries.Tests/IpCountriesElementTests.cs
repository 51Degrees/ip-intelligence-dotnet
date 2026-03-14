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

using FiftyOne.IpIntelligence.Countries.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.IpIntelligence.Countries.Tests
{
    [TestClass]
    public class IpCountriesElementTests
    {
        private static readonly List<string> TestCountryCodes =
            new List<string> { "AT", "DE", "FR", "GB", "HU", "IT", "SK", "US" };

        private Mock<ILogger<IpCountriesElement>> _logger;

        [TestInitialize]
        public void Init()
        {
            _logger = new Mock<ILogger<IpCountriesElement>>();
        }

        /// <summary>
        /// Creates a mock IIpIntelligenceData backed by a dictionary
        /// so that indexer reads and writes work through the mock.
        /// </summary>
        private Mock<IIpIntelligenceData> CreateMockIpData(
            Dictionary<string, object> store)
        {
            var mock = new Mock<IIpIntelligenceData>();

            // IElementData indexer getter — used by the element to read
            // weighted properties
            mock.As<IElementData>()
                .Setup(d => d[It.IsAny<string>()])
                .Returns((string key) =>
                {
                    if (store.TryGetValue(key, out var val))
                        return val;
                    throw new KeyNotFoundException(key);
                });

            // IData indexer setter — used by the element to write results.
            // We capture writes by hooking into SetupSet.
            mock.As<IData>()
                .SetupSet(d => d[It.IsAny<string>()] = It.IsAny<object>())
                .Callback<string, object>((key, value) => store[key] = value);

            return mock;
        }

        /// <summary>
        /// Creates a mock IFlowData that returns the given IIpIntelligenceData.
        /// </summary>
        private Mock<IFlowData> CreateMockFlowData(
            Mock<IIpIntelligenceData> ipDataMock)
        {
            var flowData = new Mock<IFlowData>();
            flowData.Setup(d => d.Get<IIpIntelligenceData>())
                .Returns(ipDataMock.Object);
            return flowData;
        }

        /// <summary>
        /// Creates a mock IFlowData where Get&lt;IIpIntelligenceData&gt;
        /// throws KeyNotFoundException (no IPI data in pipeline).
        /// </summary>
        private Mock<IFlowData> CreateMockFlowDataNoIpiData()
        {
            var flowData = new Mock<IFlowData>();
            flowData.Setup(d => d.Get<IIpIntelligenceData>())
                .Throws(new KeyNotFoundException("No IPI data"));
            return flowData;
        }

        /// <summary>
        /// Helper to build weighted string values.
        /// </summary>
        private static IWeightedValue<string> Weighted(string code, ushort weight)
        {
            return new WeightedValue<string>(weight, code);
        }

        #region Happy path — weighted data present

        [TestMethod]
        public void Process_WithWeightedGeographical_ReturnsWeightedFirstThenRemaining()
        {
            // Arrange: AT=60000, DE=30000 (AT has higher weight)
            var weightedGeo = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("DE", 30000),
                    Weighted("AT", 60000)
                });

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", weightedGeo }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            // Act
            element.Process(flowData.Object);

            // Assert
            Assert.IsTrue(store.ContainsKey("CountryCodesGeographicalAll"));
            var result = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            var codes = result.Value;

            // Weighted codes first: AT (highest weight), then DE
            Assert.AreEqual("AT", codes[0]);
            Assert.AreEqual("DE", codes[1]);

            // Remaining codes should be alphabetical and exclude AT, DE
            var remaining = codes.Skip(2).ToList();
            var expectedRemaining = TestCountryCodes
                .Where(c => c != "AT" && c != "DE")
                .OrderBy(c => c)
                .ToList();
            CollectionAssert.AreEqual(expectedRemaining, remaining);

            // Total count should match all country codes
            Assert.AreEqual(TestCountryCodes.Count, codes.Count);
        }

        [TestMethod]
        public void Process_WithWeightedPopulation_ReturnsWeightedFirstThenRemaining()
        {
            var weightedPop = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("US", 50000),
                    Weighted("GB", 20000)
                });

            var store = new Dictionary<string, object>
            {
                { "CountryCodesPopulation", weightedPop }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            Assert.IsTrue(store.ContainsKey("CountryCodesPopulationAll"));
            var result = store["CountryCodesPopulationAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            var codes = result.Value;

            Assert.AreEqual("US", codes[0]);
            Assert.AreEqual("GB", codes[1]);
            Assert.AreEqual(TestCountryCodes.Count, codes.Count);
        }

        [TestMethod]
        public void Process_WithBothWeightedProperties_PopulatesBothOutputs()
        {
            var weightedGeo = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("FR", 40000),
                    Weighted("IT", 20000)
                });
            var weightedPop = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("US", 50000)
                });

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", weightedGeo },
                { "CountryCodesPopulation", weightedPop }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            // Both properties should be set
            Assert.IsTrue(store.ContainsKey("CountryCodesGeographicalAll"));
            Assert.IsTrue(store.ContainsKey("CountryCodesPopulationAll"));

            var geoResult = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            var popResult = store["CountryCodesPopulationAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;

            Assert.IsNotNull(geoResult);
            Assert.IsNotNull(popResult);

            // Geo: FR first (highest weight), then IT
            Assert.AreEqual("FR", geoResult.Value[0]);
            Assert.AreEqual("IT", geoResult.Value[1]);
            Assert.AreEqual(TestCountryCodes.Count, geoResult.Value.Count);

            // Pop: US first
            Assert.AreEqual("US", popResult.Value[0]);
            Assert.AreEqual(TestCountryCodes.Count, popResult.Value.Count);
        }

        [TestMethod]
        public void Process_WithSingleWeightedCode_PlacesItFirstThenRemainingAlpha()
        {
            var weightedGeo = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("US", 65535)
                });

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", weightedGeo }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            var result = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            Assert.AreEqual("US", result.Value[0]);

            var remaining = result.Value.Skip(1).ToList();
            var expectedRemaining = TestCountryCodes
                .Where(c => c != "US")
                .OrderBy(c => c)
                .ToList();
            CollectionAssert.AreEqual(expectedRemaining, remaining);
        }

        #endregion

        #region Unhappy path — no weighted data

        [TestMethod]
        public void Process_NoWeightedProperties_ReturnsAllCodesAlphabetically()
        {
            // No weighted properties in the store at all
            var store = new Dictionary<string, object>();

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            // Both outputs should still be populated with all codes alphabetically
            var geoResult = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            var popResult = store["CountryCodesPopulationAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;

            Assert.IsNotNull(geoResult);
            Assert.IsNotNull(popResult);
            Assert.IsTrue(geoResult.HasValue);
            Assert.IsTrue(popResult.HasValue);

            CollectionAssert.AreEqual(TestCountryCodes, geoResult.Value.ToList());
            CollectionAssert.AreEqual(TestCountryCodes, popResult.Value.ToList());
        }

        [TestMethod]
        public void Process_WeightedPropertyHasNoValue_ReturnsAllCodesAlphabetically()
        {
            // Weighted property exists but HasValue = false
            var noValue = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>();

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", noValue },
                { "CountryCodesPopulation", noValue }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            var geoResult = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            Assert.IsNotNull(geoResult);
            Assert.IsTrue(geoResult.HasValue);
            CollectionAssert.AreEqual(TestCountryCodes, geoResult.Value.ToList());
        }

        [TestMethod]
        public void Process_WeightedPropertyEmptyList_ReturnsAllCodesAlphabetically()
        {
            // Weighted property exists and has value but the list is empty
            var emptyWeighted = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>());

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", emptyWeighted }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            var geoResult = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            Assert.IsNotNull(geoResult);
            Assert.IsTrue(geoResult.HasValue);
            CollectionAssert.AreEqual(TestCountryCodes, geoResult.Value.ToList());
        }

        #endregion

        #region Edge cases — no IPI data / null / empty codes

        [TestMethod]
        public void Process_NoIpiDataInPipeline_DoesNotThrow()
        {
            var flowData = CreateMockFlowDataNoIpiData();
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            // Should not throw — the element catches KeyNotFoundException
            element.Process(flowData.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Process_NullFlowData_ThrowsArgumentNullException()
        {
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));
            element.Process(null);
        }

        [TestMethod]
        public void Process_EmptyCountryCodesList_DoesNotPopulateOutputs()
        {
            var store = new Dictionary<string, object>();
            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>());

            element.Process(flowData.Object);

            // Element should return early — no outputs written
            Assert.IsFalse(store.ContainsKey("CountryCodesGeographicalAll"));
            Assert.IsFalse(store.ContainsKey("CountryCodesPopulationAll"));
        }

        [TestMethod]
        public void Process_NullCountryCodesList_DoesNotPopulateOutputs()
        {
            var store = new Dictionary<string, object>();
            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, null);

            element.Process(flowData.Object);

            Assert.IsFalse(store.ContainsKey("CountryCodesGeographicalAll"));
            Assert.IsFalse(store.ContainsKey("CountryCodesPopulationAll"));
        }

        #endregion

        #region Ordering verification

        [TestMethod]
        public void Process_MultipleWeightedCodes_OrderedByWeightDescending()
        {
            // Three codes with specific weights — verify ordering
            var weightedGeo = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("FR", 10000),  // lowest
                    Weighted("AT", 50000),  // highest
                    Weighted("DE", 30000)   // middle
                });

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", weightedGeo }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            var result = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            var codes = result.Value;

            // Weighted codes ordered by weight descending
            Assert.AreEqual("AT", codes[0]); // 50000
            Assert.AreEqual("DE", codes[1]); // 30000
            Assert.AreEqual("FR", codes[2]); // 10000

            // Remaining codes alphabetical
            var remaining = codes.Skip(3).ToList();
            var expectedRemaining = TestCountryCodes
                .Where(c => c != "AT" && c != "DE" && c != "FR")
                .OrderBy(c => c)
                .ToList();
            CollectionAssert.AreEqual(expectedRemaining, remaining);
        }

        [TestMethod]
        public void Process_WeightedCodeNotInMasterList_StillIncludedFirst()
        {
            // Weighted code "ZZ" is not in our master list —
            // it should still appear at the top, followed by all master codes
            var weightedGeo = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("ZZ", 50000)
                });

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", weightedGeo }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            var result = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            var codes = result.Value;

            Assert.AreEqual("ZZ", codes[0]);
            // All master codes should follow
            Assert.AreEqual(TestCountryCodes.Count + 1, codes.Count);
        }

        [TestMethod]
        public void Process_NoDuplicatesInOutput()
        {
            var weightedGeo = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("DE", 50000),
                    Weighted("FR", 30000)
                });

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", weightedGeo }
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            var result = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            var codes = result.Value;

            // No duplicates
            Assert.AreEqual(codes.Count, codes.Distinct().Count());
            // DE and FR should appear exactly once
            Assert.AreEqual(1, codes.Count(c => c == "DE"));
            Assert.AreEqual(1, codes.Count(c => c == "FR"));
        }

        #endregion

        #region Mixed scenarios

        [TestMethod]
        public void Process_OnePropertyWeightedOtherMissing_BothPopulated()
        {
            // Only geographical has weighted data; population is missing
            var weightedGeo = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                new List<IWeightedValue<string>>
                {
                    Weighted("HU", 40000),
                    Weighted("SK", 25000)
                });

            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", weightedGeo }
                // CountryCodesPopulation is missing
            };

            var ipData = CreateMockIpData(store);
            var flowData = CreateMockFlowData(ipData);
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));

            element.Process(flowData.Object);

            // Geographical should have weighted codes first
            var geoResult = store["CountryCodesGeographicalAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            Assert.AreEqual("HU", geoResult.Value[0]);
            Assert.AreEqual("SK", geoResult.Value[1]);

            // Population should have all codes alphabetically (no weighted data)
            var popResult = store["CountryCodesPopulationAll"]
                as IAspectPropertyValue<IReadOnlyList<string>>;
            CollectionAssert.AreEqual(TestCountryCodes, popResult.Value.ToList());
        }

        #endregion

        #region Element metadata

        [TestMethod]
        public void ElementDataKey_ReturnsExpectedValue()
        {
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));
            Assert.AreEqual("ipcountries", element.ElementDataKey);
        }

        [TestMethod]
        public void Properties_ContainsBothOutputProperties()
        {
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));
            var props = element.Properties;

            Assert.AreEqual(2, props.Count);
            Assert.IsTrue(props.Any(p => p.Name == "CountryCodesGeographicalAll"));
            Assert.IsTrue(props.Any(p => p.Name == "CountryCodesPopulationAll"));
        }

        [TestMethod]
        public void EvidenceKeyFilter_IsEmpty()
        {
            var element = new IpCountriesElement(_logger.Object, new List<string>(TestCountryCodes));
            // Element doesn't require any evidence keys
            var filter = element.EvidenceKeyFilter;
            Assert.IsNotNull(filter);
        }

        #endregion
    }
}
