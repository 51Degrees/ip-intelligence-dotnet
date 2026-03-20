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

using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace FiftyOne.IpIntelligence.Shared.Services
{
    /// <summary>
    /// Static helper for converting cloud data values to AspectPropertyValue 
    /// types and creating APV dictionaries.
    /// </summary>
    public static class CloudDataHelper
    {
        /// <summary>
        /// Default type converter that handles IPAddress and WktString types.
        /// Used by IpiCloudEngine for IP intelligence data.
        /// </summary>
        public static readonly Func<object, Type, object> IpTypeConverter =
            (rawValue, valueType) =>
        {
            if (valueType == typeof(IPAddress) && rawValue is string ipString)
            {
                return IPAddress.Parse(ipString);
            }
            if (valueType == typeof(WktString))
            {
                object rawValue2 = rawValue is JObject jObj
                    ? jObj.ToObject<Dictionary<string, object>>()
                    : rawValue;
                if (rawValue2 is IDictionary<string, object> rawWktValueDic)
                {
                    var value = rawWktValueDic.FirstOrDefault(
                        p => p.Key.Equals("value",
                            StringComparison.OrdinalIgnoreCase))
                        .Value;
                    if (value is string wktValue)
                    {
                        return new WktString(wktValue);
                    }
                }
            }
            return null;
        };

        /// <summary>
        /// Convert value returned by cloud in json (weakly typed)
        /// to a value expected by AspectPropertyValue (strongly typed).
        /// </summary>
        /// <param name="rawValue">The raw value from JSON</param>
        /// <param name="valueType">The target type</param>
        /// <param name="typeConverter">
        /// Optional type-specific converter for handling special types like 
        /// IPAddress. Called first to give type-specific converters priority 
        /// over generic handling.
        /// </param>
        /// <returns>The converted value</returns>
        public static object ToValueForAPV(
            object rawValue,
            Type valueType,
            Func<object, Type, object> typeConverter = null)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            if (rawValue == null)
            {
                return null;
            }

            if (typeConverter != null)
            {
                var result = typeConverter(rawValue, valueType);
                if (result != null && result.GetType() == valueType)
                {
                    return result;
                }
            }

            if (valueType.IsPrimitive && 
                rawValue.GetType().IsPrimitive && 
                rawValue is IConvertible)
            {
                return Convert.ChangeType(
                    rawValue, 
                    valueType,
                    CultureInfo.InvariantCulture);
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
                            var listType = typeof(List<>)
                                .MakeGenericType(genericType);
                            if (valueType.IsAssignableFrom(listType)
                                && rawValue
                                .GetType()
                                .GetInterfaces()
                                .Any(x => typeof(IEnumerable)
                                .IsAssignableFrom(x)))
                            {
                                object list = Activator.CreateInstance(listType);
                                var adder = listType.GetMethod("Add");
                                foreach (var nextObj in (IEnumerable)rawValue)
                                {
                                    adder.Invoke(
                                        list, 
                                        new object[] 
                                        { 
                                            ToValueForAPV(
                                                nextObj, 
                                                genericType, 
                                                typeConverter) 
                                        });
                                }
                                return list;
                            }
                        }
                        {
                            var weightedType = typeof(WeightedValue<>)
                                .MakeGenericType(genericType);
                            if (valueType.IsAssignableFrom(weightedType) &&
                                rawValue is IDictionary<string, object> 
                                rawValueDic)
                            {
                                var weighting = rawValueDic.FirstOrDefault(
                                    p => p.Key.ToUpperInvariant() 
                                    == nameof(WeightedValue<string>.RawWeighting)
                                    .ToUpperInvariant())
                                    .Value;
                                var value = rawValueDic.FirstOrDefault(
                                    p => p.Key.ToUpperInvariant() 
                                    == nameof(WeightedValue<string>.Value)
                                    .ToUpperInvariant())
                                    .Value;
                                if ((weighting is null) == false && 
                                    weighting.GetType().IsPrimitive && 
                                    weighting is IConvertible && 
                                    (value is null) == false)
                                {
                                    return Activator
                                        .CreateInstance(
                                            weightedType, 
                                            new object[] {
                                            Convert.ChangeType(
                                                weighting,
                                                typeof(ushort),
                                                CultureInfo.InvariantCulture),
                                        ToValueForAPV(
                                            value, 
                                            genericType,
                                            typeConverter),
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return ToValueForAPVSimple(rawValue, valueType);
        }

        /// <summary>
        /// Base conversion for simple types like JavaScript and Dictionary.
        /// </summary>
        private static object ToValueForAPVSimple(
            object rawValue,
            Type valueType)
        {
            if (rawValue == null)
            {
                return null;
            }

            if (valueType == typeof(JavaScript))
            {
                return new JavaScript(rawValue.ToString());
            }
            if (valueType == typeof(Dictionary<string, string>) && 
                rawValue is Newtonsoft.Json.Linq.JObject jObj)
            {
                return jObj.ToObject<Dictionary<string, string>>();
            }
            return rawValue;
        }

        /// <summary>
        /// Use the supplied cloud data to create a dictionary of 
        /// <see cref="IAspectPropertyValue"/> instances.
        /// </summary>
        /// <param name="cloudData">The cloud data to be processed</param>
        /// <param name="propertyMetaData">The meta-data for the properties</param>
        /// <param name="typeConverter">
        /// Optional type-specific converter for handling special types.
        /// </param>
        /// <param name="logger">Optional logger for warnings</param>
        /// <param name="engineTypeName">Engine type name for logging</param>
        /// <returns>A dictionary with values converted to AspectPropertyValue 
        /// where needed</returns>
        public static Dictionary<string, object> CreateAPVDictionary(
            Dictionary<string, object> cloudData,
            IReadOnlyList<IElementPropertyMetaData> propertyMetaData,
            Func<object, Type, object> typeConverter = null,
            ILogger logger = null,
            string engineTypeName = null)
        {
            if (cloudData == null)
            {
                throw new ArgumentNullException(nameof(cloudData));
            }

            var metaDataDictionary = propertyMetaData.ToDictionary(
                p => p.Name, p => p,
                StringComparer.OrdinalIgnoreCase);

            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (var property in cloudData
                .Where(kvp => kvp.Key.EndsWith("nullreason",
                    StringComparison.OrdinalIgnoreCase) == false))
            {
                var outputValue = property.Value;

                if (metaDataDictionary
                        .Where(x => x.Key.ToUpperInvariant() 
                            == property.Key.ToUpperInvariant())
                        .Select(x => x.Value)
                        .FirstOrDefault() is IElementPropertyMetaData metaData)
                {
                    if (typeof(IAspectPropertyValue)
                        .IsAssignableFrom(metaData.Type))
                    {
                        var apvType = typeof(AspectPropertyValue<>);
                        var genericType = apvType.MakeGenericType(
                            metaData.Type.GetGenericArguments());
                        object obj = Activator.CreateInstance(genericType);
                        var apv = obj as IAspectPropertyValue;
                        if (property.Value != null)
                        {
                            apv.Value = ToValueForAPV(
                                property.Value, 
                                metaData.Type.GetGenericArguments()[0], 
                                typeConverter);
                        }
                        else
                        {
                            var messageProperty = genericType
                                .GetProperty("NoValueMessage");
                            if (cloudData.TryGetValue(
                                property.Key + "nullreason",
                                out object nullreason))
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
                    logger?.LogWarning($"No meta-data entry for property " +
                        $"'{property.Key}' in '{engineTypeName ?? "Unknown"}'");
                }

                result.Add(property.Key, outputValue);
            }
            return result;
        }
    }
}
