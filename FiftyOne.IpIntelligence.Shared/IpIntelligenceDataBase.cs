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
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
namespace FiftyOne.IpIntelligence.Shared
{
	/// <summary>
	/// Abstract base class for properties relating to an IP.
	/// This includes the network, and location.
	/// </summary>
	public abstract class IpIntelligenceData : AspectDataBase, IIpIntelligenceData
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="logger">
		/// The logger for this instance to use.
		/// </param>
		/// <param name="pipeline">
		/// The Pipeline this data instance has been created by.
		/// </param>
		/// <param name="engine">
		/// The engine this data instance has been created by.
		/// </param>
		/// <param name="missingPropertyService">
		/// The missing property service to use when a requested property
		/// does not exist.
		/// </param>
		protected IpIntelligenceData(
			ILogger<AspectDataBase> logger,
			IPipeline pipeline,
			IAspectEngine engine,
			IMissingPropertyService missingPropertyService)
			: base(logger, pipeline, engine, missingPropertyService) { }

		/// <summary>
		/// Dictionary of property value types, keyed on the string
		/// name of the type.
		/// </summary>
		protected static readonly IReadOnlyDictionary<string, Type> PropertyTypes =
			new Dictionary<string, Type>()
			{
				{ "AccuracyRadiusMax", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>) },
				{ "AccuracyRadiusMin", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>) },
				{ "Areas", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "AsnName", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "AsnNumber", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "ConnectionType", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "ContinentCode2", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "ContinentName", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "Country", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "CountryCode", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "CountryCode3", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "County", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "CurrencyCode", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "DialCode", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>) },
				{ "HumanProbability", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>) },
				{ "IpRangeEnd", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>) },
				{ "IpRangeStart", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>) },
				{ "IsBroadband", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>) },
				{ "IsCellular", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>) },
				{ "IsEu", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>) },
				{ "IsHosted", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>) },
				{ "IsProxy", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>) },
				{ "IsPublicRouter", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>) },
				{ "IsTor", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>) },
				{ "IsVPN", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>) },
				{ "LanguageCode", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "Latitude", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>) },
				{ "LocationConfidence", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "Longitude", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>) },
				{ "Mcc", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "Region", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "RegisteredCountry", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "RegisteredName", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "RegisteredOwner", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "State", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "Suburb", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "TimeZoneIana", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "TimeZoneOffset", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>) },
				{ "Town", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "ZipCode", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) }
			};

		/// <summary>
		/// End of the IP range to which the evidence IP belongs.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>> IpRangeEnd { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>>("IpRangeEnd"); } }
		/// <summary>
		/// Start of the IP range to which the evidence IP belongs.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>> IpRangeStart { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>>("IpRangeStart"); } }
		/// <summary>
		/// Country code of the registered range.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredCountry { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("RegisteredCountry"); } }
		/// <summary>
		/// Name of the IP range. This is usually the owner.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("RegisteredName"); } }
		/// <summary>
		/// Registered owner of the range.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredOwner { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("RegisteredOwner"); } }
		/// <summary>
		/// Radius in meters of the circle centred around the most probable location that encompasses the entire area(s). See Areas property. This will likely be a very large distance. It is recommend to use the AccuracyRadiusMin property.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> AccuracyRadiusMax { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>>("AccuracyRadiusMax"); } }
		/// <summary>
		/// Radius in meters of the largest circle centred around the most probable location that fits within the area. Where multiple areas are returned, only the area that the most probable location falls within is considered. See Areas property.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> AccuracyRadiusMin { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>>("AccuracyRadiusMin"); } }
		/// <summary>
		/// Any shapes associated with the location. Usually this is the area which the IP range covers.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Areas { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Areas"); } }
		/// <summary>
		/// Indicates the type of connection being used. Returns either Broadband, Cellular, or Hosting and Anonymous.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> ConnectionType { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("ConnectionType"); } }
		/// <summary>
		/// The 3-character ISO 3166-1 continent code for the supplied location.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> ContinentCode2 { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("ContinentCode2"); } }
		/// <summary>
		/// The name of the continent the supplied location is in.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> ContinentName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("ContinentName"); } }
		/// <summary>
		/// The name of the country that the supplied location is in.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Country { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Country"); } }
		/// <summary>
		/// The 2-character ISO 3166-1 code of the country that the supplied location is in.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountryCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CountryCode"); } }
		/// <summary>
		/// The 3-character ISO 3166-1 alpha-3 code of the country that the supplied location is in.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountryCode3 { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CountryCode3"); } }
		/// <summary>
		/// The name of the county that the supplied location is in. In this case, a county is defined as an administrative sub-section of a country or state.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> County { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("County"); } }
		/// <summary>
		/// The Alpha-3 ISO 4217 code of the currency associated with the supplied location.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CurrencyCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CurrencyCode"); } }
		/// <summary>
		/// ITU international?telephone numbering plan code for the country.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> DialCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>>("DialCode"); } }
		/// <summary>
		/// The confidence that the IP address is a human user versus associated with hosting. A 1-10 value where; 1-3: Low confidence the user is human, 4-6: Medium confidence the user is human, 7-10: High confidence the user is human.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> HumanProbability { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>>("HumanProbability"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a broadband connection. Includes DSL, Cable, Fibre, and Satellite connections.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> IsBroadband { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>>("IsBroadband"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a cellular network.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> IsCellular { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>>("IsCellular"); } }
		/// <summary>
		/// Indicates whether the country of the supplied location is within the European Union.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> IsEu { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>>("IsEu"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with hosting. Includes both hosting and anonymised connections such as hosting networks, hosting ASNs, VPNs, proxies, TOR networks, and unreachable IP addresses.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> IsHosted { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>>("IsHosted"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a Proxy server.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> IsProxy { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>>("IsProxy"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a public router.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> IsPublicRouter { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>>("IsPublicRouter"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a TOR server.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> IsTor { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>>("IsTor"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a VPN server.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> IsVPN { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>>("IsVPN"); } }
		/// <summary>
		/// The Alpha-2 ISO 639 Language code associated with the supplied location.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> LanguageCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("LanguageCode"); } }
		/// <summary>
		/// Average latitude of the IP. For privacy, this is randomized within around 1 kilometer of the result. Randomized result will change only once per day.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>> Latitude { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>>("Latitude"); } }
		/// <summary>
		/// The confidence in the town and country provided.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> LocationConfidence { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("LocationConfidence"); } }
		/// <summary>
		/// Average longitude of the IP. For privacy, this is randomized within around 1 kilometer of the result. Randomized result will change only once per day.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>> Longitude { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>>("Longitude"); } }
		/// <summary>
		/// The name of the geographical region that the supplied location is in.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Region { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Region"); } }
		/// <summary>
		/// The name of the state that the supplied location is in.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> State { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("State"); } }
		/// <summary>
		/// The name of the suburb that the supplied location is in.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Suburb { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Suburb"); } }
		/// <summary>
		/// The time zone at the supplied location in the IANA Time Zone format.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> TimeZoneIana { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("TimeZoneIana"); } }
		/// <summary>
		/// The offset from UTC in minutes in the supplied location, at the time that the value is produced.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> TimeZoneOffset { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>>("TimeZoneOffset"); } }
		/// <summary>
		/// The name of the town that the supplied location is in.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Town { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Town"); } }
		/// <summary>
		/// The zip or postal code that the supplied location falls under.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> ZipCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("ZipCode"); } }
		/// <summary>
		/// The mobile country code of the network the device is connected to.
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Mcc { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Mcc"); } }
		/// <summary>
		/// 
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> AsnName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("AsnName"); } }
		/// <summary>
		/// 
		/// <para>More information: <see href="Network"/></para>
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> AsnNumber { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("AsnNumber"); } }
	}
}
