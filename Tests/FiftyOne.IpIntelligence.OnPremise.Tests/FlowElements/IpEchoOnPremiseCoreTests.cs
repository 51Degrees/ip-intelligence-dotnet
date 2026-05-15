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

using FiftyOne.IpIntelligence.TestHelpers;
using FiftyOne.Pipeline.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;
using FiftyOne.IpIntelligence;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.FlowElements
{
    [TestClass]
    [TestCategory("IpIntelligence")]
    [TestCategory("OnPremise")]
    [TestCategory("IpEcho")]
    public class IpEchoOnPremiseCoreTests : TestsBase
    {
        [TestInitialize]
        public new void Init()
        {
            base.Init();
            TestInitialize(PerformanceProfiles.LowMemory);
        }

        [TestMethod]
        public void Properties_IncludeSyntheticIpAndIpV6()
        {
            var names = Wrapper.Engine.Properties
                .Select(p => p.Name)
                .ToList();

            Assert.IsTrue(names.Contains("Ip"),
                "IpiOnPremiseEngine.Properties must include synthetic 'Ip' property.");
            Assert.IsTrue(names.Contains("IpV6"),
                "IpiOnPremiseEngine.Properties must include synthetic 'IpV6' property.");
        }

        [TestMethod]
        public void Properties_SyntheticIpHasNetworkComponentAndIPAddressType()
        {
            var ip = Wrapper.Engine.Properties.Single(p => p.Name == "Ip");
            var ipv6 = Wrapper.Engine.Properties.Single(p => p.Name == "IpV6");

            Assert.AreEqual("Network", ip.Component.Name,
                "Synthetic Ip property must have Component.Name 'Network' to match common-metadata.");
            Assert.AreEqual("Network", ipv6.Component.Name,
                "Synthetic IpV6 property must have Component.Name 'Network'.");

            Assert.AreEqual(typeof(IPAddress), ip.Type,
                "Synthetic Ip property must be IPAddress-typed.");
            Assert.AreEqual(typeof(IPAddress), ipv6.Type,
                "Synthetic IpV6 property must be IPAddress-typed.");
        }

        [TestMethod]
        public void Process_IPv4Evidence_PopulatesIpAndLeavesIpV6NoValue()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("server.client-ip", "1.2.3.4");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.Ip.HasValue, "Ip should have a value for IPv4 evidence.");
                Assert.AreEqual(IPAddress.Parse("1.2.3.4"), data.Ip.Value);
                Assert.IsFalse(data.IpV6.HasValue, "IpV6 should have no value for IPv4 evidence.");
            }
        }

        [TestMethod]
        public void Process_IPv6Evidence_PopulatesIpV6AndLeavesIpNoValue()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("server.client-ip", "2001:db8::1");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.IpV6.HasValue, "IpV6 should have a value for IPv6 evidence.");
                Assert.AreEqual(IPAddress.Parse("2001:db8::1"), data.IpV6.Value);
                Assert.IsFalse(data.Ip.HasValue, "Ip should have no value for IPv6 evidence.");
            }
        }

        [TestMethod]
        public void Process_NoIpEvidence_BothNoValue()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                // intentionally no IP evidence
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsFalse(data.Ip.HasValue, "Ip should be NoValue when no IP evidence is present.");
                Assert.IsFalse(data.IpV6.HasValue, "IpV6 should be NoValue when no IP evidence is present.");
                Assert.IsFalse(string.IsNullOrEmpty(data.Ip.NoValueMessage),
                    "NoValueMessage should be populated when no IP evidence is supplied.");
            }
        }
    }
}
