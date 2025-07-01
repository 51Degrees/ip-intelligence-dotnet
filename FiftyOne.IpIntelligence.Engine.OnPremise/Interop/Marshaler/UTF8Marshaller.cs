/* *********************************************************************
 * This Work is of David Jeske and is distributed under the Code 
 * Project Open License (CPOL) 1.02. A copy of the license is included
 * in this same directory and a README.md has been provided, detailing
 * the changes that were made to the original source code.
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
        static UTF8Marshaler static_instance;

        /// <summary>
        /// Marshal a managed UTF-8 encoded string to the unmanaged
        /// string.
        /// </summary>
        /// <param name="managedObj">
        /// A managed UTF-8 encoded string.
        /// </param>
        /// <returns>
        /// Returns a custom common object model (COM) callable wrapper 
        /// (CCW) that can marshal the managed interface that is passed
        /// as an argument.
        /// </returns>
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            if (managedObj == null)
                return IntPtr.Zero;
            if (!(managedObj is string))
                throw new MarshalDirectiveException(
                       Messages.ExceptionIncompatibleType);

            // not null terminated
            byte[] strbuf = Encoding.UTF8.GetBytes((string)managedObj);
            IntPtr buffer = Marshal.AllocHGlobal(strbuf.Length + 1);
            Marshal.Copy(strbuf, 0, buffer, strbuf.Length);

            // write the terminating null
            Marshal.WriteByte(buffer + strbuf.Length, 0);
            return buffer;
        }

        /// <summary>
        /// Marshal a unmanaged UTF-8 encoded string to the managed .
        /// string
        /// </summary>
        /// <param name="pNativeData">
        /// A pointer to the unmanaged UTF-8 encoded string to be wrapped.
        /// </param>
        /// <returns>
        /// A managed UTF-8 encoded string.
        /// </returns>
        public unsafe object MarshalNativeToManaged(IntPtr pNativeData)
        {
            byte* walk = (byte*)pNativeData;

            // find the end of the string
            while (*walk != 0)
            {
                walk++;
            }
            int length = (int)(walk - (byte*)pNativeData);

            // should not be null terminated
            byte[] strbuf = new byte[length];
            // skip the trailing null
            Marshal.Copy((IntPtr)pNativeData, strbuf, 0, length);
            string data = Encoding.UTF8.GetString(strbuf);
            return data;
        }

        /// <summary>
        /// Clean any unmanaged data that was returned by 
        /// <see cref="MarshalManagedToNative"/>.
        /// </summary>
        /// <param name="pNativeData">
        /// A pointer to the native data to be cleaned.
        /// </param>
        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeHGlobal(pNativeData);
        }

        /// <summary>
        /// Clean any managed data returned by 
        /// <see cref="MarshalManagedToNative"/>.
        /// </summary>
        /// <param name="managedObj">
        /// A managed object to be cleaned.
        /// </param>
        public void CleanUpManagedData(object managedObj)
        {
        }

        /// <summary>
        /// Returns the size of the unmanaged data to be marshaled.
        /// </summary>
        /// <returns></returns>
        public int GetNativeDataSize()
        {
            return -1;
        }


        /// <summary>
        /// Return an instance of the UTF8Marshaler class.
        /// </summary>
        /// <param name="cookie">
        /// Not used
        /// </param>
        /// <returns>
        /// An an UTF8Marshaler instance.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Usage", "CA1801:Review unused parameters", 
            Justification = "The cookie string is required" +
            "by the CLR interop layer but it is optional to use")]
        public static ICustomMarshaler GetInstance(string cookie)
        {
            if (static_instance == null)
            {
                return static_instance = new UTF8Marshaler();
            }
            return static_instance;
        }
    }
}
