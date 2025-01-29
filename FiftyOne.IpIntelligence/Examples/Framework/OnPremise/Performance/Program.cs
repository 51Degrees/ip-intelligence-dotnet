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
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// @example OnPremise/Performance/Program.cs
///
/// @include{doc} example-performance-ipi.txt
/// 
/// This example is available in full on [GitHub](https://github.com/51Degrees/ip-intelligence-dotnet/blob/master/FiftyOne.IpIntelligence/Examples/Framework/OnPremise/Performance/Program.cs).
/// 
/// @include{doc} example-require-datafile-ipi.txt
/// 
/// Required NuGet Dependencies:
/// - FiftyOne.IpIntelligence
/// 
/// Expected output:
/// ```
/// ...
/// This example will look up source country values for multiple IP addresses
/// Processing 10000 IP addresses
/// The 10000 process calls will use a maximum of 1000 distinct IP addresses
/// ========================================
/// Average 0.05569072 ms per IP address
/// IP addresses where the result includes more than one country: 139
/// IP addresses where the result is a single country: 3852
/// IP addresses where the country is unknown or unspecified (country code 'ZZ'): 6009
/// IP addresses that had no result: 0
/// ...
/// ```
/// </summary>
namespace FiftyOne.IpIntelligence.Examples.OnPremise.Performance
{
    public class Program
    {
        public class Example : ExampleBase
        {
            public void Run(string dataFile, string ipFile, int count)
            {
                FileInfo f = new FileInfo(dataFile);
                Console.WriteLine($"Using data file at '{f.FullName}'");
                // Build a pipeline containing an on-premise IP Intelligence engine 
                // using the IP Intelligence Pipeline Builder.
                using (var pipeline = new IpiPipelineBuilder()
                    .UseOnPremise(dataFile, null, false)
                    .SetDataFileSystemWatcher(false)
                    .SetShareUsage(false)
                    // Prefer maximum performance profile where all data loaded 
                    // into memory. Experiment with other profiles.
                    .SetPerformanceProfile(PerformanceProfiles.MaxPerformance)
                    //.SetPerformanceProfile(PerformanceProfiles.LowMemory)
                    //.SetPerformanceProfile(PerformanceProfiles.Balanced)
                    //.UseResultsCache()
                    .Build())
                {
                    Run(ipFile, count, pipeline);
                }
            }

            private static void Run(
                string ipFile,
                int count,
                IPipeline pipeline)
            {
                var countryMany = 0;
                var countryOne = 0;
                var countryUnknown = 0;
                var countryNoResult = 0;
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                int maxDistinctIPs = 1000;

                var starts = DateTime.UtcNow;

                try
                {
                    Console.WriteLine($"This example will look up the source country for multiple IP addresses");
                    Console.WriteLine($"Processing {count} IP addresses from {ipFile}");
                    Console.WriteLine($"The {count} process calls will use a " +
                        $"maximum of {maxDistinctIPs} distinct IP addresses");
                    // Start multiple threads to process a set of IP addresses, making a note of
                    // the time at which processing was started.
                    Parallel.ForEach(Report(GetIpAddresses(ipFile, count).ToList(), count, maxDistinctIPs, 40),
                        new ParallelOptions()
                        {
                            //MaxDegreeOfParallelism = 1,
                            CancellationToken = cancellationTokenSource.Token
                        },
                        ipAddress =>
                        {
                            // Create a new flow data to add evidence to and get 
                            // IP data back again.
                            var data = pipeline.CreateFlowData();
                            // Add the IP address as evidence to the flow data.
                            data.AddEvidence("query.client-ip-51d", ipAddress)
                                        .Process();

                            // Get the IP data from the engine.
                            var ip = data.Get<IIpData>();

                            // Update the counters depending on the CountryCode
                            // result.
                            var countries = ip.NetworkName; // TODO: Use CountryCode again
                            if (countries.HasValue)
                            {
                                if (countries.Value.Count > 1)
                                {
                                    Interlocked.Increment(ref countryMany);
                                }
                                else if (countries.Value.Any(c => c.Value.Equals("ZZ", StringComparison.OrdinalIgnoreCase)))
                                {
                                    Interlocked.Increment(ref countryUnknown);
                                }
                                else 
                                { 
                                    Interlocked.Increment(ref countryOne);
                                }
                            }
                            else
                            {
                                Interlocked.Increment(ref countryNoResult);
                            }

                        });
                    // Wait for all processing to finish, and make a note of the time elapsed
                    // since the processing was started.
                    var time = DateTime.UtcNow - starts;
                    // Output the average time to process a single IP address.
                    Console.WriteLine($"Average {(double)time.TotalMilliseconds / (double)count} ms per IP address");
                    Console.WriteLine($"IP addresses where the result includes more than one country: {countryMany}");
                    Console.WriteLine($"IP addresses where the result is a single country: {countryOne}");
                    Console.WriteLine($"IP addresses where the country is unknown or unspecified (country code 'ZZ'): {countryUnknown}");
                    Console.WriteLine($"IP addresses that had no result: {countryNoResult}");
                }
                catch (OperationCanceledException) { }
            }
        }

        static void Main(string[] args)
        {
#if NETCORE
            var defaultDataFile = "..\\..\\..\\..\\..\\..\\..\\..\\ip-intelligence-cxx\\ip-intelligence-data\\51Degrees-LiteV4.1.ipi";
            var defaultIpFile = "..\\..\\..\\..\\..\\..\\..\\..\\ip-intelligence-cxx\\ip-intelligence-data\\evidence.yml";
#else
            var defaultDataFile = "..\\..\\..\\..\\..\\..\\..\\ip-intelligence-cxx\\ip-intelligence-data\\51Degrees-LiteV4.1.ipi";
            var defaultIpFile = "..\\..\\..\\..\\..\\..\\..\\ip-intelligence-cxx\\ip-intelligence-data\\evidence.yml";
#endif
            var dataFile = args.Length > 0 ? args[0] : defaultDataFile;
            var ipFile = args.Length > 1 ? args[1] : defaultIpFile;
            new Example().Run(dataFile, ipFile, 10000);
#if (DEBUG)
            Console.WriteLine("Complete. Press key to exit.");
            Console.ReadKey();
#endif
        }
    }
}
