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

        private IpCountriesElement CreateElement(List<string> countryCodes)
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
            Dictionary<string, object> ipiStore,
            IpCountriesData countriesData)
        {
            var ipDataMock = new Mock<IIpIntelligenceData>();
            ipDataMock.As<IElementData>()
                .Setup(d => d[It.IsAny<string>()])
                .Returns((string key) =>
                {
                    if (ipiStore.TryGetValue(key, out var val))
                        return val;
                    throw new KeyNotFoundException(key);
                });

            var flowData = new Mock<IFlowData>();
            flowData.Setup(d => d.Get<IIpIntelligenceData>())
                .Returns(ipDataMock.Object);
            flowData.Setup(d => d.GetOrAdd(
                    It.IsAny<ITypedKey<IIpCountriesData>>(),
                    It.IsAny<Func<IPipeline, IIpCountriesData>>()))
                .Returns(countriesData);
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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(new Dictionary<string, object>(), countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

            CollectionAssert.AreEqual(TestCountryCodes,
                countriesData.CountryCodesGeographicalAll.Value.ToList());
        }

        #endregion

        #region Edge cases

        [TestMethod]
        public void Process_NoIpiDataInPipeline_DoesNotThrow()
        {
            var flowData = new Mock<IFlowData>();
            flowData.Setup(d => d.Get<IIpIntelligenceData>())
                .Throws(new KeyNotFoundException("No IPI data"));

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Process_NullFlowData_ThrowsArgumentNullException()
        {
            CreateElement(new List<string>(TestCountryCodes)).Process(null);
        }

        [TestMethod]
        public void Process_EmptyCountryCodesList_DoesNotPopulateOutputs()
        {
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(new Dictionary<string, object>(), countriesData);

            CreateElement(new List<string>()).Process(flowData.Object);

            flowData.Verify(d => d.GetOrAdd(
                It.IsAny<ITypedKey<IIpCountriesData>>(),
                It.IsAny<Func<IPipeline, IIpCountriesData>>()), Times.Never);
        }

        [TestMethod]
        public void Process_NullCountryCodesList_DoesNotPopulateOutputs()
        {
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(new Dictionary<string, object>(), countriesData);

            new IpCountriesElement(
                _logger.Object, null,
                (p, e) => new IpCountriesData(
                    new Mock<ILogger<IpCountriesData>>().Object, p))
                .Process(flowData.Object);

            flowData.Verify(d => d.GetOrAdd(
                It.IsAny<ITypedKey<IIpCountriesData>>(),
                It.IsAny<Func<IPipeline, IIpCountriesData>>()), Times.Never);
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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
            var countriesData = CreateCountriesData();
            var flowData = CreateMockFlowData(store, countriesData);

            CreateElement(new List<string>(TestCountryCodes)).Process(flowData.Object);

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
                CreateElement(new List<string>(TestCountryCodes)).ElementDataKey);
        }

        [TestMethod]
        public void Properties_ContainsBothOutputProperties()
        {
            var props = CreateElement(new List<string>(TestCountryCodes)).Properties;
            Assert.AreEqual(2, props.Count);
            Assert.IsTrue(props.Any(p => p.Name == "CountryCodesGeographicalAll"));
            Assert.IsTrue(props.Any(p => p.Name == "CountryCodesPopulationAll"));
        }

        [TestMethod]
        public void EvidenceKeyFilter_IsEmpty()
        {
            Assert.IsNotNull(
                CreateElement(new List<string>(TestCountryCodes)).EvidenceKeyFilter);
        }

        #endregion
    }
}
