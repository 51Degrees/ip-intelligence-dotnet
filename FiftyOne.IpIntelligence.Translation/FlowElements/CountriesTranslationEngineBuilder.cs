using FiftyOne.IpIntelligence.Translation.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;

namespace FiftyOne.IpIntelligence.Translation.FlowElements
{
    public class CountriesTranslationEngineBuilder
    {
        private readonly ILoggerFactory _loggerFactory;

        public CountriesTranslationEngineBuilder(ILoggerFactory loggerFactory) 
        {
            _loggerFactory = loggerFactory;
        }

        public CountriesTranslationEngine Build()
        {

            return new CountriesTranslationEngine(
                _loggerFactory.CreateLogger<CountriesTranslationEngine>(),
                CreateData);
        }

        /// <summary>
        /// Creates an instance of ElementData
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="flowElement"></param>
        /// <returns></returns>
        private ICountriesTranslationData CreateData(
            IPipeline pipeline,
            FlowElementBase<
                ICountriesTranslationData,
                IElementPropertyMetaData> flowElement)
        {
            return new CountriesTranslationData(
                _loggerFactory.CreateLogger<CountriesTranslationData>(),
                pipeline);
        }
    }
}
