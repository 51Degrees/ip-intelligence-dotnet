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
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// @example OnPremise/AutomaticUpdates/UpdateOnStartUp/Program.cs
/// 
/// @include{doc} example-automatic-updates-on-startup-ipi.txt
/// 
/// This example is available in full on [GitHub](https://github.com/51Degrees/ip-intelligence-dotnet/blob/master/FiftyOne.IpIntelligence/Examples/Framework/OnPremise/AutomaticUpdates/UpdateOnStartUp/Program.cs). 
/// 
/// @include{doc} example-require-licensekey-ipi.txt
/// @include{doc} example-require-datafile-ipi.txt
/// 
/// Required NuGet Dependencies:
/// - FiftyOne.IpIntelligence
/// 
/// Expected output:
/// ```
/// Using data file at 'IpIntelligence-LiteV4.1.ipi'
/// Data file published date: 13/04/2020 00:00:00
/// Creating pipeline and updating IP data.....
/// Data file published date: 02/07/2020 00:00:00
/// ```
/// </summary>
namespace FiftyOne.IpIntelligence.Examples.OnPremise.AutomaticUpdates.UpdateOnStartUp
{
    public class Program
    {
        public class Example : ExampleBase
        {
            public void Run(string dataFile, string licenseKey)
            {
                FileInfo f = new FileInfo(dataFile);
                Console.WriteLine($"Using data file at '{f.FullName}'");

                DateTime initialPublishedDate = DateTime.MinValue;

                // Create a temporary IP intelligence 'OnPremise' Engine to check the initial published date of the data file.
                // There is no need to do this in production, it is for demonstration purposes only.
                // This also higlights the added simplicity of the IP intelligence Pipeline builder.
                using (var loggerFactory = new LoggerFactory())
                {
                    var dataUpdateService = new DataUpdateService(loggerFactory.CreateLogger<DataUpdateService>(), new HttpClient());
                    using (var temporaryIpiEngine = new IpiOnPremiseEngineBuilder(loggerFactory, dataUpdateService)
                        .Build(dataFile, false))
                    {
                        // Get the published date of the IP data file. Engines can have multiple data files but 
                        // for the IP intelligence 'OnPremise' engine we can guarantee there will only be one.
                        initialPublishedDate = temporaryIpiEngine.DataFiles.Single().DataPublishedDateTime;
                    }
                }

                Console.WriteLine($"Data file published date: {initialPublishedDate}");
                Console.Write($"Creating pipeline and updating IP data");
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                Task.Run(() => { OutputUntilCancelled(".", 1000, cancellationSource.Token); });

                // Build a new Pipeline to use an on-premise IP intelligence engine and 
                // configure automatic updates.
                var pipeline = new IpiPipelineBuilder()
                    // Use the On-Premise engine (aka IP intelligence engine) and pass in
                    // the path to the data file, your license key and whether 
                    // to use a temporary file.
                    // A license key is required otherwise automatic updates will
                    // remain disabled.
                    .UseOnPremise(dataFile, licenseKey, true)
                    // Enable automatic updates.
                    .SetAutoUpdate(true)
                    // Watch the data file on disk and refresh the engine 
                    // as soon as that file is updated. 
                    .SetDataFileSystemWatcher(true)
                    // Enable update on startup, the auto update system 
                    // will be used to check for an update before the
                    // IP intelligence engine is created. This will block 
                    // creation of the pipeline.
                    .SetDataUpdateOnStartUp(true)
                    .Build();

                cancellationSource.Cancel();
                Console.WriteLine();

                // Get the published date of the data file from the OnPremise engine 
                // after building the pipeline.
                var updatedPublishedDate = pipeline
                    .GetElement<IpiOnPremiseEngine>()
                    .DataFiles
                    .Single()
                    .DataPublishedDateTime;

                if (DateTime.Equals(initialPublishedDate, updatedPublishedDate))
                {
                    Console.WriteLine("There was no update available at this time.");
                }
                Console.WriteLine($"Data file published date: {updatedPublishedDate}");

            }
        }

        private static void OutputUntilCancelled(string text, int intervalMs, CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                Console.Write(text);
                Task.Delay(intervalMs).Wait();
            }
        }

        static void Main(string[] args)
        {
            var licenseKey = "!!Your license key!!";

            if (licenseKey.StartsWith("!!"))
            {
                Console.WriteLine("You need a license key to run this example, " +
                    "you can obtain one by subscribing to a 51Degrees bundle: https://51degrees.com/pricing");
                Console.ReadKey();
                return;
            }

#if NETCORE
            var defaultDataFile = "..\\..\\..\\..\\..\\..\\..\\..\\..\\ip-intelligence-cxx\\ip-intelligence-data\\IpIntelligence-LiteV4.1.ipi";
#else
            var defaultDataFile = "..\\..\\..\\..\\..\\..\\..\\..\\ip-intelligence-cxx\\ip-intelligence-data\\IpIntelligence-LiteV4.1.ipi";
#endif
            var dataFile = args.Length > 0 ? args[0] : defaultDataFile;
            new Example().Run(dataFile, licenseKey);
#if (DEBUG)
            Console.WriteLine("Complete. Press key to exit.");
            Console.ReadKey();
#endif
        }
    }
}
