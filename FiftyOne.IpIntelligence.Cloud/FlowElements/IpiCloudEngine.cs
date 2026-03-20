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
using FiftyOne.Pipeline.CloudRequestEngine.Data;
using FiftyOne.Pipeline.CloudRequestEngine.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.IpIntelligence.Cloud.FlowElements
{
    /// <summary>
    /// Engine that takes the JSON response from the 
    /// <see cref="CloudRequestEngine"/> and uses it populate a 
    /// IpDataCloud instance for easier consumption.
    /// </summary>
    public class IpiCloudEngine : CloudAspectEngineBase<IpDataCloud>
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
        /// <param name="ipDataFactory">
        /// Factory function to use when creating aspect data instances.
        /// </param>
        public IpiCloudEngine(
            ILogger<IpiCloudEngine> logger,
            Func<IPipeline, FlowElementBase<IpDataCloud, IAspectPropertyMetaData>, IpDataCloud> ipDataFactory)
            : base(logger,
                  ipDataFactory)
        {
        }

        /// <summary>
        /// The key to use for storing this engine's data in a 
        /// <see cref="IFlowData"/> instance.
        /// </summary>
        public override string ElementDataKey => "ip";

        /// <summary>
        /// The filter that defines the evidence that is used by 
        /// this engine.
        /// This engine needs no evidence as it works from the response
        /// from the <see cref="ICloudRequestEngine"/>.
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter =>
            new EvidenceKeyFilterWhitelist(new List<string>());

        /// <summary>
        /// Perform the processing for this engine:
        /// 1. Get the JSON data from the <see cref="CloudRequestEngine"/> 
        /// response.
        /// 2. Extract properties relevant to this engine.
        /// 3. Deserialize JSON data to populate a 
        /// <see cref="IpDataCloud"/> instance.
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> instance containing data for the 
        /// current request.
        /// </param>
        /// <param name="aspectData">
        /// The <see cref="IpDataCloud"/> instance to populate with
        /// values.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if a required parameter is null
        /// </exception>
        protected override void ProcessEngine(IFlowData data, IpDataCloud aspectData)
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
            else
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                var propertyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictionary["ip"].ToString(),
                    new JsonSerializerSettings()
                    {
                        Converters = JSON_CONVERTERS,
                    });

                var ip = CloudDataHelper.CreateAPVDictionary(
                    propertyValues,
                    Properties.ToList(),
                    CloudDataHelper.IpTypeConverter,
                    Logger,
                    GetType().Name);
                aspectData.PopulateFrom(ip);
            }
        }

        /// <summary>
        /// Convert value returned by cloud in json (weakly typed)
        /// to a value expected by AspectPropertyValue (strongly typed).
        /// Uses the IP-specific type converter for IPAddress and WktString.
        /// </summary>
        /// <param name="rawValue">Original value.</param>
        /// <param name="valueType">Target type.</param>
        /// <returns>Converted value.</returns>
        public static object ToValueForAPV(object rawValue, Type valueType)
        {
            return CloudDataHelper.ToValueForAPV(rawValue, valueType, CloudDataHelper.IpTypeConverter);
        }

        /// <summary>
        /// Try to get the type of a property from the information
        /// returned by the cloud service. This should be overridden
        /// if anything other than simple types are required.
        /// </summary>
        /// <param name="propertyMetaData">
        /// The <see cref="PropertyMetaData"/> instance to translate.
        /// </param>
        /// <param name="parentObjectType">
        /// The type of the object on which this property exists.
        /// </param>
        /// <returns>
        /// The type of the property determined from the Type field
        /// of propertyMetaData.
        /// </returns>
        protected override Type GetPropertyType(
            PropertyMetaData propertyMetaData,
            Type parentObjectType)
        {
            if (propertyMetaData?.Type.Equals("WktString", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return typeof(AspectPropertyValue<WktString>);
            }
            const string weightedPrefix = "Weighted";
            if (propertyMetaData?.Type is string propTypeRaw
                && propTypeRaw.StartsWith(weightedPrefix, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                var realType = propTypeRaw.Substring(weightedPrefix.Length);
                var propClone = new PropertyMetaData
                {
                    Name = propertyMetaData.Name,
                    Type = realType,
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
            return typeof(AspectPropertyValue<>).MakeGenericType(
                base.GetPropertyType(propertyMetaData, parentObjectType));
        }
    }
}
