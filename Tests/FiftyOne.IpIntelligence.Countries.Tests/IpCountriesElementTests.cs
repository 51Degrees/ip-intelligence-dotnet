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

using FiftyOne.IpIntelligence.Countries.Data;
using FiftyOne.IpIntelligence.Countries.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.TypedMap;
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

        private IpCountriesElement CreateElement(IReadOnlyList<string> countryCodes)
        {
            return new IpCountriesElement(
                _logger.Object,
                countryCodes,
                (pipeline, element) =>
                    new IpCountriesData(
                        new Mock<ILogger<IpCountriesData>>().Object,
                        pipeline));
        }

        private IpCountriesData CreateCountriesData()
        {
            return new IpCountriesData(
                new Mock<ILogger<IpCountriesData>>().Object,
                new Mock<IPipeline>().Object);
        }

        private Mock<IFlowData> CreateMockFlowData(
            Dictionary<string, object> ipiStore)
        {
            var flowData = new Mock<IFlowData>();
            var ipCountriesDataStore = new IIpCountriesData[] { null };
            flowData.Setup(d => d.GetOrAdd(
                    It.IsAny<ITypedKey<IIpCountriesData>>(),
                    It.IsAny<Func<IPipeline, IIpCountriesData>>()))
                .Callback<ITypedKey<IIpCountriesData>, Func<IPipeline, IIpCountriesData>>((k, factory) => {
                    ipCountriesDataStore[0] ??= factory(null);
                })
                .Returns(() => ipCountriesDataStore[0]);
            flowData.Setup(d => d.Get<IIpCountriesData>())
                .Returns(() => ipCountriesDataStore[0]);
            if (ipiStore is null)
            {
                return flowData;
            }

            var ipDataMock = new Mock<IIpIntelligenceData>();
            ipDataMock.As<IElementData>()
                .Setup(d => d[It.IsAny<string>()])
                .Returns((string key) =>
                {
                    if (ipiStore.TryGetValue(key, out var val))
                        return val;
                    throw new KeyNotFoundException(key);
                });
            {
                AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> geoCountriesValue = new();
                if (ipiStore.TryGetValue(
                    nameof(IIpIntelligenceData.CountryCodesGeographical),
                    out var ccGeo)
                    && ccGeo is AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> countryCodesGeographical)
                {
                    geoCountriesValue = countryCodesGeographical;
                }
                ipDataMock.Setup(d => d.CountryCodesGeographical)
                    .Returns(() => geoCountriesValue);
            }
            {
                AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> popCountriesValue = new();
                if (ipiStore.TryGetValue(
                    nameof(IIpIntelligenceData.CountryCodesPopulation),
                    out var ccPop)
                    && ccPop is AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> countryCodesPopulation)
                {
                    popCountriesValue = countryCodesPopulation;
                }
                ipDataMock.Setup(d => d.CountryCodesPopulation)
                    .Returns(() => popCountriesValue);
            }

            flowData.Setup(d => d.Get<IIpIntelligenceData>())
                .Returns(ipDataMock.Object);
            return flowData;
        }

        private static IWeightedValue<string> Weighted(string code, ushort weight)
        {
            return new WeightedValue<string>(weight, code);
        }

        #region Happy path

        [TestMethod]
        public void Process_WithWeightedGeographical_ReturnsWeightedFirstThenRemaining()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> { Weighted("DE", 30000), Weighted("AT", 60000) }) }
            };
            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            var codes = countriesData.CountryCodesGeographicalAll.Value;
            Assert.AreEqual("AT", codes[0]);
            Assert.AreEqual("DE", codes[1]);
            CollectionAssert.AreEqual(
                TestCountryCodes.Where(c => c != "AT" && c != "DE").OrderBy(c => c).ToList(),
                codes.Skip(2).ToList());
            Assert.AreEqual(TestCountryCodes.Count, codes.Count);
        }

        [TestMethod]
        public void Process_WithWeightedPopulation_ReturnsWeightedFirstThenRemaining()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesPopulation", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> { Weighted("US", 50000), Weighted("GB", 20000) }) }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            var codes = countriesData.CountryCodesPopulationAll.Value;
            Assert.AreEqual("US", codes[0]);
            Assert.AreEqual("GB", codes[1]);
            Assert.AreEqual(TestCountryCodes.Count, codes.Count);
        }

        [TestMethod]
        public void Process_WithBothWeightedProperties_PopulatesBothOutputs()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> { Weighted("FR", 40000), Weighted("IT", 20000) }) },
                { "CountryCodesPopulation", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> { Weighted("US", 50000) }) }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            Assert.AreEqual("FR", countriesData.CountryCodesGeographicalAll.Value[0]);
            Assert.AreEqual("IT", countriesData.CountryCodesGeographicalAll.Value[1]);
            Assert.AreEqual("US", countriesData.CountryCodesPopulationAll.Value[0]);
        }

        [TestMethod]
        public void Process_WithSingleWeightedCode_PlacesItFirstThenRemainingAlpha()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> { Weighted("US", 65535) }) }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            var codes = countriesData.CountryCodesGeographicalAll.Value;
            Assert.AreEqual("US", codes[0]);
            CollectionAssert.AreEqual(
                TestCountryCodes.Where(c => c != "US").OrderBy(c => c).ToList(),
                codes.Skip(1).ToList());
        }

        #endregion

        #region No weighted data

        [TestMethod]
        public void Process_NoWeightedProperties_ReturnsAllCodesAlphabetically()
        {

            var flowData = CreateMockFlowData(new Dictionary<string, object>());

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            CollectionAssert.AreEqual(TestCountryCodes,
                countriesData.CountryCodesGeographicalAll.Value.ToList());
            CollectionAssert.AreEqual(TestCountryCodes,
                countriesData.CountryCodesPopulationAll.Value.ToList());
        }

        [TestMethod]
        public void Process_WeightedPropertyHasNoValue_ReturnsAllCodesAlphabetically()
        {
            var noValue = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>();
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", noValue },
                { "CountryCodesPopulation", noValue }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            CollectionAssert.AreEqual(TestCountryCodes,
                countriesData.CountryCodesGeographicalAll.Value.ToList());
        }

        [TestMethod]
        public void Process_WeightedPropertyEmptyList_ReturnsAllCodesAlphabetically()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>>()) }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            CollectionAssert.AreEqual(TestCountryCodes,
                countriesData.CountryCodesGeographicalAll.Value.ToList());
        }

        #endregion

        #region Edge cases

        [TestMethod]
        public void Process_NoIpiDataInPipeline_DoesNotThrow()
        {
            var flowData = CreateMockFlowData(null);
            flowData.Setup(d => d.Get<IIpIntelligenceData>())
                .Throws(new KeyNotFoundException("No IPI data"));

            CreateElement(TestCountryCodes).Process(flowData.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Process_NullFlowData_ThrowsArgumentNullException()
        {
            CreateElement(TestCountryCodes).Process(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Process_EmptyCountryCodesList_ThrowsArgumentNullException()
        {
            var flowData = CreateMockFlowData(new Dictionary<string, object>());

            CreateElement(Array.Empty<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Process_NullCountryCodesList_ThrowsArgumentException()
        {
            new IpCountriesElement(
                _logger.Object, null,
                (p, e) => new IpCountriesData(
                    new Mock<ILogger<IpCountriesData>>().Object, p));
        }

        #endregion

        #region Ordering

        [TestMethod]
        public void Process_MultipleWeightedCodes_OrderedByWeightDescending()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> {
                        Weighted("FR", 10000), Weighted("AT", 50000), Weighted("DE", 30000) }) }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            var codes = countriesData.CountryCodesGeographicalAll.Value;
            Assert.AreEqual("AT", codes[0]);
            Assert.AreEqual("DE", codes[1]);
            Assert.AreEqual("FR", codes[2]);
            CollectionAssert.AreEqual(
                TestCountryCodes.Where(c => c != "AT" && c != "DE" && c != "FR").OrderBy(c => c).ToList(),
                codes.Skip(3).ToList());
        }

        [TestMethod]
        public void Process_WeightedCodeNotInMasterList_StillIncludedFirst()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> { Weighted("ZZ", 50000) }) }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            var codes = countriesData.CountryCodesGeographicalAll.Value;
            Assert.AreEqual("ZZ", codes[0]);
            Assert.AreEqual(TestCountryCodes.Count + 1, codes.Count);
        }

        [TestMethod]
        public void Process_NoDuplicatesInOutput()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> { Weighted("DE", 50000), Weighted("FR", 30000) }) }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            var codes = countriesData.CountryCodesGeographicalAll.Value;
            Assert.AreEqual(codes.Count, codes.Distinct().Count());
        }

        #endregion

        #region Mixed

        [TestMethod]
        public void Process_OnePropertyWeightedOtherMissing_BothPopulated()
        {
            var store = new Dictionary<string, object>
            {
                { "CountryCodesGeographical", new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    new List<IWeightedValue<string>> { Weighted("HU", 40000), Weighted("SK", 25000) }) }
            };

            var flowData = CreateMockFlowData(store);

            CreateElement(TestCountryCodes).Process(flowData.Object);

            var countriesData = flowData.Object.Get<IIpCountriesData>();
            Assert.AreEqual("HU", countriesData.CountryCodesGeographicalAll.Value[0]);
            Assert.AreEqual("SK", countriesData.CountryCodesGeographicalAll.Value[1]);
            CollectionAssert.AreEqual(TestCountryCodes,
                countriesData.CountryCodesPopulationAll.Value.ToList());
        }

        #endregion

        #region Metadata

        [TestMethod]
        public void ElementDataKey_ReturnsExpectedValue()
        {
            Assert.AreEqual("ipcountries",
                CreateElement(TestCountryCodes).ElementDataKey);
        }

        [TestMethod]
        public void Properties_ContainsBothOutputProperties()
        {
            var props = CreateElement(TestCountryCodes).Properties;
            Assert.AreEqual(2, props.Count);
            Assert.IsTrue(props.Any(p => p.Name == "CountryCodesGeographicalAll"));
            Assert.IsTrue(props.Any(p => p.Name == "CountryCodesPopulationAll"));
        }

        [TestMethod]
        public void EvidenceKeyFilter_IsEmpty()
        {
            Assert.IsNotNull(
                CreateElement(TestCountryCodes).EvidenceKeyFilter);
        }

        #endregion
    }
}
