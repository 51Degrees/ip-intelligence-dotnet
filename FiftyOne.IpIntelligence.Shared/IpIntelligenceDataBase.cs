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
				{ "AccuracyRadiusMax", typeof(IAspectPropertyValue<int>) },
				{ "AccuracyRadiusMin", typeof(IAspectPropertyValue<int>) },
				{ "Areas", typeof(IAspectPropertyValue<WktString>) },
				{ "Asn", typeof(IAspectPropertyValue<string>) },
				{ "AsnName", typeof(IAspectPropertyValue<string>) },
				{ "ConnectionType", typeof(IAspectPropertyValue<string>) },
				{ "ContinentCode2", typeof(IAspectPropertyValue<string>) },
				{ "ContinentName", typeof(IAspectPropertyValue<string>) },
				{ "CountriesGeographical", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "CountriesGeographicalAll", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "CountriesPopulation", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "CountriesPopulationAll", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "Country", typeof(IAspectPropertyValue<string>) },
				{ "CountryCode", typeof(IAspectPropertyValue<string>) },
				{ "CountryCode3", typeof(IAspectPropertyValue<string>) },
				{ "County", typeof(IAspectPropertyValue<string>) },
				{ "CurrencyCode", typeof(IAspectPropertyValue<string>) },
				{ "DialCode", typeof(IAspectPropertyValue<string>) },
				{ "HumanProbability", typeof(IAspectPropertyValue<int>) },
				{ "IpRangeEnd", typeof(IAspectPropertyValue<IPAddress>) },
				{ "IpRangeStart", typeof(IAspectPropertyValue<IPAddress>) },
				{ "IsBroadband", typeof(IAspectPropertyValue<bool>) },
				{ "IsCellular", typeof(IAspectPropertyValue<bool>) },
				{ "IsEu", typeof(IAspectPropertyValue<bool>) },
				{ "IsHosted", typeof(IAspectPropertyValue<bool>) },
				{ "Iso31662Lvl4", typeof(IAspectPropertyValue<string>) },
				{ "Iso31662Lvl4SubdivisionOnly", typeof(IAspectPropertyValue<string>) },
				{ "Iso31662Lvl8", typeof(IAspectPropertyValue<string>) },
				{ "Iso31662Lvl8SubdivisionOnly", typeof(IAspectPropertyValue<string>) },
				{ "IsProxy", typeof(IAspectPropertyValue<bool>) },
				{ "IsPublicRouter", typeof(IAspectPropertyValue<bool>) },
				{ "IsTor", typeof(IAspectPropertyValue<bool>) },
				{ "IsVPN", typeof(IAspectPropertyValue<bool>) },
				{ "LanguageCode", typeof(IAspectPropertyValue<string>) },
				{ "Latitude", typeof(IAspectPropertyValue<float>) },
				{ "LocationConfidence", typeof(IAspectPropertyValue<string>) },
				{ "Longitude", typeof(IAspectPropertyValue<float>) },
				{ "Mcc", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "Region", typeof(IAspectPropertyValue<string>) },
				{ "RegisteredCountry", typeof(IAspectPropertyValue<string>) },
				{ "RegisteredName", typeof(IAspectPropertyValue<string>) },
				{ "RegisteredOwner", typeof(IAspectPropertyValue<string>) },
				{ "State", typeof(IAspectPropertyValue<string>) },
				{ "Suburb", typeof(IAspectPropertyValue<string>) },
				{ "TimeZoneIana", typeof(IAspectPropertyValue<string>) },
				{ "TimeZoneOffset", typeof(IAspectPropertyValue<int>) },
				{ "Town", typeof(IAspectPropertyValue<string>) },
				{ "ZipCode", typeof(IAspectPropertyValue<string>) }
			};

		/// <summary>
		/// Start of the IP range to which the evidence IP belongs.
		/// </summary>
		public IAspectPropertyValue<IPAddress> IpRangeStart { get { return GetAs<IAspectPropertyValue<IPAddress>>("IpRangeStart"); } }
		/// <summary>
		/// End of the IP range to which the evidence IP belongs.
		/// </summary>
		public IAspectPropertyValue<IPAddress> IpRangeEnd { get { return GetAs<IAspectPropertyValue<IPAddress>>("IpRangeEnd"); } }
		/// <summary>
		/// Name of the IP range. This is usually the owner.
		/// </summary>
		public IAspectPropertyValue<string> RegisteredName { get { return GetAs<IAspectPropertyValue<string>>("RegisteredName"); } }
		/// <summary>
		/// Registered owner of the range.
		/// </summary>
		public IAspectPropertyValue<string> RegisteredOwner { get { return GetAs<IAspectPropertyValue<string>>("RegisteredOwner"); } }
		/// <summary>
		/// Country code of the registered range.
		/// </summary>
		public IAspectPropertyValue<string> RegisteredCountry { get { return GetAs<IAspectPropertyValue<string>>("RegisteredCountry"); } }
		/// <summary>
		/// Radius in kilometers of the circle centred around the most probable location that encompasses the entire area. Where multiple areas are returned, this will only cover the area the most probable location is in. See Areas property. This will likely be a very large distance. It is recommend to use the AccuracyRadiusMin property.
		/// </summary>
		public IAspectPropertyValue<int> AccuracyRadiusMax { get { return GetAs<IAspectPropertyValue<int>>("AccuracyRadiusMax"); } }
		/// <summary>
		/// Radius in kilometers of the largest circle centred around the most probable location that fits within the area. Where multiple areas are returned, only the area that the most probable location falls within is considered. See Areas property.
		/// </summary>
		public IAspectPropertyValue<int> AccuracyRadiusMin { get { return GetAs<IAspectPropertyValue<int>>("AccuracyRadiusMin"); } }
		/// <summary>
		/// Average latitude of the IP. For privacy, this is randomized within around 1 kilometer of the result. Randomized result will change only once per day.
		/// </summary>
		public IAspectPropertyValue<float> Latitude { get { return GetAs<IAspectPropertyValue<float>>("Latitude"); } }
		/// <summary>
		/// Average longitude of the IP. For privacy, this is randomized within around 1 kilometer of the result. Randomized result will change only once per day.
		/// </summary>
		public IAspectPropertyValue<float> Longitude { get { return GetAs<IAspectPropertyValue<float>>("Longitude"); } }
		/// <summary>
		/// Any shapes associated with the location. Usually this is the area which the IP range covers. This is returned as a WKT String stored as a reduced format of WKB.
		/// </summary>
		public IAspectPropertyValue<WktString> Areas { get { return GetAs<IAspectPropertyValue<WktString>>("Areas"); } }
		/// <summary>
		/// Indicates the type of connection being used. Returns either Broadband, Cellular, or Hosting and Anonymous.
		/// </summary>
		public IAspectPropertyValue<string> ConnectionType { get { return GetAs<IAspectPropertyValue<string>>("ConnectionType"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a broadband connection. Includes DSL, Cable, Fibre, and Satellite connections.
		/// </summary>
		public IAspectPropertyValue<bool> IsBroadband { get { return GetAs<IAspectPropertyValue<bool>>("IsBroadband"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a cellular network.
		/// </summary>
		public IAspectPropertyValue<bool> IsCellular { get { return GetAs<IAspectPropertyValue<bool>>("IsCellular"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with hosting. Includes both hosting and anonymised connections such as hosting networks, hosting ASNs, VPNs, proxies, TOR networks, and unreachable IP addresses.
		/// </summary>
		public IAspectPropertyValue<bool> IsHosted { get { return GetAs<IAspectPropertyValue<bool>>("IsHosted"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a VPN server.
		/// </summary>
		public IAspectPropertyValue<bool> IsVPN { get { return GetAs<IAspectPropertyValue<bool>>("IsVPN"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a Proxy server.
		/// </summary>
		public IAspectPropertyValue<bool> IsProxy { get { return GetAs<IAspectPropertyValue<bool>>("IsProxy"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a public router.
		/// </summary>
		public IAspectPropertyValue<bool> IsPublicRouter { get { return GetAs<IAspectPropertyValue<bool>>("IsPublicRouter"); } }
		/// <summary>
		/// Indicates whether the IP address is associated with a TOR server.
		/// </summary>
		public IAspectPropertyValue<bool> IsTor { get { return GetAs<IAspectPropertyValue<bool>>("IsTor"); } }
		/// <summary>
		/// The ISO 3166-2 code for the supplied location. This is using the 'ISO3166-2-lvl4' property from OpenStreetMap.
		/// </summary>
		public IAspectPropertyValue<string> Iso31662Lvl4 { get { return GetAs<IAspectPropertyValue<string>>("Iso31662Lvl4"); } }
		/// <summary>
		/// The alphanumeric code representing the subdivision from the ISO 3166-2 code of the supplied location. This is using the 'ISO3166-2-lvl4' property from OpenStreetMap.
		/// </summary>
		public IAspectPropertyValue<string> Iso31662Lvl4SubdivisionOnly { get { return GetAs<IAspectPropertyValue<string>>("Iso31662Lvl4SubdivisionOnly"); } }
		/// <summary>
		/// The ISO 3166-2 code for the supplied location. This is using the 'ISO3166-2-lvl8' property from OpenStreetMap.
		/// </summary>
		public IAspectPropertyValue<string> Iso31662Lvl8 { get { return GetAs<IAspectPropertyValue<string>>("Iso31662Lvl8"); } }
		/// <summary>
		/// The alphanumeric code representing the subdivision from the ISO 3166-2 code of the supplied location. This is using the 'ISO3166-2-lvl8' property from OpenStreetMap.
		/// </summary>
		public IAspectPropertyValue<string> Iso31662Lvl8SubdivisionOnly { get { return GetAs<IAspectPropertyValue<string>>("Iso31662Lvl8SubdivisionOnly"); } }
		/// <summary>
		/// The 3-character ISO 3166-1 continent code for the supplied location.
		/// </summary>
		public IAspectPropertyValue<string> ContinentCode2 { get { return GetAs<IAspectPropertyValue<string>>("ContinentCode2"); } }
		/// <summary>
		/// The name of the continent the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<string> ContinentName { get { return GetAs<IAspectPropertyValue<string>>("ContinentName"); } }
		/// <summary>
		/// The name of the county that the supplied location is in. In this case, a county is defined as an administrative sub-section of a country or state.
		/// </summary>
		public IAspectPropertyValue<string> County { get { return GetAs<IAspectPropertyValue<string>>("County"); } }
		/// <summary>
		/// The name of the country that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<string> Country { get { return GetAs<IAspectPropertyValue<string>>("Country"); } }
		/// <summary>
		/// The 2-character ISO 3166-1 code of the country that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<string> CountryCode { get { return GetAs<IAspectPropertyValue<string>>("CountryCode"); } }
		/// <summary>
		/// The 3-character ISO 3166-1 alpha-3 code of the country that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<string> CountryCode3 { get { return GetAs<IAspectPropertyValue<string>>("CountryCode3"); } }
		/// <summary>
		/// A list of countries in ISO 3166-1 alpha-2 country code format that overlap with the area likely associated with the provided evidence. These are weighted and ordered by each country's proportion of the area.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountriesGeographical { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CountriesGeographical"); } }
		/// <summary>
		/// A full list of countries in ISO 3166-1 alpha-2 country code format. Countries that overlap with the area likely associated with the provided evidence are listed first, weighted and ordered by each country's proportion of the area. This is then followed by the remaining countries, ordered according to ISO 3166-1 alpha 2 standard.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountriesGeographicalAll { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CountriesGeographicalAll"); } }
		/// <summary>
		/// A list of countries in ISO 3166-1 alpha-2 country code format that overlap with the area likely associated with the provided evidence. These are weighted and ordered by each country's proportion of the total population within the area.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountriesPopulation { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CountriesPopulation"); } }
		/// <summary>
		/// A full list of countries in ISO 3166-1 alpha-2 country code format. Countries that overlap with the area likely associated with the provided evidence are listed first, weighted and ordered by each country's proportion of the total population within the area. This is then followed by the remaining countries, ordered according to ISO 3166-1 alpha 2 standard.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountriesPopulationAll { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CountriesPopulationAll"); } }
		/// <summary>
		/// The Alpha-3 ISO 4217 code of the currency associated with the supplied location.
		/// </summary>
		public IAspectPropertyValue<string> CurrencyCode { get { return GetAs<IAspectPropertyValue<string>>("CurrencyCode"); } }
		/// <summary>
		/// Indicates whether the country of the supplied location is within the European Union.
		/// </summary>
		public IAspectPropertyValue<bool> IsEu { get { return GetAs<IAspectPropertyValue<bool>>("IsEu"); } }
		/// <summary>
		/// ITU international telephone numbering plan code for the country.
		/// </summary>
		public IAspectPropertyValue<string> DialCode { get { return GetAs<IAspectPropertyValue<string>>("DialCode"); } }
		/// <summary>
		/// The confidence that the IP address is a human user versus associated with hosting. A 0-10 value where; 0-3: Low confidence the user is human, 4-6: Medium confidence, 7-10: High confidence. A -1 value indicates that the probability is unknown.
		/// </summary>
		public IAspectPropertyValue<int> HumanProbability { get { return GetAs<IAspectPropertyValue<int>>("HumanProbability"); } }
		/// <summary>
		/// The Alpha-2 ISO 639 Language code associated with the supplied location.
		/// </summary>
		public IAspectPropertyValue<string> LanguageCode { get { return GetAs<IAspectPropertyValue<string>>("LanguageCode"); } }
		/// <summary>
		/// The confidence in the town and country provided.
		/// </summary>
		public IAspectPropertyValue<string> LocationConfidence { get { return GetAs<IAspectPropertyValue<string>>("LocationConfidence"); } }
		/// <summary>
		/// The name of the geographical region that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<string> Region { get { return GetAs<IAspectPropertyValue<string>>("Region"); } }
		/// <summary>
		/// The time zone at the supplied location in the IANA Time Zone format.
		/// </summary>
		public IAspectPropertyValue<string> TimeZoneIana { get { return GetAs<IAspectPropertyValue<string>>("TimeZoneIana"); } }
		/// <summary>
		/// The offset from UTC in minutes in the supplied location, at the time that the value is produced.
		/// </summary>
		public IAspectPropertyValue<int> TimeZoneOffset { get { return GetAs<IAspectPropertyValue<int>>("TimeZoneOffset"); } }
		/// <summary>
		/// The name of the town that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<string> Town { get { return GetAs<IAspectPropertyValue<string>>("Town"); } }
		/// <summary>
		/// The name of the state that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<string> State { get { return GetAs<IAspectPropertyValue<string>>("State"); } }
		/// <summary>
		/// The name of the suburb that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<string> Suburb { get { return GetAs<IAspectPropertyValue<string>>("Suburb"); } }
		/// <summary>
		/// The zip or postal code that the supplied location falls under.
		/// </summary>
		public IAspectPropertyValue<string> ZipCode { get { return GetAs<IAspectPropertyValue<string>>("ZipCode"); } }
		/// <summary>
		/// The mobile country code of the network the device is connected to.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Mcc { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Mcc"); } }
		/// <summary>
		/// The name registered to the Asn associated with the IP address.
		/// </summary>
		public IAspectPropertyValue<string> AsnName { get { return GetAs<IAspectPropertyValue<string>>("AsnName"); } }
		/// <summary>
		/// Autonomous System Number associated with the IP address.
		/// </summary>
		public IAspectPropertyValue<string> Asn { get { return GetAs<IAspectPropertyValue<string>>("Asn"); } }
	}
}
