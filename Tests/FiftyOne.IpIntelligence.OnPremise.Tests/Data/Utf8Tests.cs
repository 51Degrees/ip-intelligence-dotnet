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

using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Core.Data
{
    [Ignore]
    [TestClass]
    [TestCategory("Core")]
    [TestCategory("Utf8")]
    public class Utf8OnPremiseCoreTests : TestsBase
    {
        [TestMethod]
        //[DataRow(new byte[]{
        //    0x50,0xc3,0xa4,0x69,0x6a,0xc3,0xa4,0x74,0x2d,0x48,0xc3,0xa4,0x6d,0x65,
        //}, "State", DisplayName = nameof(Utf8_OnPremise_Core_Validate_Property_Values) + "(Päijät-Häme)")]
        [DataRow(new byte[]{
            0x28, 0x31, 0x29, 0x20, 0xD8, 0xA7, 0xD9, 0x84, 0xD9, 0x85, 0xD9, 0x86, 0xD8, 0xB7, 0xD9, 0x82, 0xD8, 0xA9, 0x20, 0xD8, 0xA7, 0xD9, 0x84, 0xD8, 0xB4, 0xD8, 0xB1, 0xD9, 0x82, 0xD9, 0x8A, 0xD8, 0xA9,
        }, "County", DisplayName = nameof(Utf8_OnPremise_Core_Validate_Property_Values) + "((1) المنطقة الشرقية)")]
        //[DataRow(new byte[]{
        //    0x42,0xe1,0xba,0xbf,0x6e,0x20,0x54,0x72,0x65,0x20,0x50,0x72,0x6f,0x76,0x69,0x6e,0x63,0x65,
        //}, "State", DisplayName = nameof(Utf8_OnPremise_Core_Validate_Property_Values) + "(Bến Tre Province)")]
        public void Utf8_OnPremise_Core_Validate_Property_Values(byte[] utf8Bytes, string propertyName)
        {
            TestInitialize(PerformanceProfiles.MaxPerformance);

            ValidateUtf8PropertyValues(
                Wrapper,
                propertyName,
                Encoding.UTF8.GetString(utf8Bytes));
        }

        private void ValidateUtf8PropertyValues(
            WrapperOnPremise wrapper,
            string propertyName,
            string targetUtf8Value)
        {
            IList<IFiftyOneAspectPropertyMetaData> properties = 
                ((IpiOnPremiseEngine)wrapper.GetEngine()).Properties;
            bool propertyFound = false;
            foreach (var property in properties)
            {
                if (property.Name.Equals(propertyName))
                {
                    var value = property.GetValue(targetUtf8Value);
                    propertyFound = true;
                    Assert.IsNotNull(value);
                    Assert.AreEqual(targetUtf8Value, value.Name);
                }
            }
            Assert.IsTrue(propertyFound);
        }
    }
}
