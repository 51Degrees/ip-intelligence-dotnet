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

using FiftyOne.IpIntelligence.Engine.OnPremise.Data;
using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.FlowElements
{
    /// <summary>
    /// Tests for https://github.com/51Degrees/ip-intelligence-dotnet/issues/319
    /// - the client IP echoed as the synthetic Ip / IpV6 properties must be
    /// selected by validity, not presence, and the parser must accept the
    /// shapes that arrive in production (a value carrying a port, a
    /// comma-separated forwarded list) - and for
    /// https://github.com/51Degrees/ip-intelligence-dotnet/issues/333 - the
    /// echoed address must be the one the native lookup used, so the echo
    /// must follow the native prefix and header priority across every
    /// accepted evidence key rather than a fixed pair of key names.
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

        /// <summary>
        /// Issue #333 - the report's repro. query.client-ip is unusable but
        /// another 'query.' key holds a valid address, and a valid
        /// server.client-ip is also present. The native engine makes a full
        /// pass over 'query.' keys before it looks at 'server.' keys, so it
        /// resolves the client-ip-51d value; the echo must name that same
        /// address, not fall straight through to server.client-ip.
        /// </summary>
        [TestMethod]
        public void Process_InvalidQueryClientIp_OtherQueryKeyBeatsServerClientIp()
        {
            const string queryAddress = "5.6.7.8";
            const string serverAddress = "85.118.2.126";

            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("query.client-ip", "not-an-ip");
                flowData.AddEvidence("query.client-ip-51d", queryAddress);
                flowData.AddEvidence("server.client-ip", serverAddress);
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.Ip.HasValue,
                    "Ip should have a value when valid evidence is supplied.");
                Assert.AreEqual(IPAddress.Parse(queryAddress), data.Ip.Value,
                    "A valid 'query.' key must beat server.client-ip even " +
                    "when query.client-ip itself is unusable - that is the " +
                    "order the native lookup uses.");
                AssertLookupDescribesEcho(data, queryAddress, serverAddress);
            }
        }

        /// <summary>
        /// Issue #333 - with several valid client IP keys on one request the
        /// echo must name whichever address the native lookup used, so the
        /// location properties beside it describe that address. This does
        /// not pin the winner: the header priority is defined by the data
        /// file, so the check is only that the echo and the lookup agree.
        /// </summary>
        [TestMethod]
        public void Process_SeveralValidClientIpKeys_EchoMatchesLookup()
        {
            var suppliedAddresses = new Dictionary<string, string>
            {
                { "query.client-ip", "1.2.3.4" },
                { "query.client-ip-51d", "5.6.7.8" },
                { "server.client-ip", "85.118.2.126" },
            };

            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                foreach (var supplied in suppliedAddresses)
                {
                    flowData.AddEvidence(supplied.Key, supplied.Value);
                }
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.Ip.HasValue,
                    "Ip should have a value when valid evidence is supplied.");
                CollectionAssert.Contains(
                    suppliedAddresses.Values.ToList(),
                    data.Ip.Value.ToString(),
                    "The echo must be one of the supplied addresses.");
                AssertLookupDescribesEcho(
                    data, suppliedAddresses.Values.ToArray());
            }
        }

        /// <summary>
        /// Issue #333 - a key whose prefix is not in the engine's own casing
        /// is admitted by the (case-insensitive) evidence key filter but the
        /// native prefix match is case-sensitive, so on its own such a value
        /// used to be echoed while the lookup silently ignored it. The value
        /// must now reach the lookup under the engine's spelling of the key,
        /// so the location beside the echo is that of the echoed address.
        /// </summary>
        [TestMethod]
        public void Process_UpperCasePrefix_LookupUsesTheEchoedAddress()
        {
            const string address = "5.6.7.8";

            using (var flowData = Wrapper.Pipeline.CreateFlowData())
            {
                flowData.AddEvidence("Query.Client-IP", address);
                flowData.Process();

                var data = flowData.Get<IIpIntelligenceData>();
                Assert.IsTrue(data.Ip.HasValue,
                    "A differently cased key is admitted by the evidence " +
                    "key filter and must still be echoed.");
                Assert.AreEqual(IPAddress.Parse(address), data.Ip.Value);
                AssertLookupDescribesEcho(data, address);
            }
        }

        /// <summary>
        /// Issue #333 - the echo walks the engine's evidence keys in the
        /// order the native engine consults them: a full 'query.' pass then
        /// a 'server.' pass, each in the header priority order the native
        /// engine listed the keys in. The native key list interleaves the
        /// prefixes per header, so it has to be regrouped.
        /// </summary>
        [TestMethod]
        public void OrderEvidenceKeysAsNativeEngine_GroupsByPrefixKeepingHeaderOrder()
        {
            var nativeKeyOrder = new List<string>
            {
                "query.true-client-ip-51d",
                "server.true-client-ip-51d",
                "query.client-ip-51d",
                "server.client-ip-51d",
                "query.client-ip",
                "server.client-ip",
            };

            var ordered = IpiOnPremiseEngine
                .OrderEvidenceKeysAsNativeEngine(nativeKeyOrder);

            CollectionAssert.AreEqual(
                new[]
                {
                    "query.true-client-ip-51d",
                    "query.client-ip-51d",
                    "query.client-ip",
                    "server.true-client-ip-51d",
                    "server.client-ip-51d",
                    "server.client-ip",
                },
                ordered,
                "Every 'query.' key must come before every 'server.' key, " +
                "and each group must keep the native header order.");
        }

        [TestMethod]
        public void OrderEvidenceKeysAsNativeEngine_DropsKeysTheNativeEngineIgnores()
        {
            var ordered = IpiOnPremiseEngine.OrderEvidenceKeysAsNativeEngine(
                new List<string>
                {
                    "cookie.client-ip",
                    "server.client-ip",
                    "header.client-ip",
                    "query.client-ip",
                });

            CollectionAssert.AreEqual(
                new[] { "query.client-ip", "server.client-ip" },
                ordered,
                "Only 'query.' and 'server.' keys are read by the native " +
                "lookup, so only those may reach it or drive the echo.");
        }

        [TestMethod]
        public void OrderEvidenceKeysAsNativeEngine_EmptyKeys_EmptyResult()
        {
            var ordered = IpiOnPremiseEngine
                .OrderEvidenceKeysAsNativeEngine(new List<string>());

            Assert.AreEqual(0, ordered.Length);
        }

        /// <summary>
        /// Assert that the lookup result beside the echo describes the
        /// echoed address: the echo must lie inside the IP range the lookup
        /// resolved. The other supplied addresses must lie outside that
        /// range for the check to have teeth; if the current data file puts
        /// two of them in one range the test cannot tell which was looked
        /// up, which is a property of the data rather than a defect.
        /// </summary>
        private static void AssertLookupDescribesEcho(
            IIpIntelligenceData data,
            params string[] suppliedAddresses)
        {
            Assert.IsTrue(data.Ip.HasValue,
                "The echo must have a value for its lookup to be checked.");
            Assert.IsTrue(data.IpRangeStart.HasValue && data.IpRangeEnd.HasValue,
                "The lookup must resolve an IP range for the echo to be " +
                "checked against.");
            var echoedAddress = data.Ip.Value;
            var rangeStart = data.IpRangeStart.Value;
            var rangeEnd = data.IpRangeEnd.Value;

            Assert.IsTrue(IsWithinRange(echoedAddress, rangeStart, rangeEnd),
                $"Ip echoes {echoedAddress} but the lookup beside it " +
                $"covers {rangeStart} - {rangeEnd}: the echo and the " +
                "native lookup used different addresses.");

            var otherAddressesInRange = suppliedAddresses
                .Select(IPAddress.Parse)
                .Where(supplied => supplied.Equals(echoedAddress) == false)
                .Where(supplied => IsWithinRange(supplied, rangeStart, rangeEnd))
                .ToList();
            if (otherAddressesInRange.Count > 0)
            {
                Assert.Inconclusive(
                    $"The range {rangeStart} - {rangeEnd} the lookup " +
                    $"resolved also covers {string.Join(", ", otherAddressesInRange)}, " +
                    "so this data file cannot tell which supplied address " +
                    "was looked up. Choose test addresses in different ranges.");
            }
        }

        private static bool IsWithinRange(
            IPAddress address,
            IPAddress rangeStart,
            IPAddress rangeEnd)
        {
            return address.AddressFamily == rangeStart.AddressFamily &&
                address.AddressFamily == rangeEnd.AddressFamily &&
                CompareAddressBytes(address, rangeStart) >= 0 &&
                CompareAddressBytes(address, rangeEnd) <= 0;
        }

        private static int CompareAddressBytes(IPAddress left, IPAddress right)
        {
            var leftBytes = left.GetAddressBytes();
            var rightBytes = right.GetAddressBytes();
            for (var index = 0; index < leftBytes.Length; index++)
            {
                var comparison = leftBytes[index].CompareTo(rightBytes[index]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }
            return 0;
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

                // Issue #332 - NoValue here is "supplied and rejected", not
                // "never supplied".
                Assert.AreEqual(
                    IpDataOnPremise.InvalidIpEvidenceMessage,
                    data.Ip.NoValueMessage,
                    "Exhausting every source must report the supplied IP " +
                    "as invalid, not as absent.");
            }
        }
    }
}
