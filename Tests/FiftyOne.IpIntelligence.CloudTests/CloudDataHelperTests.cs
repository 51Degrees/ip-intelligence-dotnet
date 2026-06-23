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

using FiftyOne.IpIntelligence.Shared.Services;
using FiftyOne.Pipeline.Core.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;

namespace FiftyOne.IpIntelligence.CloudTests;

[TestClass]
public class CloudDataHelperTests
{
    /// <summary>
    /// Verify ToValueForAPV returns null when rawValue is null.
    /// </summary>
    [TestMethod]
    public void ToValueForAPV_NullRawValue_ReturnsNull()
    {
        var result = CloudDataHelper.ToValueForAPV(null, typeof(string));
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verify ToValueForAPV converts string to string unchanged.
    /// </summary>
    [TestMethod]
    public void ToValueForAPV_String_ReturnsString()
    {
        var rawValue = "test";
        var result = CloudDataHelper.ToValueForAPV(
            rawValue, 
            typeof(string));
        Assert.AreEqual("test", result);
    }

    /// <summary>
    /// Verify ToValueForAPV converts int to int.
    /// </summary>
    [TestMethod]
    public void ToValueForAPV_Int_ReturnsInt()
    {
        var rawValue = 42;
        var result = CloudDataHelper.ToValueForAPV(rawValue, typeof(int));
        Assert.AreEqual(42, result);
    }

    /// <summary>
    /// Verify ToValueForAPV converts string IP to IPAddress using IpTypeConverter.
    /// </summary>
    [TestMethod]
    public void ToValueForAPV_IPAddress_StringToIPAddress()
    {
        var rawValue = "8.8.8.8";
        var result = CloudDataHelper.ToValueForAPV(
            rawValue, 
            typeof(IPAddress),
            CloudDataHelper.IpTypeConverter);
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(IPAddress), result.GetType());
        Assert.AreEqual("8.8.8.8", result.ToString());
    }

    /// <summary>
    /// Verify ToValueForAPV converts string list to List<string>.
    /// </summary>
    [TestMethod]
    public void ToValueForAPV_StringList_ReturnsList()
    {
        var rawValue = new List<string> { "a", "b", "c" };
        var result = CloudDataHelper.ToValueForAPV(
            rawValue, 
            typeof(List<string>));
        Assert.IsNotNull(result);
        Assert.AreEqual(typeof(List<string>), result.GetType());
        var list = (List<string>)result;
        Assert.AreEqual(3, list.Count);
    }

    /// <summary>
    /// Verify IpTypeConverter is not null.
    /// </summary>
    [TestMethod]
    public void IpTypeConverter_IsNotNull()
    {
        Assert.IsNotNull(CloudDataHelper.IpTypeConverter);
    }

    /// <summary>
    /// Verify IpTypeConverter returns null for non-IPAddress type.
    /// </summary>
    [TestMethod]
    public void IpTypeConverter_ReturnsNull_ForNonIPAddress()
    {
        var result = CloudDataHelper
            .IpTypeConverter("test", typeof(string));
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verify CreateAPVDictionary throws for null input.
    /// </summary>
    [TestMethod]
    public void CreateAPVDictionary_NullInput_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            CloudDataHelper.CreateAPVDictionary(
                null,
                []));
    }

    /// <summary>
    /// Verify CreateAPVDictionary returns empty dict for empty cloud data.
    /// </summary>
    [TestMethod]
    public void CreateAPVDictionary_EmptyCloudData_ReturnsEmpty()
    {
        var cloudData = new Dictionary<string, object>();
        var result = CloudDataHelper.CreateAPVDictionary(
            cloudData, []);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}
