/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2025 51 Degrees Mobile Experts Limited, Davidson House,
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

using FiftyOne.IpIntelligence.Shared.FlowElements;
using FiftyOne.IpIntelligence.Shared.Services;
using FiftyOne.IpIntelligence.TestHelpers.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.IpIntelligence.TestHelpers.FlowElements
{
    public class ProcessTests
    {
        public static void NoEvidence(IWrapper wrapper, IDataValidator validator)
        {
            var data = wrapper.Pipeline.CreateFlowData();
            data.Process();
            validator.ValidateData(data, false);
        }

        public static void EmptyIpAddress(IWrapper wrapper, IDataValidator validator)
        {
            var data = wrapper.Pipeline.CreateFlowData();
            data.AddEvidence("query.client-ip-51d", "")
                .Process();
            validator.ValidateData(data, false);
        }

        public static void NoHeaders(IWrapper wrapper, IDataValidator validator)
        {
            var data = wrapper.Pipeline.CreateFlowData();
            data.AddEvidence("irrelevant.evidence", "some evidence")
                .Process();
            validator.ValidateData(data, false);
        }

        public static void NoUsefulHeaders(IWrapper wrapper, IDataValidator validator)
        {
            var data = wrapper.Pipeline.CreateFlowData();
            data.AddEvidence("query.irrelevant-header", "some evidence")
                .Process();
            validator.ValidateData(data, false);
        }

        public static void CaseInsensitiveEvidenceKeys(IWrapper wrapper, IDataValidator validator)
        {
            var data = wrapper.Pipeline.CreateFlowData();
            data.AddEvidence("query.CLIENT-IP-51D", Constants.Ipv4Address)
                .Process();
            validator.ValidateData(data);
        }

        public static void MetaDataService_DefaultProfilesIds(IWrapper wrapper)
        {
            var service = new MetaDataService(
                wrapper.Pipeline.FlowElements
                    .Where(e => typeof(IOnPremiseIpiEngine).IsAssignableFrom(e.GetType()))
                    .Cast<IOnPremiseIpiEngine>()
                    .ToArray());
            var defaultProfiles = service.DefaultProfilesIds();
            Assert.AreEqual(14, defaultProfiles.Count);
            Assert.IsTrue(defaultProfiles.ContainsKey(1));
            Assert.IsTrue(defaultProfiles.ContainsKey(2));
            Assert.IsTrue(defaultProfiles.ContainsKey(3));
            Assert.IsTrue(defaultProfiles.ContainsKey(4));
            Assert.IsTrue(defaultProfiles.ContainsKey(5));
            Assert.IsTrue(defaultProfiles.ContainsKey(6));
            Assert.IsTrue(defaultProfiles.ContainsKey(7));
            Assert.IsTrue(defaultProfiles.ContainsKey(8));
            Assert.IsTrue(defaultProfiles.ContainsKey(9));
            Assert.IsTrue(defaultProfiles.ContainsKey(10));
            Assert.IsTrue(defaultProfiles.ContainsKey(11));
            Assert.IsTrue(defaultProfiles.ContainsKey(12));
            Assert.IsTrue(defaultProfiles.ContainsKey(13));
            Assert.IsTrue(defaultProfiles.ContainsKey(255));
            Assert.AreEqual((uint)6416, defaultProfiles[1]);
            Assert.AreEqual((uint)6417, defaultProfiles[2]);
            Assert.AreEqual((uint)7118, defaultProfiles[4]);
            Assert.AreEqual((uint)7171, defaultProfiles[5]);
            Assert.AreEqual((uint)7172, defaultProfiles[6]);
            Assert.AreEqual((uint)7173, defaultProfiles[7]);
            Assert.AreEqual((uint)7174, defaultProfiles[8]);
            Assert.AreEqual((uint)7175, defaultProfiles[9]);
            Assert.AreEqual((uint)7176, defaultProfiles[10]);
            Assert.AreEqual((uint)7177, defaultProfiles[12]);
            Assert.AreEqual((uint)7178, defaultProfiles[13]);

            // A dynamic component which has null default profile
            Assert.IsNull(defaultProfiles[3]);
            Assert.IsNull(defaultProfiles[11]);
            Assert.IsNull(defaultProfiles[255]);
        }

        public static void MetaDataService_DefaultProfileIdForComponent(IWrapper wrapper)
        {
            var service = new MetaDataService(
                wrapper.Pipeline.FlowElements
                    .Where(e => typeof(IOnPremiseIpiEngine).IsAssignableFrom(e.GetType()))
                    .Cast<IOnPremiseIpiEngine>()
                    .ToArray());
            var defaultProfile1 = service.DefaultProfileIdForComponent(1);
            var defaultProfile2 = service.DefaultProfileIdForComponent(2);
            var defaultProfile3 = service.DefaultProfileIdForComponent(3);
            var defaultProfile4 = service.DefaultProfileIdForComponent(4);
            var defaultProfile5 = service.DefaultProfileIdForComponent(5);
            var defaultProfile6 = service.DefaultProfileIdForComponent(6);
            var defaultProfile7 = service.DefaultProfileIdForComponent(7);
            var defaultProfile8 = service.DefaultProfileIdForComponent(8);
            var defaultProfile9 = service.DefaultProfileIdForComponent(9);
            var defaultProfile10 = service.DefaultProfileIdForComponent(10);
            var defaultProfile11 = service.DefaultProfileIdForComponent(11);
            var defaultProfile12 = service.DefaultProfileIdForComponent(12);
            var defaultProfile13 = service.DefaultProfileIdForComponent(13);
            var defaultProfile99 = service.DefaultProfileIdForComponent(99);
            var defaultProfile255 = service.DefaultProfileIdForComponent(255);
            Assert.AreEqual((uint)6416, defaultProfile1);
            Assert.AreEqual((uint)6417, defaultProfile2);
            Assert.AreEqual((uint)7118, defaultProfile4);
            Assert.AreEqual((uint)7171, defaultProfile5);
            Assert.AreEqual((uint)7172, defaultProfile6);
            Assert.AreEqual((uint)7173, defaultProfile7);
            Assert.AreEqual((uint)7174, defaultProfile8);
            Assert.AreEqual((uint)7175, defaultProfile9);
            Assert.AreEqual((uint)7176, defaultProfile10);
            Assert.AreEqual((uint)7177, defaultProfile12);
            Assert.AreEqual((uint)7178, defaultProfile13);
            Assert.IsNull(defaultProfile3);
            Assert.IsNull(defaultProfile11);
            Assert.IsNull(defaultProfile99);
            Assert.IsNull(defaultProfile255);
        }

        // TODO: Currently the ComponentIdForProfile function is not supported
        // for MVP as the performance is not as expected.
        //
        // public static void MetaDataService_ComponentIdForProfile(IWrapper wrapper)
        // {
        //     var service = new MetaDataService(
        //         wrapper.Pipeline.FlowElements
        //             .Where(e => typeof(IOnPremiseIpiEngine).IsAssignableFrom(e.GetType()))
        //             .Cast<IOnPremiseIpiEngine>()
        //             .ToArray());
        //     var comonentFor6416 = service.ComponentIdForProfile(6416);
        //     var comonentFor6417 = service.ComponentIdForProfile(6417);
        //     var comonentFor7118 = service.ComponentIdForProfile(7118);
        //     var comonentFor7171 = service.ComponentIdForProfile(7171);
        //     var comonentFor7172 = service.ComponentIdForProfile(7172);
        //     var comonentFor7173 = service.ComponentIdForProfile(7173);
        //     var comonentFor7174 = service.ComponentIdForProfile(7174);
        //     var comonentFor7175 = service.ComponentIdForProfile(7175);
        //     var comonentFor7176 = service.ComponentIdForProfile(7176);
        //     var comonentFor7177 = service.ComponentIdForProfile(7177);
        //     var comonentFor7178 = service.ComponentIdForProfile(7178);
        //     var comonentFor999999999 = service.ComponentIdForProfile(999999999);
        //     var comonentFor0 = service.ComponentIdForProfile(0);
        //     Assert.AreEqual((byte)1, comonentFor6416);
        //     Assert.AreEqual((byte)2, comonentFor6417);
        //     Assert.AreEqual((byte)4, comonentFor7118);
        //     Assert.AreEqual((byte)5, comonentFor7171);
        //     Assert.AreEqual((byte)6, comonentFor7172);
        //     Assert.AreEqual((byte)7, comonentFor7173);
        //     Assert.AreEqual((byte)8, comonentFor7174);
        //     Assert.AreEqual((byte)9, comonentFor7175);
        //     Assert.AreEqual((byte)10, comonentFor7176);
        //     Assert.AreEqual((byte)12, comonentFor7177);
        //     Assert.AreEqual((byte)13, comonentFor7178);
        //     Assert.IsNull(comonentFor999999999);
        //     Assert.IsNull(comonentFor0);
        // }
    }
}
