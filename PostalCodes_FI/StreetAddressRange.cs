// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024 Eliezer - mailto:eliezer@programmer.net?subject=PostalCodes_FI
// https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://joinup.ec.europa.eu/licence/european-union-public-licence-version-12-or-later-eupl

namespace PostalCodes_FI;

/// <summary>
/// Defines a single range of even or odd numbers on a street (or a location name without street number)
/// </summary>
public class StreetAddressRange
{
	/// <summary>
	/// The data update date
	/// </summary>
	public DateTime? RunningDate { get; }
	/// <summary>
	/// The postal code this street address range belongs to
	/// </summary>
	public PostalCodeLocation PostalCode { get; }
	/// <summary>
	/// The municipality this street address range is within
	/// </summary>
	public Municipality Municipality { get; }
	/// <summary>
	/// The street/location name in Finnish
	/// </summary>
	public string StreetOrLocationNameInFinnish { get; }
	/// <summary>
	/// The street/location name in Swedish
	/// </summary>
	public string StreetOrLocationNameInSwedish { get; }
	/// <summary>
	/// The street address range begin and end numbers & possibly letters
	/// </summary>
	public string[]? AddressRange { get; }
	/// <summary>
	/// Defines if this address range is for even or odd numeric street addresses - or None if this address does not cover any numbers
	/// </summary>
	public Parity Parity { get; }
	/// <summary>
	/// The smallest street address building number in this range
	/// </summary>
	public uint SmallestBuildingNumber { get; private set; }
	/// <summary>
	/// The highest street address building number in this range
	/// </summary>
	public uint HighestBuildingNumber { get; private set; }
	/// <summary>
	/// The number of different street building addresses possibly within the range (only an estimate as outside city
	/// suburbs the number may be defined by a certain distance from the road start depending on how dense population
	/// is in the area)
	/// </summary>
	public uint PotentialNumberOfStreetAddressNumbers { get; private set; }
	/// <summary>
	/// Creates a new instance of StreetAddressRange
	/// </summary>
	/// <param name="runningDate">The data update date.</param>
	/// <param name="postalCode">The postal code this street address range belongs to.</param>
	/// <param name="municipality">The municipality this street address range is within.</param>
	/// <param name="streetOrLocationNameInFinnish">The street/location name in Finnish.</param>
	/// <param name="streetOrLocationNameInSwedish">The street/location name in Swedish.</param>
	/// <param name="addressRange">The street address range begin and end numbers & possibly letters.</param>
	/// <param name="parity">Defines if this address range is for even or odd numeric street addresses - or None if this address does not cover any numbers.</param>
	/// <param name="smallestBuildingNumber">The smallest street address building number in this range.</param>
	/// <param name="highestBuildingNumber">The highest street address building number in this range.</param>
	public StreetAddressRange(DateTime? runningDate,
							  PostalCodeLocation postalCode,
							  Municipality municipality,
							  string streetOrLocationNameInFinnish,
							  string streetOrLocationNameInSwedish,
							  string[] addressRange,
							  Parity parity,
							  uint smallestBuildingNumber,
							  uint highestBuildingNumber)
	{
		this.RunningDate = runningDate;
		this.PostalCode = postalCode;
		this.Municipality = municipality;
		this.StreetOrLocationNameInFinnish = streetOrLocationNameInFinnish;
		this.StreetOrLocationNameInSwedish = streetOrLocationNameInSwedish;
		this.AddressRange = addressRange;
		this.Parity = parity;
		this.SmallestBuildingNumber = smallestBuildingNumber;
		this.HighestBuildingNumber = highestBuildingNumber;

		uint numberOfBuildingNumbersInRange;
		// If the numbers should be odd numbers, check if they are actually even
		// or if they should be even, check if they are actually odd.
		if (this.Parity == Parity.Odd && this.SmallestBuildingNumber % 2 == 0
			|| this.Parity == Parity.Even && this.SmallestBuildingNumber % 2 == 1)
		{
			if (this.HighestBuildingNumber > this.SmallestBuildingNumber)
			{
				// First possible building number is actually one above the smallest building number; correct this:
				this.SmallestBuildingNumber += 1;
			}
			else
			{
				// No potential building numbers in range
				this.SmallestBuildingNumber = 0;
				this.HighestBuildingNumber = 0;
			}
		}
		if (this.HighestBuildingNumber > this.SmallestBuildingNumber
			&& (this.Parity == Parity.Odd && this.HighestBuildingNumber % 2 == 0
				|| this.Parity == Parity.Even && this.HighestBuildingNumber % 2 == 1))
		{
			// Last possible building number is actually one below the highest building number; correct this:
			this.HighestBuildingNumber -= 1;
		}
		if (this.HighestBuildingNumber == this.SmallestBuildingNumber || this.HighestBuildingNumber == 0)
		{
			numberOfBuildingNumbersInRange = (uint) (this.SmallestBuildingNumber > 0
				? 1 : 0);
		}
		else if (this.HighestBuildingNumber > this.SmallestBuildingNumber)
		{
			numberOfBuildingNumbersInRange = ((this.HighestBuildingNumber - this.SmallestBuildingNumber) / 2) + 1;
		}
		else
		{
			numberOfBuildingNumbersInRange = 0;
		}
		if (numberOfBuildingNumbersInRange == 0)
		{
			// If there is a location or street name defined, consider that the one single address available
			this.PotentialNumberOfStreetAddressNumbers =
				(uint) (string.IsNullOrWhiteSpace(this.StreetOrLocationNameInFinnish)
					&& string.IsNullOrWhiteSpace(this.StreetOrLocationNameInSwedish)
				? 0
				: 1);
		}
		else
		{
			this.PotentialNumberOfStreetAddressNumbers = numberOfBuildingNumbersInRange;
		}
	}
}
