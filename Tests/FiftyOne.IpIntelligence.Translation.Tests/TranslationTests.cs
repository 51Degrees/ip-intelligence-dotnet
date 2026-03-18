using FiftyOne.IpIntelligence.Translation.Data;
using FiftyOne.IpIntelligence.Translation.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.Extensions.Logging;
using Moq;

namespace FiftyOne.IpIntelligence.Translation.Tests
{
    [TestClass]
    public class TranslationTests
    {
        private ILoggerFactory _loggerFactory;

        [TestInitialize]
        public void TestInit()
        {
            _loggerFactory = LoggerFactory.Create(b => { });
        }

        /// <summary>
        /// Test that a list of countries can be translated from country codes
        /// to country names. This tests the CountryCodeTranslationEngine end
        /// to end.
        /// </summary>
        [TestMethod]
        public void CountryNamesFromCodes()
        {
            // Use a mock IP element to provide the country codes as evidence.
            // This allow the engine to be tested in isolation.
            var ipElement = MockIpElement(
                new[] { "GB", "FR" },
                new[] { "GB", "FR" });

            // Build the pipeline.
            var translationElement =
                new CountryCodeTranslationEngineBuilder(_loggerFactory)
                .Build();
            using var pipeline = new PipelineBuilder(_loggerFactory)
                .AddFlowElement(ipElement)
                .AddFlowElement(translationElement)
                .Build();

            // Process the data
            using var flowData = pipeline.CreateFlowData();
            flowData.Process();


            // Assert
            var result = flowData.Get<ICountryCodeTranslationData>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CountryNamesGeographical);
            Assert.IsNotNull(result.CountryNamesPopulation);
            Assert.IsTrue(result.CountryNamesGeographical.HasValue);
            Assert.IsTrue(result.CountryNamesPopulation.HasValue);
            Assert.AreEqual(2, result.CountryNamesGeographical.Value.Count);
            Assert.AreEqual(2, result.CountryNamesPopulation.Value.Count);
            Assert.AreEqual(
                "United Kingdom",
                result.CountryNamesGeographical.Value[0].Value);
            Assert.AreEqual(
                "France",
                result.CountryNamesGeographical.Value[1].Value);
            Assert.AreEqual(
                "United Kingdom",
                result.CountryNamesPopulation.Value[0].Value);
            Assert.AreEqual(
                "France",
                result.CountryNamesPopulation.Value[1].Value);
        }

        /// <summary>
        /// Test that a list of countries can be translated to a target
        /// language defined in the evidence. This tests the
        /// CountryCodeTranslationEngine and CountriesTranslationEngine end
        /// to end.
        /// </summary>
        [TestMethod]
        public void TranslatedCountry()
        {
            // Use a mock IP element to provide the country codes as evidence.
            // This allow the engine to be tested in isolation.
            var ipElement = MockIpElement(
                new[] { "GB", "FR" },
                new[] { "GB", "FR" });
            // Build the pipeline.
            var codeTranslationElement =
                new CountryCodeTranslationEngineBuilder(_loggerFactory)
                .Build();
            var nameTranslationElement =
                new CountriesTranslationEngineBuilder(_loggerFactory)
                .Build();
            using var pipeline = new PipelineBuilder(_loggerFactory)
                .AddFlowElement(ipElement)
                .AddFlowElement(codeTranslationElement)
                .AddFlowElement(nameTranslationElement)
                .Build();

            // Process the data, adding the language as evidence.
            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language", "fr_FR");
            flowData.Process();

            // Assert
            var result = flowData.Get<ICountriesTranslationData>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CountryNamesGeographicalTranslated);
            Assert.IsNotNull(result.CountryNamesPopulationTranslated);
            Assert.IsTrue(result.CountryNamesGeographicalTranslated.HasValue);
            Assert.IsTrue(result.CountryNamesPopulationTranslated.HasValue);
            Assert.AreEqual(2, result.CountryNamesGeographicalTranslated.Value.Count);
            Assert.AreEqual(2, result.CountryNamesPopulationTranslated.Value.Count);
            Assert.AreEqual(
                "Royaume-Uni",
                result.CountryNamesGeographicalTranslated.Value[0].Value);
            Assert.AreEqual(
                "France",
                result.CountryNamesGeographicalTranslated.Value[1].Value);
            Assert.AreEqual(
                "Royaume-Uni",
                result.CountryNamesPopulationTranslated.Value[0].Value);
            Assert.AreEqual(
                "France",
                result.CountryNamesPopulationTranslated.Value[1].Value);
        }


        /// <summary>
        /// Test that a list of countries can be translated from country codes
        /// to country names. This tests the CountryCodeAllTranslationEngine end
        /// to end.
        /// </summary>
        [TestMethod]
        public void CountryNamesAllFromCodes()
        {
            // Use a mock IP element to provide the country codes as evidence.
            // This allow the engine to be tested in isolation.
            var ipElement = MockIpCountriesElement(
                new[] { "GB", "FR" },
                new[] { "GB", "FR" });

            // Build the pipeline.
            var translationElement =
                new CountryCodeAllTranslationEngineBuilder(_loggerFactory)
                .Build();
            using var pipeline = new PipelineBuilder(_loggerFactory)
                .AddFlowElement(ipElement)
                .AddFlowElement(translationElement)
                .Build();

            // Process the data
            using var flowData = pipeline.CreateFlowData();
            flowData.Process();


            // Assert
            var result = flowData.Get<ICountryCodeTranslationData>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CountryNamesGeographicalAll);
            Assert.IsNotNull(result.CountryNamesPopulationAll);
            Assert.IsTrue(result.CountryNamesGeographicalAll.HasValue);
            Assert.IsTrue(result.CountryNamesPopulationAll.HasValue);
            Assert.AreEqual(2, result.CountryNamesGeographicalAll.Value.Count);
            Assert.AreEqual(2, result.CountryNamesPopulationAll.Value.Count);
            Assert.AreEqual(
                "United Kingdom",
                result.CountryNamesGeographicalAll.Value[0]);
            Assert.AreEqual(
                "France",
                result.CountryNamesGeographicalAll.Value[1]);
            Assert.AreEqual(
                "United Kingdom",
                result.CountryNamesPopulationAll.Value[0]);
            Assert.AreEqual(
                "France",
                result.CountryNamesPopulationAll.Value[1]);
        }

        /// <summary>
        /// Test that a list of countries can be translated to a target
        /// language defined in the evidence. This tests the
        /// CountryCodeAllTranslationEngine and CountriesTranslationEngine end
        /// to end.
        /// </summary>
        [TestMethod]
        public void Translate4dCountryAll()
        {
            // Use a mock IP element to provide the country codes as evidence.
            // This allow the engine to be tested in isolation.
            var ipElement = MockIpCountriesElement(
                new[] { "GB", "FR" },
                new[] { "GB", "FR" });
            // Build the pipeline.
            var codeTranslationElement =
                new CountryCodeAllTranslationEngineBuilder(_loggerFactory)
                .Build();
            var nameTranslationElement =
                new CountriesTranslationEngineBuilder(_loggerFactory)
                .Build();
            using var pipeline = new PipelineBuilder(_loggerFactory)
                .AddFlowElement(ipElement)
                .AddFlowElement(codeTranslationElement)
                .AddFlowElement(nameTranslationElement)
                .Build();

            // Process the data, adding the language as evidence.
            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language", "fr_FR");
            flowData.Process();

            // Assert
            var result = flowData.Get<ICountriesTranslationData>();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CountryNamesGeographicalAllTranslated);
            Assert.IsNotNull(result.CountryNamesPopulationAllTranslated);
            Assert.IsTrue(result.CountryNamesGeographicalAllTranslated.HasValue);
            Assert.IsTrue(result.CountryNamesPopulationAllTranslated.HasValue);
            Assert.AreEqual(2, result.CountryNamesGeographicalAllTranslated.Value.Count);
            Assert.AreEqual(2, result.CountryNamesPopulationAllTranslated.Value.Count);
            Assert.AreEqual(
                "Royaume-Uni",
                result.CountryNamesGeographicalAllTranslated.Value[0]);
            Assert.AreEqual(
                "France",
                result.CountryNamesGeographicalAllTranslated.Value[1]);
            Assert.AreEqual(
                "Royaume-Uni",
                result.CountryNamesPopulationAllTranslated.Value[0]);
            Assert.AreEqual(
                "France",
                result.CountryNamesPopulationAllTranslated.Value[1]);
        }


        /// <summary>
        /// Basic implementation of the abstract ElementData class to be used
        /// by the mock element.
        /// </summary>
        private class TestIpData : ElementDataBase
        {
            public TestIpData(
                ILogger<ElementDataBase> logger, 
                IPipeline pipeline)
                : base(logger, pipeline)
            {
            }
        }

        /// <summary>
        /// Create a mocked IP countries element that adds the country code
        /// propertiesprovided to a new ElementData. The Element key, and
        /// properties are initialized just to make the Pipeline work.
        /// </summary>
        /// <param name="countriesGeographicAll"></param>
        /// <param name="countriesPopulationAll"></param>
        /// <returns></returns>
        private IFlowElement MockIpCountriesElement(
            string[] countriesGeographicAll,
            string[] countriesPopulationAll)
        {
            return MockIpCountriesElement(
                new AspectPropertyValue<IReadOnlyList<string>>(
                    countriesGeographicAll.ToList()),
                new AspectPropertyValue<IReadOnlyList<string>>(
                    countriesPopulationAll.ToList()));
        }

        /// <summary>
        /// Create a mocked IP countries element that adds the country code
        /// propertiesprovided to a new ElementData. The Element key, and
        /// properties are initialized just to make the Pipeline work.
        /// </summary>
        /// <param name="countriesGeographicAll"></param>
        /// <param name="countriesPopulationAll"></param>
        /// <returns></returns>
        private IFlowElement MockIpCountriesElement(
            IAspectPropertyValue<IReadOnlyList<string>> countriesGeographicAll,
            IAspectPropertyValue<IReadOnlyList<string>> countriesPopulationAll)
        {
            var ipData = new TestIpData(null, null);

            ipData["CountryCodesGeographicalAll"] = countriesGeographicAll;
            ipData["CountryCodesPopulationAll"] = countriesPopulationAll;

            var element = new Mock<IFlowElement>();
            element.SetupGet(i => i.Properties)
                .Returns(new List<IElementPropertyMetaData>());
            element.SetupGet(i => i.ElementDataKey).Returns("ipcountries");
            element.Setup(i => i.Process(It.IsAny<IFlowData>()))
                .Callback<IFlowData>(data =>
                {
                    data.GetOrAdd(element.Object.ElementDataKey, p => ipData);
                });
            return element.Object;
        }

        /// <summary>
        /// Create a mocked IP element that adds the country code properties
        /// provided to a new ElementData. The Element key, and properties are
        /// initialized just to make the Pipeline work.
        /// </summary>
        /// <param name="countriesGeographic"></param>
        /// <param name="countriesPopulation"></param>
        /// <returns></returns>
        private IFlowElement MockIpElement(
            string[] countriesGeographic,
            string[] countriesPopulation)
        {
            return MockIpElement(
                new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    countriesGeographic.Select(i =>
                        new WeightedValue<string>(0, i))
                    .ToList()),
                new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    countriesPopulation.Select(i =>
                        new WeightedValue<string>(0, i))
                    .ToList()));
        }

        /// <summary>
        /// Create a mocked IP element that adds the country code properties
        /// provided to a new ElementData. The Element key, and properties are
        /// initialized just to make the Pipeline work.
        /// </summary>
        /// <param name="countriesGeographic"></param>
        /// <param name="countriesPopulation"></param>
        /// <returns></returns>
        private IFlowElement MockIpElement(
            IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> countriesGeographic,
            IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> countriesPopulation)
        {
            var ipData = new TestIpData(null, null);

            ipData["CountryCodesGeographical"] = countriesGeographic;
            ipData["CountryCodesPopulation"] = countriesPopulation;

            var element = new Mock<IFlowElement>();
            element.SetupGet(i => i.Properties)
                .Returns(new List<IElementPropertyMetaData>());
            element.SetupGet(i => i.ElementDataKey).Returns("ip");
            element.Setup(i => i.Process(It.IsAny<IFlowData>()))
                .Callback<IFlowData>(data =>
                {
                    data.GetOrAdd(element.Object.ElementDataKey, p => ipData);
                });
            return element.Object;
        }
    }
}
