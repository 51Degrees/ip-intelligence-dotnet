
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
		/// Accuracy radius of the matched location in meters.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> AccuracyRadius { get; }
		/// <summary>
		/// Any shapes associated with the location. Usually this is the area which the IP range covers.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Areas { get; }
		/// <summary>
		/// The name of the country that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Country { get; }
		/// <summary>
		/// The 2-character ISO 3166-1 code of the country that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountryCode { get; }
		/// <summary>
		/// The 3-character ISO 3166-1 alpha-3 code of the country that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> CountryCode3 { get; }
		/// <summary>
		/// End of the IP range to which the evidence IP belongs.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>> IpRangeEnd { get; }
		/// <summary>
		/// Start of the IP range to which the evidence IP belongs.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>> IpRangeStart { get; }
		/// <summary>
		/// Average latitude of the IP. For privacy, this is randomized within around 1 mile of the result. Randomized result will change only once per day.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>> Latitude { get; }
		/// <summary>
		/// Average longitude of the IP. For privacy, this is randomized within around 1 mile of the result. Randomized result will change only once per day.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>> Longitude { get; }
		/// <summary>
		/// The mobile country code of the network the device is connected to.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Mcc { get; }
		/// <summary>
		/// The name of the geographical region that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Region { get; }
		/// <summary>
		/// Country code of the registered range.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredCountry { get; }
		/// <summary>
		/// Name of the IP range. This is usually the owner.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredName { get; }
		/// <summary>
		/// Registered owner of the range.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> RegisteredOwner { get; }
		/// <summary>
		/// The name of the state that the supplied location in in.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> State { get; }
		/// <summary>
		/// The offset from UTC in minutes in the supplied location, at the time that the value is produced.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> TimeZoneOffset { get; }
		/// <summary>
		/// The name of the town that the supplied location is in.
		/// </summary>
		IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> Town { get; }
	}
}
