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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace FiftyOne.IpIntelligence.Shared.Services
{
    /// <summary>
    /// IP-specific cloud data helper that handles IPAddress and WktString types.
    /// </summary>
    public class IpCloudDataHelper : CloudDataHelper
    {
        /// <inheritdoc/>
        public override object ToValueForAPV(object rawValue, Type valueType)
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
                        p => p.Key.Equals("value", StringComparison.OrdinalIgnoreCase))
                        .Value;
                    if (value is string wktValue)
                    {
                        return new WktString(wktValue);
                    }
                }
            }
            return base.ToValueForAPV(rawValue, valueType);
        }
    }
}
