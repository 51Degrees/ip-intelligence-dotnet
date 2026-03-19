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
            Assert.IsTrue(result.CountryNamesGeographicalTranslated.HasValue);
            Assert.IsTrue(result.CountryNamesPopulationTranslated.HasValue);
            Assert.AreEqual(2,
                result.CountryNamesGeographicalTranslated.Value.Count);
            Assert.AreEqual(2,
                result.CountryNamesPopulationTranslated.Value.Count);
            Assert.AreEqual(
                "Royaume-Uni",
                result.CountryNamesGeographicalTranslated.Value[0].Value);
            Assert.AreEqual(
                "France",
                result.CountryNamesGeographicalTranslated.Value[1].Value);
        }


        /// <summary>
        /// Test that the "All" lists are produced correctly, with weighted
        /// countries first and remaining countries sorted alphabetically
        /// by translated name.
        /// </summary>
        [TestMethod]
        public void AllListsProducedCorrectly()
        {
            var ipElement = MockIpElement(
                new[] { "GB", "FR" },
                new[] { "GB", "FR" });
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

            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language", "fr_FR");
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            // Check "All" lists exist and have values.
            Assert.IsTrue(
                result.CountryNamesGeographicalAllTranslated.HasValue);
            Assert.IsTrue(
                result.CountryNamesPopulationAllTranslated.HasValue);
            Assert.IsTrue(result.CountryCodesGeographicalAll.HasValue);
            Assert.IsTrue(result.CountryCodesPopulationAll.HasValue);

            var namesAll =
                result.CountryNamesGeographicalAllTranslated.Value;
            var codesAll = result.CountryCodesGeographicalAll.Value;

            // Same number of names and codes.
            Assert.AreEqual(namesAll.Count, codesAll.Count);

            // The weighted countries should be first (GB and FR).
            Assert.AreEqual("Royaume-Uni", namesAll[0]);
            Assert.AreEqual("GB", codesAll[0]);
            Assert.AreEqual("France", namesAll[1]);
            Assert.AreEqual("FR", codesAll[1]);

            // All known countries should be present.
            Assert.IsTrue(namesAll.Count > 200);

            // GB and FR should not appear again after the first 2 positions.
            Assert.IsFalse(codesAll.Skip(2).Contains("GB"));
            Assert.IsFalse(codesAll.Skip(2).Contains("FR"));

            // Remaining countries should be sorted alphabetically by
            // translated name.
            var remaining = namesAll.Skip(2).ToList();
            for (int i = 1; i < remaining.Count; i++)
            {
                Assert.IsTrue(
                    string.Compare(remaining[i - 1], remaining[i],
                        StringComparison.CurrentCulture) <= 0,
                    $"Expected '{remaining[i - 1]}' <= '{remaining[i]}'");
            }
        }

        /// <summary>
        /// Test that "All" lists work correctly without a translation
        /// language specified (falls back to English names).
        /// </summary>
        [TestMethod]
        public void AllListsWithoutLanguage()
        {
            var ipElement = MockIpElement(
                new[] { "GB" },
                new[] { "GB" });
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

            using var flowData = pipeline.CreateFlowData();
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            Assert.IsTrue(
                result.CountryNamesGeographicalAllTranslated.HasValue);
            Assert.IsTrue(result.CountryCodesGeographicalAll.HasValue);

            var namesAll =
                result.CountryNamesGeographicalAllTranslated.Value;
            var codesAll = result.CountryCodesGeographicalAll.Value;

            // GB (United Kingdom) should be first.
            Assert.AreEqual("United Kingdom", namesAll[0]);
            Assert.AreEqual("GB", codesAll[0]);

            // All known countries should be present.
            Assert.IsTrue(namesAll.Count > 200);

            // Remaining should be sorted alphabetically by English name.
            var remaining = namesAll.Skip(1).ToList();
            for (int i = 1; i < remaining.Count; i++)
            {
                Assert.IsTrue(
                    string.Compare(remaining[i - 1], remaining[i],
                        StringComparison.CurrentCulture) <= 0,
                    $"Expected '{remaining[i - 1]}' <= '{remaining[i]}'");
            }
        }

        /// <summary>
        /// Test that "All" lists contain all countries when no IP data
        /// is present (no weighted codes), and are sorted alphabetically.
        /// </summary>
        [TestMethod]
        public void AllListsWithNoIpData()
        {
            // Create a pipeline with translation engines but no IP element.
            var emptyIpElement = MockIpElement(
                Array.Empty<string>(),
                Array.Empty<string>());
            var codeTranslationElement =
                new CountryCodeTranslationEngineBuilder(_loggerFactory)
                .Build();
            var nameTranslationElement =
                new CountriesTranslationEngineBuilder(_loggerFactory)
                .Build();
            using var pipeline = new PipelineBuilder(_loggerFactory)
                .AddFlowElement(emptyIpElement)
                .AddFlowElement(codeTranslationElement)
                .AddFlowElement(nameTranslationElement)
                .Build();

            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language", "de_DE");
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            Assert.IsTrue(
                result.CountryNamesGeographicalAllTranslated.HasValue);
            Assert.IsTrue(result.CountryCodesGeographicalAll.HasValue);

            var namesAll =
                result.CountryNamesGeographicalAllTranslated.Value;
            var codesAll = result.CountryCodesGeographicalAll.Value;

            // All known countries should be present, all sorted
            // alphabetically.
            Assert.IsTrue(namesAll.Count > 200);
            for (int i = 1; i < namesAll.Count; i++)
            {
                Assert.IsTrue(
                    string.Compare(namesAll[i - 1], namesAll[i],
                        StringComparison.CurrentCulture) <= 0,
                    $"Expected '{namesAll[i - 1]}' <= '{namesAll[i]}'");
            }
        }

        /// <summary>
        /// Test that the population "All" lists work independently from
        /// geographical "All" lists.
        /// </summary>
        [TestMethod]
        public void PopulationAndGeographicalAreIndependent()
        {
            var ipElement = MockIpElement(
                geographicCodes: new[] { "DE" },
                populationCodes: new[] { "US", "CN" });
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

            using var flowData = pipeline.CreateFlowData();
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            var geoCodes = result.CountryCodesGeographicalAll.Value;
            var popCodes = result.CountryCodesPopulationAll.Value;

            // Geographical should start with DE.
            Assert.AreEqual("DE", geoCodes[0]);

            // Population should start with US, CN.
            Assert.AreEqual("US", popCodes[0]);
            Assert.AreEqual("CN", popCodes[1]);

            // DE should not be at position 0 in the population list
            // (unless it happens to be sorted first, which it won't).
            Assert.AreNotEqual("DE", popCodes[0]);
        }

        /// <summary>
        /// Test that German translation produces correct translated names
        /// in the "All" lists.
        /// </summary>
        [TestMethod]
        public void GermanTranslation()
        {
            var ipElement = MockIpElement(
                new[] { "DE" },
                new[] { "DE" });
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

            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language", "de_DE");
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            // Weighted translation check.
            Assert.AreEqual("Deutschland",
                result.CountryNamesGeographicalTranslated.Value[0].Value);

            // "All" list should have Deutschland first.
            Assert.AreEqual("Deutschland",
                result.CountryNamesGeographicalAllTranslated.Value[0]);
            Assert.AreEqual("DE",
                result.CountryCodesGeographicalAll.Value[0]);
        }

        /// <summary>
        /// Test that Accept-Language header with dash format (e.g. fr-FR)
        /// is correctly parsed.
        /// </summary>
        [TestMethod]
        public void AcceptLanguageWithDash()
        {
            var ipElement = MockIpElement(
                new[] { "GB" },
                new[] { "GB" });
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

            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language", "fr-FR");
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            Assert.AreEqual("Royaume-Uni",
                result.CountryNamesGeographicalTranslated.Value[0].Value);
        }

        /// <summary>
        /// Test that English Accept-Language produces no translation
        /// (English names are returned as-is).
        /// </summary>
        [TestMethod]
        public void EnglishLanguageNoTranslation()
        {
            var ipElement = MockIpElement(
                new[] { "FR", "DE" },
                new[] { "FR", "DE" });
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

            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language", "en-US,en;q=0.9");
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            // Weighted names should remain in English.
            Assert.AreEqual("France",
                result.CountryNamesGeographicalTranslated.Value[0].Value);
            Assert.AreEqual("Germany",
                result.CountryNamesGeographicalTranslated.Value[1].Value);

            // "All" list names should be English.
            Assert.AreEqual("France",
                result.CountryNamesGeographicalAllTranslated.Value[0]);
            Assert.AreEqual("Germany",
                result.CountryNamesGeographicalAllTranslated.Value[1]);
        }

        /// <summary>
        /// Test that English is picked even when followed by other languages
        /// that have translation files (e.g. en-US,de-DE;q=0.5).
        /// English should win and no German translation should occur.
        /// </summary>
        [TestMethod]
        public void EnglishPreferredOverOtherLanguages()
        {
            var ipElement = MockIpElement(
                new[] { "DE" },
                new[] { "DE" });
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

            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language",
                "en-US,en;q=0.9,de-DE;q=0.5,fr;q=0.3");
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            // Should be English "Germany", NOT German "Deutschland".
            Assert.AreEqual("Germany",
                result.CountryNamesGeographicalTranslated.Value[0].Value);
        }

        /// <summary>
        /// Test that when the preferred language (e.g. "es") has no exact
        /// locale match but a translation file exists (es_ES), it is found
        /// before falling through to a lower-priority language.
        /// </summary>
        [TestMethod]
        public void PreferredLanguageMatchedBeforeLowerPriority()
        {
            var ipElement = MockIpElement(
                new[] { "DE" },
                new[] { "DE" });
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

            using var flowData = pipeline.CreateFlowData();
            flowData.AddEvidence("header.Accept-Language",
                "es,de-DE;q=0.8,fr;q=0.5");
            flowData.Process();

            var result = flowData.Get<ICountriesTranslationData>();

            // Should be Spanish "Alemania", NOT German "Deutschland".
            Assert.AreEqual("Alemania",
                result.CountryNamesGeographicalTranslated.Value[0].Value);
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
        /// Create a mocked IP element that adds the country code properties
        /// provided to a new ElementData. The Element key, and properties are
        /// initialized just to make the Pipeline work.
        /// </summary>
        private IFlowElement MockIpElement(
            string[] geographicCodes,
            string[] populationCodes)
        {
            return MockIpElement(
                new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    geographicCodes.Select(i =>
                        new WeightedValue<string>(0, i))
                    .ToList()),
                new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>(
                    populationCodes.Select(i =>
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
