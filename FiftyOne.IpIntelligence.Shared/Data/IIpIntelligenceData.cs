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

using System.Net;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Engines.Data;
using System.Collections.Generic;

// This interface sits at the top of the name space in order to make 
// life easier for consumers.
namespace FiftyOne.IpIntelligence
{
	/// <summary>
	/// Represents a data object containing values relating to an IP.
	/// This includes the network, and location.
	/// </summary>
	public interface IIpIntelligenceData : IAspectData
	{
		/// <summary>
		/// Radius in kilometers of the circle centred around the most probable location that encompasses the entire area. Where multiple areas are returned, this will only cover the area the most probable location is in. See Areas property. This will likely be a very large distance. It is recommend to use the AccuracyRadiusMin property.
		/// </summary>
		IAspectPropertyValue<int> AccuracyRadiusMax { get; }
		/// <summary>
		/// Radius in kilometers of the largest circle centred around the most probable location that fits within the area. Where multiple areas are returned, only the area that the most probable location falls within is considered. See Areas property.
		/// </summary>
		IAspectPropertyValue<int> AccuracyRadiusMin { get; }
		/// <summary>
		/// Any shapes associated with the location. Usually this is the area which the IP range covers. This is returned as a WKT String stored as a reduced format of WKB.
		/// </summary>
		IAspectPropertyValue<WktString> Areas { get; }
		/// <summary>
		/// Autonomous System Number associated with the IP address.
		/// </summary>
		IAspectPropertyValue<string> Asn { get; }
		/// <summary>
		/// The name registered to the Asn associated with the IP address.
		/// </summary>
		IAspectPropertyValue<string> AsnName { get; }
		/// <summary>
		/// Indicates the type of connection being used. Returns either Broadband, Cellular, or Hosting and Anonymous.
		/// </summary>
		IAspectPropertyValue<string> ConnectionType { get; }
		/// <summary>
		/// The 3-character ISO 3166-1 continent code for the supplied location.
		/// </summary>
		IAspectPropertyValue<string> ContinentCode2 { get; }
		/// <summary>
		/// The name of the continent the supplied location is in.
		/// </summary>
		IAspectPropertyValue<string> ContinentName { get; }
		/// <summary>
		/// The name of the country that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<string> Country { get; }
		/// <summary>
		/// The 2-character ISO 3166-1 code of the country that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<string> CountryCode { get; }
		/// <summary>
		/// The 3-character ISO 3166-1 alpha-3 code of the country that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<string> CountryCode3 { get; }
		/// <summary>
		/// The name of the county that the supplied location is in. In this case, a county is defined as an administrative sub-section of a country or state.
		/// </summary>
		IAspectPropertyValue<string> County { get; }
		/// <summary>
		/// The Alpha-3 ISO 4217 code of the currency associated with the supplied location.
		/// </summary>
		IAspectPropertyValue<string> CurrencyCode { get; }
		/// <summary>
		/// ITU international telephone numbering plan code for the country.
		/// </summary>
		IAspectPropertyValue<string> DialCode { get; }
		/// <summary>
		/// The confidence that the IP address is a human user versus associated with hosting. A 1-10 value where; 1-3: Low confidence the user is human, 4-6: Medium confidence, 7-10: High confidence.
		/// </summary>
		IAspectPropertyValue<int> HumanProbability { get; }
		/// <summary>
		/// End of the IP range to which the evidence IP belongs.
		/// </summary>
		IAspectPropertyValue<IPAddress> IpRangeEnd { get; }
		/// <summary>
		/// Start of the IP range to which the evidence IP belongs.
		/// </summary>
		IAspectPropertyValue<IPAddress> IpRangeStart { get; }
		/// <summary>
		/// Indicates whether the IP address is associated with a broadband connection. Includes DSL, Cable, Fibre, and Satellite connections.
		/// </summary>
		IAspectPropertyValue<bool> IsBroadband { get; }
		/// <summary>
		/// Indicates whether the IP address is associated with a cellular network.
		/// </summary>
		IAspectPropertyValue<bool> IsCellular { get; }
		/// <summary>
		/// Indicates whether the country of the supplied location is within the European Union.
		/// </summary>
		IAspectPropertyValue<bool> IsEu { get; }
		/// <summary>
		/// Indicates whether the IP address is associated with hosting. Includes both hosting and anonymised connections such as hosting networks, hosting ASNs, VPNs, proxies, TOR networks, and unreachable IP addresses.
		/// </summary>
		IAspectPropertyValue<bool> IsHosted { get; }
		/// <summary>
		/// Indicates whether the IP address is associated with a Proxy server.
		/// </summary>
		IAspectPropertyValue<bool> IsProxy { get; }
		/// <summary>
		/// Indicates whether the IP address is associated with a public router.
		/// </summary>
		IAspectPropertyValue<bool> IsPublicRouter { get; }
		/// <summary>
		/// Indicates whether the IP address is associated with a TOR server.
		/// </summary>
		IAspectPropertyValue<bool> IsTor { get; }
		/// <summary>
		/// Indicates whether the IP address is associated with a VPN server.
		/// </summary>
		IAspectPropertyValue<bool> IsVPN { get; }
		/// <summary>
		/// The Alpha-2 ISO 639 Language code associated with the supplied location.
		/// </summary>
		IAspectPropertyValue<string> LanguageCode { get; }
		/// <summary>
		/// Average latitude of the IP. For privacy, this is randomized within around 1 kilometer of the result. Randomized result will change only once per day.
		/// </summary>
		IAspectPropertyValue<float> Latitude { get; }
		/// <summary>
		/// The confidence in the town and country provided.
		/// </summary>
		IAspectPropertyValue<string> LocationConfidence { get; }
		/// <summary>
		/// Average longitude of the IP. For privacy, this is randomized within around 1 kilometer of the result. Randomized result will change only once per day.
		/// </summary>
		IAspectPropertyValue<float> Longitude { get; }
		/// <summary>
		/// The mobile country code of the network the device is connected to.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Mcc { get; }
		/// <summary>
		/// The name of the geographical region that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<string> Region { get; }
		/// <summary>
		/// Country code of the registered range.
		/// </summary>
		IAspectPropertyValue<string> RegisteredCountry { get; }
		/// <summary>
		/// Name of the IP range. This is usually the owner.
		/// </summary>
		IAspectPropertyValue<string> RegisteredName { get; }
		/// <summary>
		/// Registered owner of the range.
		/// </summary>
		IAspectPropertyValue<string> RegisteredOwner { get; }
		/// <summary>
		/// The name of the state that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<string> State { get; }
		/// <summary>
		/// The name of the suburb that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<string> Suburb { get; }
		/// <summary>
		/// The time zone at the supplied location in the IANA Time Zone format.
		/// </summary>
		IAspectPropertyValue<string> TimeZoneIana { get; }
		/// <summary>
		/// The offset from UTC in minutes in the supplied location, at the time that the value is produced.
		/// </summary>
		IAspectPropertyValue<int> TimeZoneOffset { get; }
		/// <summary>
		/// The name of the town that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<string> Town { get; }
		/// <summary>
		/// The zip or postal code that the supplied location falls under.
		/// </summary>
		IAspectPropertyValue<string> ZipCode { get; }
	}
}
