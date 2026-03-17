using FiftyOne.IpIntelligence.Translation.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;

namespace FiftyOne.IpIntelligence.Translation.FlowElements
{
    public class CountryCodeTranslationEngineBuilder
    {
        private readonly ILoggerFactory _loggerFactory;

        public CountryCodeTranslationEngineBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public CountryCodeTranslationEngine Build()
        {

            return new CountryCodeTranslationEngine(
                _loggerFactory.CreateLogger<CountryCodeTranslationEngine>(),
                CreateData);
        }

        /// <summary>
        /// Creates an instance of ElementData
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="flowElement"></param>
        /// <returns></returns>
        private ICountryCodeTranslationData CreateData(
            IPipeline pipeline,
            FlowElementBase<
                ICountryCodeTranslationData,
                IElementPropertyMetaData> flowElement)
        {
            return new CountryCodeTranslationData(
                _loggerFactory.CreateLogger<CountryCodeTranslationData>(),
                pipeline);
        }
    }
}
