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

using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.Pipeline.Engines;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// @example OnPremise/Metadata/Program.cs
///
/// @include{doc} example-metadata-ipi.txt
/// 
/// This example is available in full on [GitHub](https://github.com/51Degrees/ip-intelligence-dotnet/blob/master/FiftyOne.IpIntelligence/Examples/Framework/OnPremise/Metadata/Program.cs). 
/// 
/// @include{doc} example-require-datafile-ipi.txt
/// 
/// Required NuGet Dependencies:
/// - FiftyOne.IpIntelligence
/// 
/// Expected output:
/// ```
/// ...
/// ConnectivityType[Category: ](String) - The connectivity type
/// Possible values: Mobile,Network HSPAP, Network LTE,... + 6 more...
/// MCC[Category: ] (String) - Mobile country code.Only valid if type = mobile
/// Possible values: 000,001,200,201,... + 360 more...
/// MNC[Category: ] (String) - Mobile network code.Only valid if type = mobile
/// Possible values: 00,000,001,002,... + 628 more...
/// IspName[Category: ] (String) - The name of the network operator
/// Possible values: *bliep,012 Telecom,018 Xphone,... + 1620 more...
/// IspCountryCode[Category: ] (String) - The 2 character country code associated with this operator record
/// Possible values: 000000,202001,202005,202010... + 862 more...
/// IpRangeStart[Category: ](IPAddress) - The first IP address in the range
/// Possible values: 0.0.0.0,1.0.0.0,1.0.64.0,... + 161882 more...
/// IpRangeEnd[Category: ](IPAddress) - The last IP address in the range
/// Possible values: 0.255.255.255,1.0.63.255,... + 161882 more...
/// ...
/// ```
/// </summary>
namespace FiftyOne.IpIntelligence.Examples.OnPremise.Metadata
{
    public class Program
    {
        public class Example : ExampleBase
        {
            // Truncate value if it contains newline
            private string TruncateToNl(string s)
            {
                var lines = s.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var result = lines.FirstOrDefault();
                if (lines.Length > 1)
                {
                    result += " ...";
                }
                return result;
            }

            public void Run(string dataFile)
            {
                FileInfo f = new FileInfo(dataFile);
                Console.WriteLine($"Using data file at '{f.FullName}'");
                Console.WriteLine($"This example will use the meta-data exposed " +
                    $"by the IP intelligence engine to display the names and " +
                    $"details of all the properties it can populate.");
                Console.WriteLine($"Press any key to continue.");
                Console.ReadKey();

                // Build a new on-premise IP intelligence engine with the low memory performance profile.
                using (var ipiEngine = new IpiOnPremiseEngineBuilder(
                    new LoggerFactory())
                    .SetAutoUpdate(false)
                    .SetDataFileSystemWatcher(false)
                    .SetDataUpdateOnStartup(false)
                    // Prefer low memory profile where all data streamed 
                    // from disk on-demand. Experiment with other profiles.
                    //.SetPerformanceProfile(PerformanceProfiles.HighPerformance)
                    .SetPerformanceProfile(PerformanceProfiles.LowMemory)
                    //.SetPerformanceProfile(PerformanceProfiles.Balanced)

                    // Finally build the engine.
                    .Build(dataFile, false))
                {
                    // Iterate over all properties in the data file, printing the name, value type,
                    // and description for each one.
                    foreach (var property in ipiEngine.Properties)
                    {
                        // Output some details about the property.
                        // Add some formatting to highlight property names.
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.Write(property.Name);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine($"[Category: {property.Category}]" +
                            $"({property.Type.Name}) - {property.Description}");

                        // TODO: Handle possible values
                        continue;

                        // Next, output a list of the possible values this 
                        // property can have.
                        // Most properties in the IP Metrics category do
                        // not have defined values so exclude them.
                        if (property.Category != "IP Metrics")
                        {
                            StringBuilder values = new StringBuilder("Possible values: ");
                            foreach (var value in property.Values.Take(20))
                            {
                                // add value
                                values.Append(TruncateToNl(value.Name));
                                // add description if exists
                                if (string.IsNullOrEmpty(value.Description) == false)
                                {
                                    values.Append($"({value.Description})");
                                }
                                values.Append(",");
                            }
                            if (property.Values.Count() > 20)
                            {
                                values.Append($" + {property.Values.Count() - 20} more ...");
                            }
                            Console.WriteLine(values);
                        }
                    }
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