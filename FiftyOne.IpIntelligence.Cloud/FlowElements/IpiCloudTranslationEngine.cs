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

using FiftyOne.IpIntelligence.Cloud.Data;
using FiftyOne.IpIntelligence.Shared.Services;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.IpIntelligence.Cloud.FlowElements
{
    /// <summary>
    /// Cloud engine that reads translation data from the JSON response
    /// and populates <see cref="ICountryCodeTranslationData"/>.
    /// This engine extracts country name translations from the cloud service
    /// response and makes them available for further translation to other languages.
    /// </summary>
    public class IpiCloudTranslationEngine : CloudAspectEngineBase<CloudCountryCodeTranslationData>
    {
        private static JsonConverter[] JSON_CONVERTERS = new JsonConverter[]
        {
            new CloudJsonConverter()
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// The logger for this instance to use
        /// </param>
        /// <param name="translationDataFactory">
        /// Factory function to use when creating aspect data instances.
        /// </param>
        public IpiCloudTranslationEngine(
            ILogger<IpiCloudTranslationEngine> logger,
            Func<IPipeline,
                FlowElementBase<CloudCountryCodeTranslationData,
                    IAspectPropertyMetaData>,
                CloudCountryCodeTranslationData> translationDataFactory)
            : base(logger, translationDataFactory)
        {
        }

        /// <summary>
        /// The key to use for storing this engine's data in a 
        /// <see cref="IFlowData"/> instance.
        /// </summary>
        public override string ElementDataKey => Constants.CountryNamesKey;

        /// <summary>
        /// The filter that defines the evidence that is used by 
        /// this engine.
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter =>
            new EvidenceKeyFilterWhitelist(new List<string>());

        /// <summary>
        /// Static list of properties that this engine always supports.
        /// These are defined locally rather than fetched from cloud metadata
        /// since the translation properties are always the same.
        /// </summary>
        private static readonly IList<IAspectPropertyMetaData> _properties =
            new List<IAspectPropertyMetaData>
            {
                new AspectPropertyMetaData(
                    null,
                    "CountryNamesGeographical",
                    typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>),
                    "Country Names",
                    new List<string>(),
                    true,
                    "",
                    null,
                    false,
                    null),
                new AspectPropertyMetaData(
                    null,
                    "CountryNamesPopulation",
                    typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>),
                    "Country Names",
                    new List<string>(),
                    true,
                    "",
                    null,
                    false,
                    null),
                new AspectPropertyMetaData(
                    null,
                    "CountryNamesGeographicalAll",
                    typeof(IAspectPropertyValue<IReadOnlyList<string>>),
                    "Country Names",
                    new List<string>(),
                    true,
                    "",
                    null,
                    false,
                    null),
                new AspectPropertyMetaData(
                    null,
                    "CountryNamesPopulationAll",
                    typeof(IAspectPropertyValue<IReadOnlyList<string>>),
                    "Country Names",
                    new List<string>(),
                    true,
                    "",
                    null,
                    false,
                    null),
                new AspectPropertyMetaData(
                    null,
                    "CountryNamesGeographicalTranslated",
                    typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>),
                    "Country Names",
                    new List<string>(),
                    true,
                    "",
                    null,
                    false,
                    null),
                new AspectPropertyMetaData(
                    null,
                    "CountryNamesPopulationTranslated",
                    typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>),
                    "Country Names",
                    new List<string>(),
                    true,
                    "",
                    null,
                    false,
                    null),
                new AspectPropertyMetaData(
                    null,
                    "CountryNamesGeographicalAllTranslated",
                    typeof(IAspectPropertyValue<IReadOnlyList<string>>),
                    "Country Names",
                    new List<string>(),
                    true,
                    "",
                    null,
                    false,
                    null),
                new AspectPropertyMetaData(
                    null,
                    "CountryNamesPopulationAllTranslated",
                    typeof(IAspectPropertyValue<IReadOnlyList<string>>),
                    "Country Names",
                    new List<string>(),
                    true,
                    "",
                    null,
                    false,
                    null)
            };

        /// <summary>
        /// Get the property meta-data for properties populated by this engine.
        /// Returns the static list since these properties are always locally defined.
        /// </summary>
        public override IList<IAspectPropertyMetaData> Properties => _properties;

        /// <summary>
        /// Indicates that properties have been loaded.
        /// Always returns true since this engine uses locally defined properties
        /// rather than cloud metadata.
        /// </summary>
        public override bool HasLoadedProperties => true;

        /// <summary>
        /// Perform the processing for this engine:
        /// 1. Get the JSON data from the <see cref="CloudRequestEngine"/> response.
        /// 2. Extract properties relevant to this engine.
        /// 3. Deserialize JSON data to populate a 
        /// <see cref="CloudCountryCodeTranslationData"/> instance.
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> instance containing data for the 
        /// current request.
        /// </param>
        /// <param name="aspectData">
        /// The <see cref="CloudCountryCodeTranslationData"/> instance to populate.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if a required parameter is null
        /// </exception>
        protected override void ProcessEngine(IFlowData data, CloudCountryCodeTranslationData aspectData)
        {
            if (data == null) { throw new ArgumentNullException(nameof(data)); }
            if (aspectData == null) { throw new ArgumentNullException(nameof(aspectData)); }

            var requestData = data.GetFromElement(RequestEngine.GetInstance());
            var json = requestData?.JsonResponse;

            if (string.IsNullOrEmpty(json))
            {
                throw new PipelineConfigurationException(
                    $"Json response from cloud request engine is null. " +
                    $"This is probably because there is not a " +
                    $"'CloudRequestEngine' before the '{GetType().Name}' " +
                    $"in the Pipeline. This engine will be unable " +
                    $"to produce results until this is corrected.");
            }

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var propertyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                dictionary["countrynames"].ToString(),
                new JsonSerializerSettings()
                {
                    Converters = JSON_CONVERTERS,
                });

            var translated = CloudDataHelper.CreateAPVDictionary(propertyValues, Properties.ToList(), null, Logger, GetType().Name);
            aspectData.PopulateFrom(translated);
        }

        /// <summary>
        /// Get the type of a property from the meta-data.
        /// </summary>
        protected override Type GetPropertyType(
            PropertyMetaData propertyMetaData,
            Type parentObjectType)
        {
            return typeof(AspectPropertyValue<>).MakeGenericType(
                typeof(IReadOnlyList<>).MakeGenericType(
                    typeof(IWeightedValue<>).MakeGenericType(
                        base.GetPropertyType(propertyMetaData, parentObjectType))));
        }
    }
}
