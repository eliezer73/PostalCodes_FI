// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024 Eliezer - mailto:eliezer@programmer.net?subject=PostalCodes_FI
// https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://joinup.ec.europa.eu/licence/european-union-public-licence-version-12-or-later-eupl

namespace PostalCodes_FI;

/// <summary>
/// Finnish postal code location information
/// </summary>
/// <param name="runningDate">The data update date.</param>
/// <param name="code">The postal code - 5 digits, may have zeros in the beginning.</param>
/// <param name="postalCodeNameInFinnish">The postal code location name in Finnish.</param>
/// <param name="postalCodeNameInSwedish">The postal code location name in Swedish.</param>
/// <param name="postalCodeNameAbbreviationInFinnish">The postal code location abbreviation in Finnish.</param>
/// <param name="postalCodeNameAbbreviationInSwedish">The postal code location abbreviation in Swedish.</param>
/// <param name="dateOfEntryIntoForce">The date this postal code was fist taken into use.</param>
/// <param name="typeCode">Type of the postal code.</param>
public class PostalCodeLocation(DateTime? runningDate,
								string code,
								string postalCodeNameInFinnish,
								string postalCodeNameInSwedish,
								string postalCodeNameAbbreviationInFinnish,
								string postalCodeNameAbbreviationInSwedish,
								DateTime? dateOfEntryIntoForce,
								PostalCodeType? typeCode)
{
	/// <summary>
	/// The data update date
	/// </summary>
	public DateTime? RunningDate { get; set; } = runningDate;
	/// <summary>
	/// The postal code - 5 digits, may have zeros in the beginning
	/// </summary>
	public string Code { get; set; } = code;
	/// <summary>
	/// The postal code location name in Finnish
	/// </summary>
	public string PostalLocationNameInFinnish { get; set; } = postalCodeNameInFinnish;
	/// <summary>
	/// The postal code location name in Swedish
	/// </summary>
	public string PostalLocationNameInSwedish { get; set; } = postalCodeNameInSwedish;
	/// <summary>
	/// The postal code location abbreviation in Finnish
	/// </summary>
	public string PostalLocationNameAbbreviationInFinnish { get; set; } = postalCodeNameAbbreviationInFinnish;
	/// <summary>
	/// The postal code location abbreviation in Swedish
	/// </summary>
	public string PostalLocationNameAbbreviationInSwedish { get; set; } = postalCodeNameAbbreviationInSwedish;
	/// <summary>
	/// The date this postal code was fist taken into use
	/// </summary>
	public DateTime? DateOfEntryIntoForce { get; set; } = dateOfEntryIntoForce;
	/// <summary>
	/// Type of the postal code
	/// </summary>
	public PostalCodeType? TypeCode { get; set; } = typeCode;
	/// <summary>
	/// All the street addresses in the postal code within the given municipality
	/// (postal code area may cover parts of multiple municipalities)
	/// </summary>
	public Dictionary<string, List<StreetAddressRange>> StreetAddressesByMunicipalityCode { get; private set; } = [];
	/// <summary>
	/// The number of street addresses of this postal code within the given municipality
	/// (for estimating where this postal code is most used)
	/// </summary>
	public Dictionary<string, int> NumberOfStreetAddressesByMunicipalityCode { get; set; } = [];

	private readonly Dictionary<string, Municipality> _municipalityByCode = [];

	private Municipality? _primaryMunicipality = null;
	/// <summary>
	/// The municipality this postal code is primarily for
	/// (postal code area may cover parts of multiple municipalities)
	/// </summary>
	public Municipality PrimaryMunicipality
	{
		get
		{
			this._primaryMunicipality ??= this.GetMunicipalities().First();
			return this._primaryMunicipality;
		}
	}

	/// <summary>
	/// Add this postal code to a municipality 
	/// </summary>
	/// <param name="municipality">The municipality to add postal code to.</param>
	public void AddToMunicipality(Municipality municipality)
	{
		this._municipalityByCode.TryAdd(municipality.Code, municipality);
		this.StreetAddressesByMunicipalityCode.TryAdd(municipality.Code, []);
		this.NumberOfStreetAddressesByMunicipalityCode.TryAdd(municipality.Code, 0);
	}

	/// <summary>
	/// Checks if either municipality name starts with postal code location name
	/// or postal code location name starts with municipality name. Checks in both Finnish and Swedish.
	/// </summary>
	/// <param name="municipality">The municipality to compare to.</param>
	/// <returns><c>true</c>, if names are similar, otherwise <c>false</c>.</returns>
	public bool IsNameSimilarToMunicipalityName(Municipality municipality)
	{
		// Add space after name for comparison so "Vihtijärvi" will not match "Vihti" but "Laukaa As" will match "Laukaa"
		string municipalityFin = $"{municipality.NameInFinnish} ";
		string municipalitySwe = $"{municipality.NameInSwedish} ";
		string postalLocFin = $"{this.PostalLocationNameInFinnish} ";
		string postalLocSwe = $"{this.PostalLocationNameInSwedish} ";

		return (!string.IsNullOrWhiteSpace(municipalityFin) && !string.IsNullOrWhiteSpace(postalLocFin)
					&& (municipalityFin.StartsWith(postalLocFin, StringComparison.CurrentCultureIgnoreCase)
						|| postalLocFin.StartsWith(municipalityFin, StringComparison.CurrentCultureIgnoreCase)))
				|| (!string.IsNullOrWhiteSpace(municipalitySwe) && !string.IsNullOrWhiteSpace(postalLocSwe)
					&& (municipalitySwe.StartsWith(postalLocSwe, StringComparison.CurrentCultureIgnoreCase)
						|| postalLocSwe.StartsWith(municipalitySwe, StringComparison.CurrentCultureIgnoreCase)));
	}

	/// <summary>
	/// Gets the municipalities for this postal code - in descending order by number of potential addresses for the postal code within the municipality
	/// </summary>
	/// <returns>IEnumerable{Municipality}</returns>
	public IEnumerable<Municipality> GetMunicipalities()
	{
		return this._municipalityByCode
			.OrderByDescending(m => this.IsNameSimilarToMunicipalityName(m.Value))
			.ThenByDescending(m => this.NumberOfStreetAddressesByMunicipalityCode[m.Key])
			.Select(m => m.Value);
	}

	/// <summary>
	/// Adds a street address range to this postal code.
	/// </summary>
	/// <param name="runningDate">The data update date.</param>
	/// <param name="municipality">The municipality this street address range is within.</param>
	/// <param name="streetOrLocationNameInFinnish">The street/location name in Finnish.</param>
	/// <param name="streetOrLocationNameInSwedish">The street/location name in Swedish.</param>
	/// <param name="addressRange">The street address range begin and end numbers & possibly letters.</param>
	/// <param name="buildingDataType">Defines if this address range is for even or odd numeric street addresses - or None if this address does not cover any numbers.</param>
	/// <param name="smallestBuildingNumber">The smallest street address building number in this range.</param>
	/// <param name="highestBuildingNumber">The highest street address building number in this range.</param>
	public void AddStreetAddressRange(DateTime? runningDate,
									  Municipality municipality,
									  string streetOrLocationNameInFinnish,
									  string streetOrLocationNameInSwedish,
									  string[] addressRange,
									  Parity buildingDataType,
									  uint smallestBuildingNumber,
									  uint highestBuildingNumber)
	{
		this.AddToMunicipality(municipality);
		string code = municipality.Code;
		var streetAddressRange = new StreetAddressRange(runningDate, this, municipality, streetOrLocationNameInFinnish,
			streetOrLocationNameInSwedish, addressRange, buildingDataType, smallestBuildingNumber, highestBuildingNumber);
		this.StreetAddressesByMunicipalityCode[code].Add(streetAddressRange);
		this.NumberOfStreetAddressesByMunicipalityCode[code] += (int) streetAddressRange.PotentialNumberOfStreetAddressNumbers;
	}
}
