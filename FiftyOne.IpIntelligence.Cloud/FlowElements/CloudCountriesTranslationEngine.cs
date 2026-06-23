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
    /// Cloud counterpart of the on-premise
    /// <see cref="FiftyOne.IpIntelligence.Translation.FlowElements.CountriesTranslationEngine"/>.
    /// Reads the already-computed country name translations from the cloud
    /// service response (the cloud runs the translation pipeline server-side)
    /// and exposes them through <see cref="ICountriesTranslationData"/>.
    /// Like <see cref="IpiCloudEngine"/>, its property list is driven by the
    /// cloud metadata, so a resource key that does not grant the translation
    /// properties fails fast when the pipeline is built.
    /// </summary>
    public class CloudCountriesTranslationEngine
        : CloudAspectEngineBase<CloudCountriesTranslationData>
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
        public CloudCountriesTranslationEngine(
            ILogger<CloudCountriesTranslationEngine> logger,
            Func<IPipeline,
                FlowElementBase<CloudCountriesTranslationData,
                    IAspectPropertyMetaData>,
                CloudCountriesTranslationData> translationDataFactory)
            : base(logger, translationDataFactory)
        {
        }

        /// <summary>
        /// The key to use for storing this engine's data in a
        /// <see cref="IFlowData"/> instance. Matches the on-premise
        /// CountriesTranslationEngine so the two are interchangeable.
        /// </summary>
        public override string ElementDataKey =>
            Constants.CountryNamesTranslatedKey;

        /// <summary>
        /// The filter that defines the evidence that is used by
        /// this engine.
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter =>
            new EvidenceKeyFilterWhitelist(new List<string>());

        /// <summary>
        /// Perform the processing for this engine:
        /// 1. Extract the "countrynamestranslated" section from the cloud JSON.
        /// 2. Deserialize it to populate a
        /// <see cref="CloudCountriesTranslationData"/> instance.
        ///
        /// The base <see cref="CloudAspectEngineBase{T}"/> handles fetching the
        /// JSON from the <see cref="CloudRequestEngine"/>, the missing-engine
        /// error and the empty-response (cloud request failed) case, then calls
        /// this method with the JSON.
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> instance containing data for the
        /// current request.
        /// </param>
        /// <param name="aspectData">
        /// The <see cref="CloudCountriesTranslationData"/> instance to populate.
        /// </param>
        /// <param name="json">
        /// The JSON response from the <see cref="CloudRequestEngine"/>.
        /// </param>
        protected override void ProcessCloudEngine(
            IFlowData data, CloudCountriesTranslationData aspectData, string json)
        {
            if (aspectData == null) { throw new ArgumentNullException(nameof(aspectData)); }

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var propertyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                dictionary[Constants.CountryNamesTranslatedKey].ToString(),
                new JsonSerializerSettings()
                {
                    Converters = JSON_CONVERTERS,
                });

            var translated = CloudDataHelper.CreateAPVDictionary(
                propertyValues, Properties.ToList(), null, Logger, GetType().Name);
            aspectData.PopulateFrom(translated);
        }

        /// <summary>
        /// Map a property's cloud meta-data type to the strongly-typed
        /// <see cref="AspectPropertyValue{T}"/> the data class exposes.
        /// The weighted translated lists arrive as "WeightedString" and the
        /// "All" lists arrive as "Array" (a plain list of strings).
        /// </summary>
        /// <param name="propertyMetaData">
        /// The <see cref="PropertyMetaData"/> instance to translate.
        /// </param>
        /// <param name="parentObjectType">
        /// The type of the object on which this property exists.
        /// </param>
        protected override Type GetPropertyType(
            PropertyMetaData propertyMetaData,
            Type parentObjectType)
        {
            if (propertyMetaData == null)
            {
                throw new ArgumentNullException(nameof(propertyMetaData));
            }

            const string weightedPrefix = "Weighted";
            if (propertyMetaData.Type?.StartsWith(
                    weightedPrefix, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                var elementType = propertyMetaData.Type.Substring(weightedPrefix.Length);
                var propClone = new PropertyMetaData
                {
                    Name = propertyMetaData.Name,
                    Type = elementType,
                    Category = propertyMetaData.Category,
                    ItemProperties = propertyMetaData.ItemProperties,
                    DelayExecution = propertyMetaData.DelayExecution,
                    EvidenceProperties = propertyMetaData.EvidenceProperties,
                };
                return typeof(AspectPropertyValue<>).MakeGenericType(
                    typeof(IReadOnlyList<>).MakeGenericType(
                        typeof(IWeightedValue<>).MakeGenericType(
                            base.GetPropertyType(propClone, parentObjectType))));
            }

            // The base class does not handle "Array"; the translation "All"
            // lists are always lists of strings.
            if (string.Equals(propertyMetaData.Type, "Array",
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(AspectPropertyValue<IReadOnlyList<string>>);
            }

            return typeof(AspectPropertyValue<>).MakeGenericType(
                base.GetPropertyType(propertyMetaData, parentObjectType));
        }
    }
}
