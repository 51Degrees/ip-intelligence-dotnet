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

using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.Pipeline.Engines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// @example OnPremise/GettingStarted/Program.cs
/// 
/// @include{doc} example-getting-started-ipi.txt
/// 
/// This example is available in full on [GitHub](https://github.com/51Degrees/ip-intelligence-dotnet/blob/master/FiftyOne.IpIntelligence/Examples/Framework/OnPremise/GettingStarted/Program.cs). 
/// 
/// @include{doc} example-require-datafile-ipi.txt
/// 
/// Required NuGet Dependencies:
/// - FiftyOne.IpIntelligence
/// 
/// Expected output:
/// ```
/// ...
/// This example demonstrates some of the information that can be determined from an IP address.
///
/// Querying IPv4 Address
/// =====================
/// 1. What is the IP address range that is matched for '185.28.167.77'?
///         185.28.164.0 - 185.28.167.255
/// 2. What are the source countries for requests in this IP range?
///         'gb', 100%
/// 3. What is the average location for requests in this IP range?
///         53.576283,-2.3281078
/// 
/// Querying IPv6 Address
/// =====================
/// 1. What is the IP address range that is matched for '2001:4860:4860::8888'?
///         2001:4860:: - 2001:4860:ffff:ffff:ffff:ffff:ffff:ffff
/// 2. What are the source countries for requests in this IP range?
///         'ZZ', 100%
/// 3. What is the average location for requests in this IP range?
///         0,0
/// Complete. Press key to exit.
/// ...
/// ```
/// </summary>
namespace FiftyOne.IpIntelligence.Examples.OnPremise.GettingStarted
{
    public class Program
    {
        public class Example : ExampleBase
        {
            private static string ipv4Address = "185.28.167.77";
            private static string ipv6Address = "2001:4860:4860::8888";

            public void Run(string dataFile)
            {
                FileInfo f = new FileInfo(dataFile);
                Console.WriteLine($"Using data file at '{f.FullName}'");
                // Use the IpiOnPremisePipelineBuilder to build a new Pipeline 
                // to use an on-premise IP intelligence engine with the low memory
                // performance profile.
                using (var pipeline = new IpiPipelineBuilder()
                    .UseOnPremise(dataFile, null, false)
                    // Prefer low memory profile where all data streamed 
                    // from disk on-demand. Experiment with other profiles.
                    //.SetPerformanceProfile(PerformanceProfiles.HighPerformance)
                    .SetPerformanceProfile(PerformanceProfiles.LowMemory)
                    //.SetPerformanceProfile(PerformanceProfiles.Balanced)
                    .Build())
                {
                    Console.WriteLine("This example demonstrates some of the information that can be determined from an IP address.");
                    Console.WriteLine();
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

            private void AnalyseIpAddress(string ipAddress, IPipeline pipeline)
            {
                // Create the FlowData instance.
                var data = pipeline.CreateFlowData();
                // Add an IPv4 as evidence.
                data.AddEvidence("query.client-ip-51d", ipAddress);
                // Process the supplied evidence.
                data.Process();
                // Get IP data from the flow data.
                var ip = data.Get<IIpData>();

                // TODO: Revert to reading other properties
                //{
                //    var networkName = ip.NetworkName;
                //    Console.WriteLine($"0.1. What is the network name that is matched for '{ipAddress}'?");
                //    if (!networkName.HasValue)
                //    {
                //        Console.WriteLine($"\t{networkName.NoValueMessage} - {networkName.NoValueMessage}");
                //    }
                //    else
                //    {
                //        var networkNameValues = string.Join(", ", networkName.Value.Select(x => $"('{x.Value}' @ {x.Weight})"));
                //        Console.WriteLine($"\t[{networkName.Value.Count}]: {networkNameValues}");
                //    }
                //}
                {
                    var coordinate = ip.Coordinate;
                    Console.WriteLine($"0.2. What is the coordinate that is matched for '{ipAddress}'?");
                    if (!coordinate.HasValue)
                    {
                        Console.WriteLine($"\t{coordinate.NoValueMessage} - {coordinate.NoValueMessage}");
                    }
                    else
                    {
                        var coordinateValues = string.Join(", ", coordinate.Value.Select(x => $"(lat = {x.Value.Latitude}, long = {x.Value.Longitude}) @ {x.Weight})"));
                        Console.WriteLine($"\t[{coordinate.Value.Count}]: {coordinateValues}");
                    }
                }
                return;

                var rangeStart = ip.IpRangeStart;
                var rangeEnd = ip.IpRangeEnd;
                Console.WriteLine($"1. What is the IP address range that is matched for '{ipAddress}'?");
                // Output the value of the 'RangeStart' and 'RangeEnd' properties.
                Console.WriteLine($"\t{(rangeStart.HasValue ? rangeStart.ToString() : rangeStart.NoValueMessage)}" +
                    $" - {(rangeEnd.HasValue ? rangeEnd.ToString() : rangeEnd.NoValueMessage)}");

                // Obtain the list of countries where the IP address is possibly from
                var countries = ip.CountryCode;
                Console.WriteLine($"2. What are the source countries for requests in this IP range?");
                if (countries.HasValue)
                {
                    IEnumerator<WeightedValue<string>> enumerator = countries.Value.GetEnumerator();
                    while (enumerator.MoveNext()) 
                    {
                        Console.WriteLine($"\t'{enumerator.Current.Value}', {enumerator.Current.Weight * 100}%");
                    }
                }
                else 
                {
                    Console.WriteLine($"\t{rangeStart.NoValueMessage}");
                }

                // Obtain the Average Location of the IP address
                var averageLocation = ip.AverageLocation;
                Console.WriteLine($"3. What is the average location for requests in this IP range?");
                if (averageLocation.HasValue)
                {
                    Console.WriteLine($"\t{averageLocation.Value.Latitude},{averageLocation.Value.Longitude}");
                }
                else
                {
                    Console.WriteLine($"\t{averageLocation.NoValueMessage}");
                }
            }
        }

        static void Main(string[] args)
        {
#if NETCORE
            var defaultDataFile = "..\\..\\..\\..\\..\\..\\..\\..\\ip-intelligence-cxx\\ip-intelligence-data\\51Degrees-LiteV4.1.ipi";
#else
            var defaultDataFile = "..\\..\\..\\..\\..\\..\\..\\ip-intelligence-cxx\\ip-intelligence-data\\51Degrees-LiteV4.1.ipi";
#endif
            var dataFile = args.Length > 0 ? args[0] : defaultDataFile;
            new Example().Run(dataFile);
#if (DEBUG)
            Console.WriteLine("Complete. Press key to exit.");
            Console.ReadKey();
#endif
        }
    }
}
