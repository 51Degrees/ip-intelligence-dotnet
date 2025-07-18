using FiftyOne.IpIntelligence.Engine.OnPremise.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.Engine.OnPremise.Data
{
    internal static class UTF8StringSwigExtensions
    {
        public static string ToUTF8String(this UTF8StringSwig stringSwig)
            => Encoding.UTF8.GetString(stringSwig.ToArray());
    }
}
