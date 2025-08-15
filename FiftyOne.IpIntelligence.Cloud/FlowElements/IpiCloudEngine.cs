/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2025 51 Degrees Mobile Experts Limited, Davidson House,
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace FiftyOne.IpIntelligence.Cloud.FlowElements
{
    /// <summary>
    /// Engine that takes the JSON response from the 
    /// <see cref="CloudRequestEngine"/> and uses it populate a 
    /// IpDataCloud instance for easier consumption.
    /// </summary>
    public class IpiCloudEngine : CloudAspectEngineBase<IpDataCloud>
    {
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

        private static JsonConverter[] JSON_CONVERTERS = new JsonConverter[]
        {
            new CloudJsonConverter()
        };

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
                // Extract data from JSON to the aspectData instance.
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                var propertyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictionary["ip"].ToString(),
                    new JsonSerializerSettings()
                    {
                        Converters = JSON_CONVERTERS,
                    });

                var ip = CreateAPVDictionary2(propertyValues, Properties.ToList());
                aspectData.PopulateFrom(ip);
            }
        }

        /// <summary>
        /// Use the supplied cloud data to create a dictionary of 
        /// <see cref="AspectPropertyValue{T}"/> instances.
        /// </summary>
        /// <remarks>
        /// This method uses the meta-data exposed by the 
        /// <see cref="CloudAspectEngineBase{T}.Properties"/> collection to determine
        /// if a given entry in the supplied cloudData should
        /// be converted to an <see cref="AspectPropertyValue{T}"/>
        /// or not.
        /// If not it will be output unchanged. If it is then a new
        /// <see cref="AspectPropertyValue{T}"/> instance will be created 
        /// and the value from the cloud data assigned to it.
        /// If the value is null then the code will look for a property
        /// in the cloud data with the same name suffixed with 'nullreason'.
        /// If it exists, then it's value will be used to set the 
        /// noValueMessage on the new <see cref="AspectPropertyValue{T}"/>.
        /// </remarks>
        /// <param name="cloudData">
        /// The cloud data to be processed.
        /// Keys are flat property names (i.e. no '.' separators).
        /// Values are the property values.
        /// </param>
        /// <param name="propertyMetaData">
        /// The meta-data for the properties in the data.
        /// This will usually be the list from <see cref="CloudAspectEngineBase{T}.Properties"/>
        /// but will be different if dealing with sub-properties.
        /// </param>
        /// <returns>
        /// A dictionary containing the original values converted to 
        /// <see cref="AspectPropertyValue{T}"/> instances where needed.
        /// Any entries in the source dictionary where the key ends 
        /// with 'nullreason' will not appear in the output.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if a required parameter is null.
        /// </exception>
        private Dictionary<string, object> CreateAPVDictionary2(
            Dictionary<string, object> cloudData,
            IReadOnlyList<IElementPropertyMetaData> propertyMetaData)
        {
            if (cloudData == null) throw new ArgumentNullException(nameof(cloudData));

            // Convert the meta-data to a dictionary for faster access.
            var metaDataDictionary = propertyMetaData.ToDictionary(
                p => p.Name, p => p,
                StringComparer.OrdinalIgnoreCase);

            Dictionary<string, object> result = new Dictionary<string, object>();
            // Iterate through all entries in the source data where the
            // key is not suffixed with 'nullreason'.
            foreach (var property in cloudData
                .Where(kvp => kvp.Key.EndsWith("nullreason",
                    StringComparison.OrdinalIgnoreCase) == false))
            {
                var outputValue = property.Value;

                if (metaDataDictionary
                        .Where(x => x.Key.ToUpperInvariant() == property.Key.ToUpperInvariant())
                        .Select(x => x.Value)
                        .FirstOrDefault() is IElementPropertyMetaData metaData)
                {
                    if (typeof(IAspectPropertyValue).IsAssignableFrom(metaData.Type))
                    {
                        // If this property has a type of AspectPropertyValue
                        // then create a new instance and populate it.
                        var apvType = typeof(AspectPropertyValue<>);
                        var genericType = apvType.MakeGenericType(metaData.Type.GetGenericArguments());
                        object obj = Activator.CreateInstance(genericType);
                        var apv = obj as IAspectPropertyValue;
                        if (property.Value != null)
                        {
                            apv.Value = ToValueForAPV(property.Value, metaData.Type.GetGenericArguments()[0]);
                        }
                        else
                        {
                            // Value is null so check if we have a 
                            // corresponding reason.
                            // We need to set the no value message with 
                            // reflection as the property is read only 
                            // through the interface.
                            var messageProperty = genericType.GetProperty("NoValueMessage");
                            if (cloudData.TryGetValue(property.Key + "nullreason", out object nullreason))
                            {
                                messageProperty.SetValue(apv, nullreason);
                            }
                            else
                            {
                                messageProperty.SetValue(apv, "Unknown");
                            }
                        }
                        outputValue = apv;
                    }
                }
                else
                {
                    Logger.LogWarning($"No meta-data entry for property " +
                        $"'{property.Key}' in '{GetType().Name}'");
                }

                result.Add(property.Key, outputValue);
            }
            return result;
        }

        /// <summary>
        /// Convert value return by cloud in json (weakly typed)
        /// to a value expected by AspectPropertyValue (strongly typed)
        /// </summary>
        /// <param name="rawValue">Original value.</param>
        /// <param name="valueType">Target type.</param>
        /// <returns>Converted value.</returns>
        public static object ToValueForAPV(object rawValue, Type valueType)
        {
            if (valueType is null) throw new ArgumentNullException(nameof(valueType));
            if (rawValue is null)
            {
                return null;
            }
            if (valueType.IsPrimitive && rawValue.GetType().IsPrimitive && rawValue is IConvertible)
            {
                object v = Convert.ChangeType(rawValue, valueType, CultureInfo.InvariantCulture);
                return v;
            }

            if (valueType.IsGenericType)
            {
                var defType = valueType.GetGenericTypeDefinition();
                var genericArgs = valueType.GetGenericArguments();
                if (defType.IsInterface)
                {
                    if (genericArgs.Length == 1)
                    {
                        var genericType = genericArgs[0];
                        {
                            var listType = typeof(List<>).MakeGenericType(genericType);
                            if (valueType.IsAssignableFrom(listType)
                                && rawValue.GetType().GetInterfaces().Any(x => typeof(IEnumerable).IsAssignableFrom(x)))
                            {
                                object list = Activator.CreateInstance(listType);
                                var adder = listType.GetMethod("Add");
                                foreach (var nextObj in (IEnumerable)rawValue)
                                {
                                    adder.Invoke(list, new object[] { ToValueForAPV(nextObj, genericType) });
                                }
                                return list;
                            }
                        }
                        {
                            var weightedType = typeof(WeightedValue<>).MakeGenericType(genericType);
                            if (valueType.IsAssignableFrom(weightedType)
                                && rawValue is IDictionary<string, object> rawValueDic)
                            {
                                var weighting = rawValueDic.FirstOrDefault(
                                    p => p.Key.ToUpperInvariant() == nameof(WeightedValue<string>.RawWeighting).ToUpperInvariant())
                                    .Value;
                                var value = rawValueDic.FirstOrDefault(
                                    p => p.Key.ToUpperInvariant() == nameof(WeightedValue<string>.Value).ToUpperInvariant())
                                    .Value;
                                if (!(weighting is null)
                                    && weighting.GetType().IsPrimitive && weighting is IConvertible
                                    && !(value is null))
                                {
                                    object weighted = Activator.CreateInstance(weightedType, new object[] {
                                        Convert.ChangeType(weighting, typeof(ushort), CultureInfo.InvariantCulture),
                                        ToValueForAPV(value, genericType),
                                    });
                                    return weighted;
                                }
                            }
                        }
                    }
                }
            }
            if (valueType == typeof(IPAddress) && rawValue.GetType() == typeof(string))
            {
                return IPAddress.Parse((string)rawValue);
            }
            return ToValueForAPV_Base(rawValue, valueType);
        }

        private static object ToValueForAPV_Base(object rawValue, Type valueType)
        {
            if (valueType == typeof(JavaScript))
            {
                return new JavaScript(rawValue.ToString());
            }
            if (valueType == typeof(Dictionary<string, string>))
            {
                return ((Newtonsoft.Json.Linq.JObject)rawValue).ToObject<Dictionary<string, string>>();
            }
            return rawValue;
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
            return typeof(AspectPropertyValue<>).MakeGenericType(
                typeof(IReadOnlyList<>).MakeGenericType(
                    typeof(IWeightedValue<>).MakeGenericType(
                        base.GetPropertyType(propertyMetaData, parentObjectType))));
        }
    }
}
