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

using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
namespace FiftyOne.IpIntelligence.Shared
{
	/// <summary>
	/// Abstract base class for properties relating to an IP address.
	/// This includes the internet service providers, asns, connectivity
	/// and many others.
	/// </summary>
	public abstract class IpDataBase : AspectDataBase, IIpData
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
		protected IpDataBase(
			ILogger<AspectDataBase> logger,
			IPipeline pipeline,
			IAspectEngine engine,
			IMissingPropertyService missingPropertyService)
			: base(logger, pipeline, engine, missingPropertyService) { }

		/// <summary>
		/// Indicates the average latitude and longitude of all the coordinates that have been recorded for the 
		/// matched IP range
		/// </summary>
		public IAspectPropertyValue<Coordinate> AverageLocation { get { return GetAs<IAspectPropertyValue<Coordinate>>("AverageLocation"); } }
		/// <summary>
		/// Indicates the South East corner of the square box that bound all of the coordinates recorded for the
		/// matched IP range
		/// </summary>
		public IAspectPropertyValue<Coordinate> LocationBoundSouthEast { get { return GetAs<IAspectPropertyValue<Coordinate>>("LocationBoundSouthEast"); } }
		/// <summary>
		/// Indicates the North West corner of the square box that bound all of the coordinates records for the 
		/// matched IP range
		/// </summary>
		public IAspectPropertyValue<Coordinate> LocationBoundNorthWest { get { return GetAs<IAspectPropertyValue<Coordinate>>("LocationBoundNorthWest"); } }
		/// <summary>
		/// Indicates the list of country codes that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the country that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> CountryCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("CountryCode"); } }
		/// <summary>
		/// Indicates the list of Regions that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the region that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> RegionName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("RegionName"); } }
		/// <summary>
		/// Indicates the list of states that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the state that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> StateName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("StateName"); } }
		/// <summary>
		/// Indicates the list of counties that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the county that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> CountyName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("CountyName"); } }
		/// <summary>
		/// Indicates the list of cities that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the city that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> CityName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("CityName"); } }
		/// <summary>
		/// Indicates the list of timezones that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the timezone that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> TimezoneName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("TimezoneName"); } }
		/// <summary>
		/// Indicates the list of Isp names that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the Isp name that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> IspName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("IspName"); } }
		/// <summary>
		/// Indicates the list of Isp country codes that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the Isp country code that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> IspCountryCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("IspCountryCode"); } }
		/// <summary>
		/// Indicates the list of connectivity types that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the connectivity type that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> ConnectivityType { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("ConnectivityType"); } }
		/// <summary>
		/// Indicates the list of connectivity mcc(s) that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the connectivity mcc that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> MCC { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("MCC"); } }
		/// <summary>
		/// Indicates the list of connectivity mnc(s) that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the connectivity mnc that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> MNC { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("MNC"); } }
		/// <summary>
		/// Indicates the list of asns that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the asn that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<int>>> ASNumber { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<int>>>>("ASNumber"); } }
		/// <summary>
		/// Indicates the list of IP registered owner name that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the owner name that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> OwnerName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("OwnerName"); } }
		/// <summary>
		/// Indicates the list of IP registered owner address that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the owner address that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> OwnerAddress { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("OwnerAddress"); } }
		/// <summary>
		/// Indicates the list of IP registered country codes that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the registred country code that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> IpRegistrationCountryCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("IpRegistrationCountryCode"); } }
		/// <summary>
		/// Indicates the list of abuse contact name that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the abuse contact name that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> AbuseContactName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("AbuseContactName"); } }
		/// <summary>
		/// Indicates the list of abuse contact email that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the abuse contact email that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> AbuseContactEmail { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("AbuseContactEmail"); } }
		/// <summary>
		/// Indicates the list of abuse contact phone that have been recorded for the matched IP range together with their
		/// proportions representing the likelyhood of accuracy. The order is descending based on the proportion 
		/// such that the abuse contact phone that the IP address is most likely to be located will be placed first.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> AbuseContactPhone { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("AbuseContactPhone"); } }
		/// <summary>
		/// Indicates the start IP address of the IP range that the IP address falls within
		/// </summary>
		public IAspectPropertyValue<IPAddress> IpRangeStart { get { return GetAs<IAspectPropertyValue<IPAddress>>("IpRangeStart"); } }
		/// <summary>
		/// Indicates the end IP address of the IP range that the IP address falls within
		/// </summary>
		public IAspectPropertyValue<IPAddress> IpRangeEnd { get { return GetAs<IAspectPropertyValue<IPAddress>>("IpRangeEnd"); } }
		/// <summary>
		/// Indicates the network ID of the IP range the IP address falls within.
		/// The format represents a collection of the profile IDs and their percentages. Where the component is 
		/// dynamic, the profile ID will be 0 and its' percentage is 1
		/// e.g. '123:90.0|321:10.0|0:1.0'
		/// </summary>
		public IAspectPropertyValue<string> NetworkId { get { return GetAs<IAspectPropertyValue<string>>("NetworkId"); } }

        /// <summary>
        /// Network name
        /// </summary>
        public IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> NetworkName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>>>("networkname"); } }

        /// <summary>
        /// Coordinate
        /// </summary>
        public IAspectPropertyValue<IReadOnlyList<WeightedValue<Coordinate>>> Coordinate { get { return GetAs<IAspectPropertyValue<IReadOnlyList<WeightedValue<Coordinate>>>>("coordinate"); } }
    }
}
