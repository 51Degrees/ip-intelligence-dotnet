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

using FiftyOne.Pipeline.Core.Configuration;
using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// @example OnPremise/ConfigureFromFile/Program.cs
/// 
/// @include{doc} example-configure-from-file-ipi.txt
/// 
/// This example is available in full on [GitHub](https://github.com/51Degrees/ip-intelligence-dotnet/blob/master/FiftyOne.IpIntelligence/Examples/OnPremise/ConfigureFromFile/Program.cs). 
/// 
/// @include{doc} example-require-datafile-ipi.txt
/// 
/// Required NuGet Dependencies:
/// - FiftyOne.IpIntelligence
/// - Microsoft.Extensions.Configuration.Json OR Microsoft.Extensions.Configuration.Xml
/// 
/// Expected output:
/// ```
/// ...
/// Querying IPv4 Address
/// =====================
/// 1. What is the RangeStart of '185.28.167.77'?
///         185.28.167.0
/// 2. What is the Country where the '185.28.167.77' is from?
///         'gb', 100%
/// 3. What is the NetworkId of the '185.28.167.77'?
///         6416:1.000000|6417:1.000000|0:1.000000|...
/// 
/// Querying IPv6 Address
/// =====================
/// 1. What is the RangeStart of '2001:4860:4860::8888'?
///         2001:4700::
/// 2. What is the Country where the '2001:4860:4860::8888' is from?
///         'ZZ', 100%
/// 3. What is the NetworkId of the '2001:4860:4860::8888'?
///         6416:1.000000|6417:1.000000|0:1.000000|...
/// ...
/// ```
/// </summary>
namespace FiftyOne.IpIntelligence.Examples.OnPremise.ConfigureFromFile
{
    public class Program
    {
        public class Example : ExampleBase
        {
            private static string Ipv4Address = "185.28.167.77";
            private static string Ipv6Address = "2001:4860:4860::8888";

            public void Run()
            {
                Console.WriteLine($"Constructing pipeline from configuration file.");

                // Load the assembly with the on-premise IP intelligence engine.
                // This is required because nothing else in the example 
                // references any types from this assembly and it will
                // need to be loaded into the app domain in order for 
                // the PipelineBuilder to find the engine builder to use 
                // when creating the IP intelligence engine.
                // (i.e. the 'IpiOnPremiseEngineBuilder'
                // specified in appsettings.json)
                Assembly.Load("FiftyOne.IpIntelligence.Engine.OnPremise");

                // Create the configuration object
                var config = new ConfigurationBuilder()
#if NETCORE
                    .AddJsonFile("appsettings.json")
#else
                    .AddXmlFile("App.config")
#endif
                    .Build();
                // Bind the configuration to a pipeline options instance
                PipelineOptions options = new PipelineOptions();
                config.Bind("PipelineOptions", options);

                // Create the pipeline using the options object
                using (var pipeline = new FiftyOnePipelineBuilder()
                    .BuildFromConfiguration(options))
                {
                    // First try an IPv4 address.
                    Console.WriteLine("Querying IPv4 Address");
                    Console.WriteLine("=====================");
                    AnalyseIpAddress(Ipv4Address, pipeline);
                    Console.WriteLine();
                    // Now try an IPv6 address.
                    Console.WriteLine("Querying IPv6 Address");
                    Console.WriteLine("=====================");
                    AnalyseIpAddress(Ipv6Address, pipeline);
                }
            }

            /// <summary>
            /// Process a single IP address string to retrieve the values associated
            /// with the IP address for the selected properties.
            /// </summary>
            /// <param name="ipAddress"></param>
            /// <param name="pipeline"></param>
            private void AnalyseIpAddress(string ipAddress, IPipeline pipeline)
            {
                // Create the FlowData instance.
                var data = pipeline.CreateFlowData();
                // Add an IP address as evidence.
                data.AddEvidence("query.client-ip-51d", ipAddress);
                // Process the supplied evidence.
                data.Process();
                // Get IP data from the flow data.
                var ip = data.Get<IIpData>();
                var rangeStart = ip.IpRangeStart;
                Console.WriteLine($"1. What is the RangeStart of '{ipAddress}'?");
                // Output the value of the 'RangeStart' property.
                if (rangeStart.HasValue)
                {
                    Console.WriteLine($"\t{rangeStart.Value.ToString()}");
                }
                else
                {
                    Console.WriteLine($"\t{rangeStart.NoValueMessage}");
                }

                // Obtain the list of country codes where the IP address is possibly from
                var countries = ip.CountryCode;
                Console.WriteLine($"2. What is the Country where the '{ipAddress}' is from?");
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

                // Obtain the network id of the IP address
                var networkId = ip.NetworkId;
                Console.WriteLine($"3. What is the NetworkId of the '{ipAddress}'?");
                if (networkId.HasValue)
                {
                    Console.WriteLine($"\t{networkId.Value}");
                }
                else
                {
                    Console.WriteLine($"\t{networkId.NoValueMessage}");
                }
            }
        }

        static void Main(string[] args)
        {
            new Example().Run();
#if (DEBUG)
            Console.WriteLine("Complete. Press key to exit.");
            Console.ReadKey();
#endif
        }
    }
}
