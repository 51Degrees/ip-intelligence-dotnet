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
				{ "AccuracyRadius", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>) },
				{ "Areas", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "Country", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "CountryCode", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "CountryCode3", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "IpRangeEnd", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>) },
				{ "IpRangeStart", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>) },
				{ "Latitude", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>) },
				{ "Longitude", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>) },
				{ "Region", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "RegisteredCountry", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "RegisteredName", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "RegisteredOwner", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "State", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) },
				{ "TimeZoneOffset", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>) },
				{ "Town", typeof(IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>) }
			};

		/// <summary>
		/// End of the IP range to which the evidence IP belongs.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>> IpRangeEnd { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>>("IpRangeEnd"); } }
		/// <summary>
		/// Start of the IP range to which the evidence IP belongs.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>> IpRangeStart { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>>("IpRangeStart"); } }
		/// <summary>
		/// Country code of the registered range.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredCountry { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("RegisteredCountry"); } }
		/// <summary>
		/// Name of the IP range. This is usually the owner.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredName { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("RegisteredName"); } }
		/// <summary>
		/// Registered owner of the range.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredOwner { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("RegisteredOwner"); } }
		/// <summary>
		/// Accuracy radius of the matched location in meters.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> AccuracyRadius { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>>("AccuracyRadius"); } }
		/// <summary>
		/// Any shapes associated with the location. Usually this is the area which the IP range covers.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Areas { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Areas"); } }
		/// <summary>
		/// The name of the country that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Country { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Country"); } }
		/// <summary>
		/// The 2-character ISO 3166-1 code of the country that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountryCode { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CountryCode"); } }
		/// <summary>
		/// The 3-character ISO 3166-1 alpha-3 code of the country that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountryCode3 { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("CountryCode3"); } }
		/// <summary>
		/// Average latitude of the IP. For privacy, this is randomized within around 1 mile of the result. Randomized result will change only once per day.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>> Latitude { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>>("Latitude"); } }
		/// <summary>
		/// Average longitude of the IP. For privacy, this is randomized within around 1 mile of the result. Randomized result will change only once per day.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>> Longitude { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>>("Longitude"); } }
		/// <summary>
		/// The name of the geographical region that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Region { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Region"); } }
		/// <summary>
		/// The name of the state that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> State { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("State"); } }
		/// <summary>
		/// The offset from UTC in minutes in the supplied location, at the time that the value is produced.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> TimeZoneOffset { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>>("TimeZoneOffset"); } }
		/// <summary>
		/// The name of the town that the supplied location is in.
		/// </summary>
		public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Town { get { return GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>("Town"); } }
	}
}
