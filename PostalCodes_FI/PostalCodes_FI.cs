// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024 Eliezer - mailto:eliezer@programmer.net?subject=PostalCodes_FI
// https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://joinup.ec.europa.eu/licence/european-union-public-licence-version-12-or-later-eupl

using System.Text;

namespace PostalCodes_FI;

/// <summary>
/// Class for handling Finnish postal codes
/// </summary>
public class PostalCodes_FI
{
	/// <summary>
	/// Dictionary of postal code objects by postal code
	/// </summary>
	private static readonly Dictionary<string, PostalCodeLocation> _postalCodes = [];

	/// <summary>
	/// Dictionary of administrative regions by region code
	/// </summary>
	private static readonly Dictionary<string, AdministrativeRegion> _regions = [];

	/// <summary>
	/// Dictionary of municipalities by municipality code
	/// </summary>
	private static readonly Dictionary<string, Municipality> _municipalities = [];

	/// <summary>
	/// Get a collection of postal codes based on postal code files in the specified directory
	/// </summary>
	/// <param name="directory">The directory for the postal code files</param>
	/// <seealso cref="IOrderedEnumerable{TElement}"/>
	/// <seealso cref="PostalCodeLocation"/>
	/// <returns>IOrderedEnumerable{PostalCodeLocation}</returns>
	public static IOrderedEnumerable<PostalCodeLocation> GetPostalCodes(DirectoryInfo directory)
	{
		if (_postalCodes.Count == 0)
		{
			ReadPostalCodeFile(directory);
			ReadBasicAddressFile(directory);
		}
		return _postalCodes.Values
			.OrderBy(pc => pc.PrimaryMunicipality.AdministrativeRegion?.Code ?? "")
			.ThenBy(pc => pc.PrimaryMunicipality.Code)
			.ThenBy(pc => pc.Code);
	}

	/// <summary>
	/// Reads the latest postal code file (PCF_{date}.dat) from the given directory.
	/// </summary>
	/// <param name="directory">The directory for the postal code file.</param>
	private static void ReadPostalCodeFile(DirectoryInfo directory)
	{
		FileInfo? postalCodeFile = directory.GetFiles("PCF_*.dat").OrderByDescending(x => x.Name).FirstOrDefault();
		if (postalCodeFile != null)
		{
			using FileStream stream = File.OpenRead(postalCodeFile.FullName);
			using StreamReader reader = new(stream, Encoding.Latin1);
			AdministrativeRegion? currentRegion = null;
			Municipality? currentMunicipality = null;
			PostalCodeLocation? currentPostalCode = null;
			string? line;
			do
			{
				line = reader.ReadLine();
				if (!string.IsNullOrWhiteSpace(line) && line.Length == 220)
				{
					string recordIdentifier = line[..5];
					if (!recordIdentifier.Equals("PONOT"))
					{
						continue;
					}
					DateTime? runningDate = null;
					if (int.TryParse(line[5..9], out int year) && int.TryParse(line[9..11], out int month) && int.TryParse(line[11..13], out int dayOfMonth))
					{
						runningDate = new DateTime(year, month, dayOfMonth, 0, 0, 0, DateTimeKind.Local);
					}
					string administrativeRegionCode = line[111..116].Trim();
					if (currentRegion == null || currentRegion.Code != administrativeRegionCode)
					{
						if (_regions.TryGetValue(administrativeRegionCode, out AdministrativeRegion? value))
						{
							currentRegion = value;
						}
						else
						{
							string administrativeRegionNameInFinnish = line[116..146].Trim();
							string administrativeRegionNameInSwedish = line[146..176].Trim();
							currentRegion = new AdministrativeRegion(runningDate, administrativeRegionCode, administrativeRegionNameInFinnish, administrativeRegionNameInSwedish);
							_regions.Add(administrativeRegionCode, currentRegion);
						}
					}
					string municipalityCode = line[176..179];
					if (currentMunicipality == null || currentMunicipality.Code != municipalityCode)
					{
						if (_municipalities.TryGetValue(municipalityCode, out Municipality? value))
						{
							currentMunicipality = value;
						}
						else
						{
							string municipalityNameInFinnish = line[179..199].Trim();
							string municipalityNameInSwedish = line[199..219].Trim();
							LanguageDistributionCode? municipalityLanguageDistributionCode = null;
							if (int.TryParse(line[219..220], out int distributionCode))
							{
								municipalityLanguageDistributionCode = (LanguageDistributionCode) distributionCode;
							}
							currentMunicipality = new Municipality(runningDate, currentRegion, municipalityCode, municipalityNameInFinnish, municipalityNameInSwedish, municipalityLanguageDistributionCode);
							_municipalities.Add(municipalityCode, currentMunicipality);
						}
					}
					string postalCode = line[13..18];
					if (currentPostalCode == null || currentPostalCode.Code != postalCode)
					{
						if (_postalCodes.TryGetValue(postalCode, out PostalCodeLocation? value))
						{
							currentPostalCode = value;
						}
						else
						{
							string postalCodeNameInFinnish = line[18..48].Trim();
							string postalCodeNameInSwedish = line[48..78].Trim();
							string postalCodeNameAbbreviationInFinnish = line[78..90].Trim();
							string postalCodeNameAbbreviationInSwedish = line[90..102].Trim();
							DateTime? dateOfEntryIntoForce = null;
							if (int.TryParse(line[102..106], out year) && int.TryParse(line[106..108], out month) && int.TryParse(line[108..110], out dayOfMonth))
							{
								dateOfEntryIntoForce = new DateTime(year, month, dayOfMonth, 0, 0, 0, DateTimeKind.Local);
							}
							PostalCodeType? typeCode = null;
							if (int.TryParse(line[110..111], out int type))
							{
								typeCode = (PostalCodeType) type;
							}
							currentPostalCode = new PostalCodeLocation(runningDate, postalCode, postalCodeNameInFinnish, postalCodeNameInSwedish, postalCodeNameAbbreviationInFinnish, postalCodeNameAbbreviationInSwedish, dateOfEntryIntoForce, typeCode);
							_postalCodes.Add(postalCode, currentPostalCode);
						}
					}
					currentPostalCode.AddToMunicipality(currentMunicipality); // Safe to call, does not add duplicates
				}
			}
			while (!string.IsNullOrEmpty(line) && line.Length == 220);
		}
	}

	/// <summary>
	/// Reads the basic address file (BAF_{date}.dat) from the given directory.
	/// </summary>
	/// <param name="directory">The directory for the basic address file.</param>
	private static void ReadBasicAddressFile(DirectoryInfo directory)
	{
		FileInfo? basicAddressFile = directory.GetFiles("BAF_*.dat").OrderByDescending(x => x.Name).FirstOrDefault();
		if (basicAddressFile != null)
		{
			using FileStream stream = File.OpenRead(basicAddressFile.FullName);
			using StreamReader reader = new(stream, Encoding.Latin1);
			Municipality? currentMunicipality = null;
			PostalCodeLocation? currentPostalCode = null;
			string? line;
			do
			{
				line = reader.ReadLine();
				if (!string.IsNullOrWhiteSpace(line) && line.Length == 256)
				{
					string recordIdentifier = line[..5];
					if (!recordIdentifier.Equals("KATUN"))
					{
						continue;
					}
					DateTime? runningDate = null;
					if (int.TryParse(line[5..9], out int year) && int.TryParse(line[9..11], out int month) && int.TryParse(line[11..13], out int dayOfMonth))
					{
						runningDate = new DateTime(year, month, dayOfMonth, 0, 0, 0, DateTimeKind.Local);
					}
					string municipalityCode = line[213..216];
					string postalCode = line[13..18];
					if (currentMunicipality == null || currentMunicipality.Code != municipalityCode)
					{
						if (_municipalities.TryGetValue(municipalityCode, out Municipality? value))
						{
							currentMunicipality = value;
						}
						else
						{
							string municipalityNameInFinnish = line[216..236];
							string municipalityNameInSwedish = line[236..256];
							currentMunicipality = new Municipality(runningDate, null, municipalityCode, municipalityNameInFinnish, municipalityNameInSwedish, null);
							_municipalities.Add(municipalityCode, currentMunicipality);
						}
					}
					if (currentPostalCode == null || currentPostalCode.Code != postalCode)
					{
						if (_postalCodes.TryGetValue(postalCode, out PostalCodeLocation? value))
						{
							currentPostalCode = value;
						}
						else
						{
							string postalCodeNameInFinnish = line[18..48].Trim();
							string postalCodeNameInSwedish = line[48..78].Trim();
							string postalCodeNameAbbreviationInFinnish = line[78..90].Trim();
							string postalCodeNameAbbreviationInSwedish = line[90..102].Trim();
							DateTime? dateOfEntryIntoForce = null;
							if (int.TryParse(line[102..106], out year) && int.TryParse(line[106..108], out month) && int.TryParse(line[108..110], out dayOfMonth))
							{
								dateOfEntryIntoForce = new DateTime(year, month, dayOfMonth, 0, 0, 0, DateTimeKind.Local);
							}
							PostalCodeType? typeCode = null;
							if (int.TryParse(line[110..111], out int type))
							{
								typeCode = (PostalCodeType) type;
							}
							currentPostalCode = new PostalCodeLocation(runningDate, postalCode, postalCodeNameInFinnish, postalCodeNameInSwedish, postalCodeNameAbbreviationInFinnish, postalCodeNameAbbreviationInSwedish, dateOfEntryIntoForce, typeCode);
							_postalCodes.Add(postalCode, currentPostalCode);
						}
					}
					string streetOrLocationNameInFinnish = line[102..132].Trim();
					string streetOrLocationNameInSwedish = line[132..162].Trim();
					Parity buildingDataType;
					if (int.TryParse(line[186..186], out int parityCode) && parityCode >= 0 && parityCode <= 2)
					{
						buildingDataType = (Parity) parityCode;
					}
					else
					{
						buildingDataType = Parity.None;
					}
					string smallestBuildingNumber1 = line[187..192].Trim();
					string smallestBuildingNumberBuildingDeliveryLetter1 = line[192..192].Trim();
					string smallestBuildingNumberPunctuationMark = line[193..193].Trim();
					string smallestBuildingNumber2 = line[194..199].Trim();
					string smallestBuildingNumberBuildingDeliveryLetter2 = line[199..199].Trim();
					if (!uint.TryParse(smallestBuildingNumber1, out uint smallestBuildingNumber))
					{
						smallestBuildingNumber = 0;
					}
					string highestBuildingNumber1 = line[200..205].Trim();
					string highestBuildingNumberBuildingDeliveryLetter1 = line[205..205].Trim();
					string highestBuildingNumberPunctuationMark = line[206..206].Trim();
					string highestBuildingNumber2 = line[207..212].Trim();
					string highestBuildingNumberBuildingDeliveryLetter2 = line[212..212].Trim();
					uint highestBuildingNumber = 0;
					if (highestBuildingNumberPunctuationMark == "-" && !uint.TryParse(highestBuildingNumber2, out highestBuildingNumber))
					{
						highestBuildingNumber = 0;
					}
					if (highestBuildingNumber == 0 && !uint.TryParse(highestBuildingNumber1, out highestBuildingNumber))
					{
						highestBuildingNumber = 0;
					}
					string addressRangeStart = $"{smallestBuildingNumber1}{smallestBuildingNumberBuildingDeliveryLetter1}{smallestBuildingNumberPunctuationMark}{smallestBuildingNumber2}{smallestBuildingNumberBuildingDeliveryLetter2}";
					string addressRangeEnd = $"{highestBuildingNumber1}{highestBuildingNumberBuildingDeliveryLetter1}{highestBuildingNumberPunctuationMark}{highestBuildingNumber2}{highestBuildingNumberBuildingDeliveryLetter2}";
					string[] addressRange;
					if (string.IsNullOrWhiteSpace(addressRangeStart))
					{
						addressRange = [];
					}
					else if (string.IsNullOrWhiteSpace(addressRangeEnd) || addressRangeEnd.Equals(addressRangeStart))
					{
						addressRange = [addressRangeStart];
					}
					else
					{
						addressRange = [addressRangeStart, addressRangeEnd];
					}
					currentPostalCode.AddStreetAddressRange(runningDate, currentMunicipality,
						streetOrLocationNameInFinnish, streetOrLocationNameInSwedish,
						addressRange, buildingDataType, smallestBuildingNumber, highestBuildingNumber);
				}
			}
			while (!string.IsNullOrEmpty(line) && line.Length == 256);
		}
	}
}
