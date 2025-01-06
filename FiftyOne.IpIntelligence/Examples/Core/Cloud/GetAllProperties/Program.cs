/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2020 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
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

using FiftyOne.IpIntelligence;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using System;
using System.Collections;
using System.Linq;
/// <summary>
/// @example Cloud/GetAllProperties/Program.cs
/// 
/// @include{doc} example-get-all-properties-cloud.txt
/// 
/// @include{doc} example-require-resourcekey.txt
/// 
/// Expected output:
/// ```
/// ...
/// countyname = Reading:0.35714287,Dumfries and Galloway:0.53571427,Swindon:0.10714286
/// iprangeend = 185.28.167.127
/// iprangestart = 185.28.167.0
/// ispcountrycode = Unknown:1
/// ispname = Unknown:1
/// locationboundnorthwest = 55.37805,-3.435973
/// locationboundsoutheast = 51.457577,-0.9760192
/// mcc = Unknown:1
/// mnc = Unknown:1
/// networkid = 6416:1.000000|6417:1.000000|0:1.000000|0:1.000000|7217:0.769231|7297:0.230769|7403:0.464286|7481:0.535714|7330:1.000000|7579:0.357143|7737:0.535714|7738:0.107143|7884:0.357143|8065:0.535714|8066:0.107...
/// regionname = South East:0.7692308,South West England:0.23076923
/// statename = England:0.4642857,Scotland:0.5357142
/// ...
/// ```
/// </summary>
namespace GetAllProperties
{
    class Program
    {
        private static string ipv4Address = "185.28.167.127";

        static void Main(string[] args)
        {
            // Obtain a resource key for free at https://configure.51degrees.com
            string resourceKey = "!!YOUR_RESOURCE_KEY!!";

            if (resourceKey.StartsWith("!!"))
            {
                Console.WriteLine("You need to create a resource key at " +
                    "https://configure.51degrees.com and paste it into the code, " +
                    "replacing !!YOUR_RESOURCE_KEY!!.");
                Console.WriteLine("Make sure to include all the properties " +
                    "that you want to see displayed by this example.");

            }
            else
            {
                // Create the pipeline
                using (var pipeline = new IpiPipelineBuilder()
                    // Tell it that we want to use cloud and pass our resource key.
                    .UseCloud(resourceKey)
                    .Build())
                {
                    // Output details for a mobile User-Agent.
                    AnalyseIpAddress(ipv4Address, pipeline);
                }
            }
#if (DEBUG)
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
#endif
        }

        static void AnalyseIpAddress(string ipAddress, IPipeline pipeline)
        {
            // Create the FlowData instance.
            var data = pipeline.CreateFlowData();
            // Add an IPv4 address as evidence.
            data.AddEvidence("query.client-ip-51d", ipAddress);
            // Process the supplied evidence.
            data.Process();
            // Get device data from the flow data.
            var device = data.Get<IIpData>();
            Console.WriteLine($"What property values are associated with " +
                $"the IP address '{ipAddress}'?");

            // Iterate through IP data results, displaying all values.
            foreach (var property in device.AsDictionary()
                .OrderBy(p => p.Key))
            {
                Console.WriteLine($"{property.Key} = {GetValueToOutput(property.Value)}");
            }
        }

        /// <summary>
        /// Convert the given value into a human-readable string representation 
        /// </summary>
        /// <param name="propertyValue">
        /// Property value object to be converted
        /// </param>
        /// <returns></returns>
        private static string GetValueToOutput(object propertyValue)
        {
            if (propertyValue == null)
            {
                return "NULL";
            }

            var basePropetyType = propertyValue.GetType();
            var basePropertyValue = propertyValue;

            if (propertyValue is IAspectPropertyValue aspectPropertyValue)
            {
                if (aspectPropertyValue.HasValue)
                {
                    // Get the type and value parameters from the 
                    // AspectPropertyValue instance.
                    basePropetyType = basePropetyType.GenericTypeArguments[0];
                    basePropertyValue = aspectPropertyValue.Value;
                }
                else
                {
                    // The property has no value so output the reason.
                    basePropetyType = typeof(string);
                    basePropertyValue = $"NO VALUE ({aspectPropertyValue.NoValueMessage})";
                }
            }

            if (basePropetyType != typeof(string) &&
                typeof(IEnumerable).IsAssignableFrom(basePropetyType))
            {
                // Property is an IEnumerable (that is not a string)
                // so return a comma-separated list of values.
                var collection = basePropertyValue as IEnumerable;
                var output = "";
                foreach (var entry in collection)
                {
                    if (output.Length > 0) { output += ","; }
                    output += entry.ToString();
                }
                return output;
            }
            else
            {
                var str = basePropertyValue.ToString();
                // Truncate any long strings to 200 characters
                if (str.Length > 200)
                {
                    str = str.Remove(200);
                    str += "...";
                }
                return str;
            }
        }
    }
}
