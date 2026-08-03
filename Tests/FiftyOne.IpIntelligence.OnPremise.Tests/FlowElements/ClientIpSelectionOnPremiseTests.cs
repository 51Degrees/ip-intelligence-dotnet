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

using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.FlowElements
{
    /// <summary>
    /// Tests for https://github.com/51Degrees/ip-intelligence-dotnet/issues/319
    /// - the client IP echoed as the synthetic Ip / IpV6 properties must be
    /// selected by validity, not presence, and the parser must accept the
    /// shapes that arrive in production (a value carrying a port, a
    /// comma-separated forwarded list).
    /// </summary>
    [TestClass]
    [TestCategory("IpIntelligence")]
    [TestCategory("OnPremise")]
    [TestCategory("IpEcho")]
    public class ClientIpSelectionOnPremiseTests : TestsBase
    {
        [TestInitialize]
        public new void Init()
        {
            base.Init();
            TestInitialize(PerformanceProfiles.LowMemory);
        }

        [TestMethod]
        public void Process_InvalidQueryClientIp_FallsBackToServerClientIp()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                // The value from the original report - an IPv4 shape with an
                // out-of-range octet, so it fails to parse.
                flowData.AddEvidence("query.client-ip", "82.12.343.23");
                flowData.AddEvidence("server.client-ip", "1.2.3.4");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.Ip.HasValue,
                    "A malformed query.client-ip must not suppress the echo " +
                    "when a valid server.client-ip is on the same request.");
                Assert.AreEqual(IPAddress.Parse("1.2.3.4"), data.Ip.Value);
            }
        }

        [TestMethod]
        public void Process_ValidQueryAndServerClientIp_QueryTakesPriority()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("query.client-ip", "5.6.7.8");
                flowData.AddEvidence("server.client-ip", "1.2.3.4");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.Ip.HasValue,
                    "Ip should have a value when valid evidence is supplied.");
                Assert.AreEqual(IPAddress.Parse("5.6.7.8"), data.Ip.Value,
                    "query.client-ip must keep priority over " +
                    "server.client-ip when both are valid.");
            }
        }

        [TestMethod]
        public void Process_ClientIpWithPort_IsAccepted()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                // The form a connection-derived value takes in production
                // telemetry: address plus ephemeral port.
                flowData.AddEvidence("server.client-ip", "82.132.237.238:34947");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.Ip.HasValue,
                    "A client IP carrying a port must be accepted.");
                Assert.AreEqual(IPAddress.Parse("82.132.237.238"), data.Ip.Value);
            }
        }

        [TestMethod]
        public void Process_ForwardedList_FirstEntryUsed()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("server.client-ip", "1.2.3.4, 5.6.7.8");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.Ip.HasValue,
                    "A comma-separated forwarded list must be accepted.");
                Assert.AreEqual(IPAddress.Parse("1.2.3.4"), data.Ip.Value,
                    "The first entry of a forwarded list is the client.");
            }
        }

        [TestMethod]
        public void Process_BracketedIpV6WithPort_IsAccepted()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("server.client-ip", "[2001:db8::1]:443");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.IpV6.HasValue,
                    "A bracketed IPv6 address carrying a port must be accepted.");
                Assert.AreEqual(IPAddress.Parse("2001:db8::1"), data.IpV6.Value);
                Assert.IsFalse(data.Ip.HasValue,
                    "Ip should have no value for IPv6 evidence.");
            }
        }

        [TestMethod]
        public void Process_InvalidQueryClientIp_NativeLookupStillRuns()
        {
            // The synthetic Ip / IpV6 echo is computed entirely managed-side,
            // so the other tests here cannot see whether the NATIVE lookup
            // survived a malformed query.client-ip. Natively, an unparseable
            // query-prefixed value raises INCORRECT_IP_ADDRESS_FORMAT and
            // aborts before the server-prefixed evidence is tried; the
            // evidence sanitization in ProcessEngine exists to prevent that.
            // Process the same valid server IP with and without the
            // malformed query value: the populated properties must match.
            var controlKeys = GetPopulatedKeys(withInvalidQueryIp: false);
            var affectedKeys = GetPopulatedKeys(withInvalidQueryIp: true);

            Assert.IsTrue(controlKeys.Count > 1,
                "The control run should populate properties beyond the " +
                "echoed 'ip' - without that this test cannot detect an " +
                "aborted native lookup.");
            CollectionAssert.AreEquivalent(controlKeys, affectedKeys,
                "A malformed query.client-ip must not change which " +
                "properties the native lookup populates from the valid " +
                "server.client-ip.");
        }

        private List<string> GetPopulatedKeys(bool withInvalidQueryIp)
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                if (withInvalidQueryIp)
                {
                    flowData.AddEvidence("query.client-ip", "82.12.343.23");
                }
                flowData.AddEvidence("server.client-ip", "8.8.8.8");
                flowData.Process();

                var populatedKeys = new List<string>();
                foreach (var entry in
                    flowData.Get<IIpIntelligenceData>().AsDictionary())
                {
                    var propertyValue = entry.Value as IAspectPropertyValue;
                    if (propertyValue == null || propertyValue.HasValue)
                    {
                        populatedKeys.Add(entry.Key);
                    }
                }
                return populatedKeys;
            }
        }

        [TestMethod]
        public void Process_EverySourceInvalid_BothNoValueAndNoThrow()
        {
            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("query.client-ip", "82.12.343.23");
                flowData.AddEvidence("server.client-ip", "not-an-ip");
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsFalse(data.Ip.HasValue,
                    "Ip should be NoValue when every source is invalid.");
                Assert.IsFalse(data.IpV6.HasValue,
                    "IpV6 should be NoValue when every source is invalid.");
            }
        }
    }
}
