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
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.IpIntelligence.Shared.Services
{
    /// <summary>
    /// Helper service for creating AspectPropertyValue dictionaries from cloud data.
    /// </summary>
    public interface ICloudDataHelper
    {
        /// <summary>
        /// Use the supplied cloud data to create a dictionary of 
        /// <see cref="IAspectPropertyValue"/> instances.
        /// </summary>
        Dictionary<string, object> CreateAPVDictionary(
            Dictionary<string, object> cloudData,
            IReadOnlyList<IElementPropertyMetaData> propertyMetaData,
            ILogger logger = null,
            string engineTypeName = null);

        /// <summary>
        /// Convert value returned by cloud in json (weakly typed)
        /// to a value expected by AspectPropertyValue (strongly typed).
        /// </summary>
        object ToValueForAPV(object rawValue, Type valueType);
    }

    /// <summary>
    /// Implementation of <see cref="ICloudDataHelper"/>.
    /// </summary>
    public class CloudDataHelper : ICloudDataHelper
    {
        /// <inheritdoc/>
        public virtual object ToValueForAPV(object rawValue, Type valueType)
        {
            return CloudDataHelperInternal.ToValueForAPV(rawValue, valueType);
        }

        /// <inheritdoc/>
        public Dictionary<string, object> CreateAPVDictionary(
            Dictionary<string, object> cloudData,
            IReadOnlyList<IElementPropertyMetaData> propertyMetaData,
            ILogger logger = null,
            string engineTypeName = null)
        {
            if (cloudData == null) throw new ArgumentNullException(nameof(cloudData));

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
                        .Where(x => x.Key.ToUpperInvariant() == property.Key.ToUpperInvariant())
                        .Select(x => x.Value)
                        .FirstOrDefault() is IElementPropertyMetaData metaData)
                {
                    if (typeof(IAspectPropertyValue).IsAssignableFrom(metaData.Type))
                    {
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
                    logger?.LogWarning($"No meta-data entry for property " +
                        $"'{property.Key}' in '{engineTypeName ?? "Unknown"}'");
                }

                result.Add(property.Key, outputValue);
            }
            return result;
        }
    }
}
