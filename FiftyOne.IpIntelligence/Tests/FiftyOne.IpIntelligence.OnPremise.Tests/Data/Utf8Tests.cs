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
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Core.Data
{
    [TestClass]
    [TestCategory("Core")]
    [TestCategory("Utf8")]
    public class Utf8OnPremiseCoreTests : TestsBase
    {
        [TestMethod]
        public void Utf8_OnPremise_Core_Validate_Property_Values()
        {
            TestInitialize(PerformanceProfiles.Balanced);
            
            // Test Utf8 string: محافظة إب
            byte[] utf8Bytes =
                new byte[]{ 0xD9, 0x85, 0xD8, 0xAD, 0xD8, 0xA7, 0xD9, 0x81, 0xD8, 0xB8, 0xD8, 0xA9, 0x20, 0xD8, 0xA5, 0xD8, 0xA8 };
            
            ValidateUtf8PropertyValues(
                Wrapper, 
                "StateName", 
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
