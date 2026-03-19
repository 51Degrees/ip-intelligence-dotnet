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

using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FiftyOne.IpIntelligence.Shared.Services
{
    /// <summary>
    /// Internal helper for converting cloud data values to AspectPropertyValue types.
    /// </summary>
    public static class CloudDataHelperInternal
    {
        /// <summary>
        /// Convert value returned by cloud in json (weakly typed)
        /// to a value expected by AspectPropertyValue (strongly typed)
        /// </summary>
        public static object ToValueForAPV(object rawValue, Type valueType)
        {
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));
            if (rawValue == null) return null;

            if (valueType.IsPrimitive && rawValue.GetType().IsPrimitive && rawValue is IConvertible)
            {
                return Convert.ChangeType(rawValue, valueType, CultureInfo.InvariantCulture);
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
                                    return Activator.CreateInstance(weightedType, new object[] {
                                        Convert.ChangeType(weighting, typeof(ushort), CultureInfo.InvariantCulture),
                                        ToValueForAPV(value, genericType),
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
        public static object ToValueForAPVSimple(object rawValue, Type valueType)
        {
            if (rawValue == null) return null;
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
    }
}
