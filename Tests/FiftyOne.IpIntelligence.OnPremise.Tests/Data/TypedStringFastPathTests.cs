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

using FiftyOne.Common.TestHelpers;
using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Constants = FiftyOne.IpIntelligence.TestHelpers.Constants;
using Utils = FiftyOne.IpIntelligence.TestHelpers.Utils;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Data
{
    [TestClass]
    [TestCategory("IpIntelligence")]
    [TestCategory("OnPremise")]
    public class TypedStringFastPathTests : TestsBase
    {
        [TestMethod]
        public void TypedStringProperty_PrefersStoredOverride()
        {
            TestInitialize(PerformanceProfiles.LowMemory);
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("server.client-ip", "1.2.3.4");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                var expected = new AspectPropertyValue<string>("Override");
                data.PopulateFrom(new[]
                {
                    new KeyValuePair<string, object>("CountryCode", expected)
                });

                Assert.AreSame(expected, data.CountryCode);
            }
        }

        /// <summary>
        /// The order in which required properties are requested must not
        /// change the values returned.
        /// </summary>
        /// <remarks>
        /// A required property index addresses the native
        /// PropertiesAvailable array, which properties.c sorts into
        /// case insensitive ascending name order
        /// (initRequiredProperties calls sortRequiredProperties). Any
        /// managed cache of those indexes that is built from the order the
        /// caller asked for, rather than from the sorted native order,
        /// silently returns another property's value whenever the two
        /// orders differ.
        ///
        /// Both engines here request the same two properties and differ
        /// only in the order of the list, so every value must agree. The
        /// reversed engine also has to disagree with a naive positional
        /// mapping, which is what makes this test able to fail.
        /// </remarks>
        [TestMethod]
        public void TypedStringProperty_IsIndependentOfRequestedPropertyOrder()
        {
            // Deliberately not alphabetical, so a cache built from this
            // order maps RegisteredName to 0 and CountryCode to 1 while the
            // native sorted order is the opposite.
            var ascending = new List<string> { "CountryCode", "RegisteredName" };
            var descending = new List<string> { "RegisteredName", "CountryCode" };

            var addresses = ReadTestAddresses(50);
            Assert.AreNotEqual(0, addresses.Count, "No test IP addresses were available.");

            var ascendingValues = Evaluate(ascending, addresses);
            var descendingValues = Evaluate(descending, addresses);

            var countryCodesFound = ascendingValues.Count(v => v.CountryCode != null);
            var registeredNamesFound = ascendingValues.Count(v => v.RegisteredName != null);
            Assert.AreNotEqual(0, countryCodesFound,
                "No CountryCode values were returned, so the comparison would be vacuous.");
            Assert.AreNotEqual(0, registeredNamesFound,
                "No RegisteredName values were returned, so the comparison would be vacuous.");

            for (var i = 0; i < addresses.Count; i++)
            {
                Assert.AreEqual(
                    ascendingValues[i].CountryCode,
                    descendingValues[i].CountryCode,
                    $"CountryCode for '{addresses[i]}' changed when the requested " +
                    $"property order was reversed.");
                Assert.AreEqual(
                    ascendingValues[i].RegisteredName,
                    descendingValues[i].RegisteredName,
                    $"RegisteredName for '{addresses[i]}' changed when the requested " +
                    $"property order was reversed.");
            }
        }

        /// <summary>
        /// A typed string accessor must agree with the same property read
        /// through the generic dictionary, for every configured property
        /// and for the no value case.
        /// </summary>
        [TestMethod]
        public void TypedStringProperty_MatchesGenericAccessor()
        {
            var properties = new List<string> { "RegisteredName", "CountryCode" };
            var addresses = ReadTestAddresses(25);

            using (var pipeline = BuildPipeline(properties))
            {
                foreach (var address in addresses)
                {
                    using (var flowData = pipeline.CreateFlowData())
                    {
                        flowData.AddEvidence("server.client-ip", address);
                        flowData.Process();
                        var data = flowData.Get<IIpIntelligenceData>();

                        foreach (var name in properties)
                        {
                            var generic = (IAspectPropertyValue<string>)data[name];
                            var typed = string.Equals(name, "CountryCode", StringComparison.Ordinal)
                                ? data.CountryCode
                                : data.RegisteredName;

                            Assert.AreEqual(generic.HasValue, typed.HasValue,
                                $"HasValue disagreed for '{name}' and '{address}'.");
                            if (generic.HasValue)
                            {
                                Assert.AreEqual(generic.Value, typed.Value,
                                    $"Value disagreed for '{name}' and '{address}'.");
                            }
                            else
                            {
                                Assert.AreEqual(generic.NoValueMessage, typed.NoValueMessage,
                                    $"NoValueMessage disagreed for '{name}' and '{address}'.");
                            }
                        }
                    }
                }
            }
        }

        private sealed class Values
        {
            public string CountryCode { get; set; }
            public string RegisteredName { get; set; }
        }

        /// <summary>
        /// Reads client IP addresses out of the test evidence file. The
        /// shared IpAddressGenerator returns raw lines, including the YAML
        /// document separators and the evidence key, none of which parse as
        /// an address, so the values are taken directly here.
        /// </summary>
        private static List<string> ReadTestAddresses(int count)
        {
            const string key = "server.client-ip:";
            return System.IO.File.ReadLines(
                Utils.GetFilePath(Constants.IP_FILE_NAME).FullName)
                .Where(line => line.StartsWith(key, StringComparison.Ordinal))
                .Select(line => line.Substring(key.Length).Trim())
                .Where(value => value.Length > 0)
                .Take(count)
                .ToList();
        }

        private static IPipeline BuildPipeline(IList<string> properties)
        {
            var loggerFactory = new TestLoggerFactory();
            var engine = new IpiOnPremiseEngineBuilder(loggerFactory, null)
                .SetPerformanceProfile(PerformanceProfiles.LowMemory)
                .SetAutoUpdate(false)
                .SetDataFileSystemWatcher(false)
                .SetProperties(new List<string>(properties))
                .Build(Utils.GetFilePath(Constants.IPI_DATA_FILE_NAME).FullName, false);
            return new PipelineBuilder(loggerFactory)
                .AddFlowElement(engine)
                .Build();
        }

        private static List<Values> Evaluate(
            IList<string> properties,
            IList<string> addresses)
        {
            var results = new List<Values>(addresses.Count);
            using (var pipeline = BuildPipeline(properties))
            {
                foreach (var address in addresses)
                {
                    using (var flowData = pipeline.CreateFlowData())
                    {
                        flowData.AddEvidence("server.client-ip", address);
                        flowData.Process();
                        var data = flowData.Get<IIpIntelligenceData>();
                        results.Add(new Values
                        {
                            CountryCode = data.CountryCode.HasValue
                                ? data.CountryCode.Value : null,
                            RegisteredName = data.RegisteredName.HasValue
                                ? data.RegisteredName.Value : null,
                        });
                    }
                }
            }
            return results;
        }
    }
}
