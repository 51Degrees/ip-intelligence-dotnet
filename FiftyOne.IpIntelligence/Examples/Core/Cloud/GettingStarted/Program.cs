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

using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using System;
using System.Collections.Generic;
/// <summary>
/// @example Cloud/GettingStarted/Program.cs
///
/// @include{doc} exmaple-getting-started-cloud-ipi.txt
/// 
/// This example is available in full on [GitHub](https://github.com/51Degrees/ip-intelligence-dotnet/blob/master/FiftyOne.IpIntelligence/Examples/Core/Cloud/Getting%20Started/Program.cs). 
/// 
/// @include{doc} example-require-resourcekey.txt
///
/// Required NuGet Dependencies:
/// - FiftyOne.IpIntelligence
/// 
/// Expected output:
/// ```
/// ...
/// Querying IPv4 Address
/// =====================
/// 1. What is the RangeStart of '185.28.167.77'?
///         185.28.167.0
/// 2. What are the codes of the countries where the '185.28.167.50' is possibly from?
///         'gb', 100%
/// 3. What is the average location of the '185.28.167.50'?
///         53.576283,-2.3281078
/// 4. What is the NetworkId of the '185.28.167.50'?
///         6416:1.000000|6417:1.000000|0:1.000000|...
/// 
/// Querying IPv4 Address
/// =====================
/// 1. What is the RangeStart of '2001:4860:4860::8888'?
///         2001:4700::
/// 2. What are the codes of the countries where the '2001:4860:4860::8888' is possibly from?
///         'ZZ', 100%
/// 3. What is the average location of the '2001:4860:4860::8888'?
///         0,0
/// 4. What is the NetworkId of the '2001:4860:4860::8888'?
///         6416:1.000000|6417:1.000000|0:1.000000|...
/// Done. Press any key to exit.
/// ...
/// ```
/// </summary>
namespace FiftyOne.IpIntelligence.Examples.Cloud.GettingStarted
{
    class Program
    {
        private static string ipv4Address = "185.28.167.77";
        private static string ipv6Address = "2001:4860:4860::8888";

        static void Main(string[] args)
        {
            // Obtain a resource key for free at https://configure.51degrees.com
            // Make sure to include the 'IpRangeStart', 'Countries', and 
            // 'AverageLocation' properties as they are used by this example.
            string resourceKey = "AQS5HKcyfUmfWEX310g";

            if (resourceKey.StartsWith("!!"))
            {
                Console.WriteLine("You need to create a resource key at " +
                    "https://configure.51degrees.com and paste it into the code, " +
                    "replacing !!YOUR_RESOURCE_KEY!!.");
                Console.WriteLine("Make sure to include the 'IsMobile' " +
                    "property as it is used by this example.");
            }
            else
            {
                // Build a new Pipeline with a cloud-based IP intelligence engine.
                using (var pipeline = new IpiPipelineBuilder()
                    // Tell it that we want to use cloud and pass our resource key.
                    .UseCloud(resourceKey)
                    .Build())
                {
                    // First try an IPv4 address.
                    Console.WriteLine("Querying IPv4 Address");
                    Console.WriteLine("=====================");
                    AnalyseIpAddress(ipv4Address, pipeline);
                    Console.WriteLine();
                    // Now try an IPv6 address.
                    Console.WriteLine("Querying IPv6 Address");
                    Console.WriteLine("=====================");
                    AnalyseIpAddress(ipv6Address, pipeline);
                }
            }
#if (DEBUG)
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
#endif
        }

        static void AnalyseIpAddress(string ipAddress, IPipeline pipeline)
        {
            // Create a new FlowData instance ready to be populated with evidence for the
            // Pipeline.
            var data = pipeline.CreateFlowData();
            // Add an IP address string to the evidence collection.
            data.AddEvidence("query.client-ip-51d", ipAddress);
            // Process the supplied evidence.
            data.Process();
            // Get IP data from the flow data.
            var ip = data.Get<IIpIntelligenceData>();
            {
                var rangeStart = ip.IpRangeStart;
                var rangeEnd = ip.IpRangeEnd;
                Console.WriteLine($"1. What is the IP address range that is matched for '{ipAddress}'?");
                // Output the value of the 'RangeStart' and 'RangeEnd' properties.
                Console.WriteLine($"\t{(rangeStart.HasValue ? rangeStart.ToString() : rangeStart.NoValueMessage)}" +
                    $" - {(rangeEnd.HasValue ? rangeEnd.ToString() : rangeEnd.NoValueMessage)}");
            }
            {
                // Obtain the list of countries where the IP address is possibly from
                var countries = ip.CountryCode;
                Console.WriteLine($"2. What are the source countries for requests in this IP range?");
                if (countries.HasValue)
                {
                    IEnumerator<IWeightedValue<string>> enumerator = countries.Value.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Console.WriteLine($"\t'{enumerator.Current.Value}', {enumerator.Current.Weighting() * 100}%");
                    }
                }
                else
                {
                    Console.WriteLine($"\t{countries.NoValueMessage}");
                }
            }
            // Obtain the Average Location of the IP address
            {
                var latitude = ip.Latitude;
                Console.WriteLine($"3. What is the latitude for requests in this IP range?");

                if (latitude.HasValue)
                {
                    foreach (var nextValue in latitude.Value)
                    {
                        Console.WriteLine($"\t'{nextValue.Value}', {nextValue.Weighting() * 100}%");
                    }
                }
                else
                {
                    Console.WriteLine($"\t{latitude.NoValueMessage}");
                }
            }
            {
                var longitude = ip.Longitude;
                Console.WriteLine($"4. What is the longitude for requests in this IP range?");

                if (longitude.HasValue)
                {
                    foreach (var nextValue in longitude.Value)
                    {
                        Console.WriteLine($"\t'{nextValue.Value}', {nextValue.Weighting() * 100}%");
                    }
                }
                else
                {
                    Console.WriteLine($"\t{longitude.NoValueMessage}");
                }
            }
            {
                var accuracyRadius = ip.AccuracyRadius;
                Console.WriteLine($"4. What is the accuracy radius for requests in this IP range?");

                if (accuracyRadius.HasValue)
                {
                    foreach (var nextValue in accuracyRadius.Value)
                    {
                        Console.WriteLine($"\t'{nextValue.Value}', {nextValue.Weighting() * 100}%");
                    }
                }
                else
                {
                    Console.WriteLine($"\t{accuracyRadius.NoValueMessage}");
                }
            }
        }
    }
}
