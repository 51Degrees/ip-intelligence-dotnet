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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FiftyOne.IpIntelligence.Engine.OnPremise.Interop
{
    /// <summary>
    /// A UTF-8 encoding Marshaler to marshal the string
    /// being passed between C# and C/C++ layer as UTF-8
    /// encoded.
    /// 
    /// SWIG by default handle the C string as ASCII encoded
    /// when passing to/from C# layer. This class override
    /// the behaviour by treating the encoding as UTF-8.
    /// 
    /// </summary>
    public class UTF8Marshaler : ICustomMarshaler
    {
        private static UTF8Marshaler static_instance;

        /// <inheritdoc cref="ICustomMarshaler.MarshalManagedToNative"/>
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            if (managedObj is null)
            {
                return IntPtr.Zero;
            }
            
            var str = managedObj as string;
            if (str is null)
            {
                throw new MarshalDirectiveException(
                       Messages.ExceptionIncompatibleType);
            }

            // Convert string to UTF-8 bytes (not null terminated)
            var stringBytes = Encoding.UTF8.GetBytes(str);
            var memPtr = Marshal.AllocHGlobal(stringBytes.Length + 1);
            Marshal.Copy(stringBytes, 0, memPtr, stringBytes.Length);

            // Add null terminator at the end
            Marshal.WriteByte(memPtr + stringBytes.Length, 0);
            return memPtr;
        }

        /// <inheritdoc cref="ICustomMarshaler.MarshalNativeToManaged"/>
        public unsafe object MarshalNativeToManaged(IntPtr pNativeData)
        {
            byte* currentByte = (byte*)pNativeData;

            // Locate the string terminator
            for (; *currentByte != 0; currentByte++) { }
            var byteCount = (int)(currentByte - (byte*)pNativeData);

            // Extract bytes without null terminator
            var byteArray = new byte[byteCount];
            Marshal.Copy((IntPtr)pNativeData, byteArray, 0, byteCount);
            var resultString = Encoding.UTF8.GetString(byteArray);
            return resultString;
        }

        /// <inheritdoc cref="ICustomMarshaler.CleanUpNativeData"/>
        public void CleanUpNativeData(IntPtr pNativeData) => Marshal.FreeHGlobal(pNativeData);

        /// <inheritdoc cref="ICustomMarshaler.CleanUpManagedData"/>
        public void CleanUpManagedData(object managedObj) { }

        /// <inheritdoc cref="ICustomMarshaler.GetNativeDataSize"/>
        public int GetNativeDataSize() => -1;

        /// <summary>
        /// Returns a singleton instance of the UTF8Marshaler
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Usage", "CA1801:Review unused parameters", 
            Justification = "The cookie string is required" +
            "by the CLR interop layer but it is optional to use")]
        public static ICustomMarshaler GetInstance(string cookie) => 
            static_instance ?? (static_instance = new UTF8Marshaler());
    }
}
