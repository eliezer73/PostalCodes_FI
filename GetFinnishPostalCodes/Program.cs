// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024 Eliezer - mailto:eliezer@programmer.net?subject=PostalCodes_FI
// https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://joinup.ec.europa.eu/licence/european-union-public-licence-version-12-or-later-eupl

using PostalCodes_FI;
using System.Globalization;

namespace GetFinnishPostalCodes;

/// <summary>
/// A wrapper program for reading Finnish postal codes from <see cref="PostalCodes_FI"/> and printing them out to console.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry of the program
    /// </summary>
    /// <param name="args">The command line arguments:
    /// /includespecial or --includespecial - include all types of postal codes - if not specified, only the "normal" postal
    /// codes for geographical areas are included and all the special codes (like corporate or postal box codes)
    /// are excluded
    /// </param>
    public static void Main(string[] args)
    {
        string directoryName = "";
        bool includeSpecialPostalCodes = false;
        foreach (string arg in args)
        {
            if (arg.StartsWith('/') || arg.StartsWith("--"))
            {
                if (arg.Trim('/', '-').Equals("includespecial", StringComparison.InvariantCultureIgnoreCase))
                {
                    includeSpecialPostalCodes = true;
                }
            }
            else if (string.IsNullOrWhiteSpace(directoryName))
            {
                directoryName = arg;
            }
            
        }

        DirectoryInfo directory = new(args.Length > 0 ? args[0] : "../../../../../PostalCodes_FI/data/");
        List<PostalCodeLocation> result = [.. PostalCodes_FI.PostalCodes_FI.GetPostalCodes(directory)];
        Dictionary<string, AdministrativeRegion> regionByCode = [];
        Dictionary<string, AdministrativeRegion> regionByMunicipalityCode = [];
        Dictionary<string, Municipality> municipalityByCode = [];
        Dictionary<string, List<string>> municipalityCodesByRegionCode = [];
        Dictionary<string, PostalCodeLocation> postalLocationByPostalCode = [];
        Dictionary<string, List<string>> postalCodesByMunicipalityCode = [];
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        try
        {
            // Temporarily switch to Finnish culture for sorting
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fi-FI");
            // For some reason a few special postal codes like "Eduskunta" and "Korvatunturi" are
            // listed in normal post codes. They can be excluded by their ending (2 and 9) as
            // most of the normal postal codes end with 0, some with 5 and a few with 7.
            foreach (PostalCodeLocation postalLocation in result.Where(pl => includeSpecialPostalCodes
                || (pl.TypeCode == PostalCodeType.Normal
                    && (pl.Code.EndsWith('0')
                        || pl.Code.EndsWith('5')
                        || pl.Code.EndsWith('7')))))
            {
                string postalCode = postalLocation.Code;
                postalLocationByPostalCode.Add(postalCode, postalLocation);
                foreach (Municipality municipality in postalLocation.GetMunicipalities())
                {
                    string municipalityCode = municipality.Code;
                    municipalityByCode.TryAdd(municipalityCode, municipality);
                    if (!postalCodesByMunicipalityCode.TryAdd(municipalityCode, [postalCode])
                        && !postalCodesByMunicipalityCode[municipalityCode].Contains(postalCode))
                    {
                        postalCodesByMunicipalityCode[municipalityCode].Add(postalCode);
                    }
                    AdministrativeRegion? region = municipality.AdministrativeRegion;
                    if (region == null)
                    {
                        regionByMunicipalityCode.TryGetValue(municipalityCode, out region);
                    }
                    string regionCode = region?.Code ?? "_NO_REGION";
                    if (region != null && regionByMunicipalityCode.TryAdd(municipalityCode, region))
                    {
                        if (municipalityCodesByRegionCode.TryGetValue("_NO_REGION", out List<string>? municipalitiesWithoutRegionInfo))
                        {
                            municipalitiesWithoutRegionInfo.Remove(municipalityCode);
                        }
                        regionByCode.TryAdd(regionCode, region);
                        if (!municipalityCodesByRegionCode.TryAdd(regionCode, [municipalityCode])
                            && !municipalityCodesByRegionCode[regionCode].Contains(municipalityCode))
                        {
                            municipalityCodesByRegionCode[regionCode].Add(municipalityCode);
                        }
                    }
                }
            }
            bool isFirstRegion = true;
            foreach (AdministrativeRegion region in regionByCode.OrderBy(r => r.Key).Select(r => r.Value))
            {
                if (!isFirstRegion)
                {
                    Console.WriteLine();
                }
                string regionName = region.NameInFinnish;
                if (!regionName.Equals(region.NameInSwedish))
                {
                    regionName += $" - {region.NameInSwedish}";
                }
                regionName += $" [{region.Code}]";
                Console.WriteLine(regionName);
                for (int i = 0; i < regionName.Length; i++)
                {
                    Console.Write('=');
                }
                Console.WriteLine();
                bool isFirstMunicipality = true;
                foreach (Municipality municipality in municipalityCodesByRegionCode[region.Code]
                    .Select(c => municipalityByCode[c])
                    .OrderBy(m => m.IsFinnishMajority
                        ? m.NameInFinnish
                        : m.NameInSwedish))
                {
                    if (!isFirstMunicipality)
                    {
                        Console.WriteLine();
                    }
                    string municipalityName;
                    if (municipality.NameInFinnish.Equals(municipality.NameInSwedish)
                        || string.IsNullOrWhiteSpace(municipality.NameInSwedish))
                    {
                        municipalityName = municipality.NameInFinnish;
                    }
                    else if (string.IsNullOrWhiteSpace(municipality.NameInFinnish))
                    {
                        municipalityName = municipality.NameInSwedish;
                    }
                    else if (municipality.IsFinnishMajority)
                    {
                        municipalityName = $"{municipality.NameInFinnish} - {municipality.NameInSwedish}";
                    }
                    else
                    {
                        municipalityName = $"{municipality.NameInSwedish} - {municipality.NameInFinnish}";
                    }
                    municipalityName += $" [{municipality.Code}]:";
                    Console.WriteLine(municipalityName);
                    foreach (PostalCodeLocation postalLocation in postalCodesByMunicipalityCode[municipality.Code]
                        .Select(c => postalLocationByPostalCode[c])
                        .OrderByDescending(pl => pl.PrimaryMunicipality.Code.Equals(municipality.Code))
                        .ThenByDescending(pl => pl.IsNameSimilarToMunicipalityName(municipality))
                        .ThenByDescending(pl => pl.NumberOfStreetAddressesByMunicipalityCode[municipality.Code])
                        .ThenBy(pl => pl.Code))
                    {
                        string postalLocationName;
                        if (postalLocation.PostalLocationNameInFinnish.Equals(postalLocation.PostalLocationNameInSwedish)
                            || string.IsNullOrWhiteSpace(postalLocation.PostalLocationNameInSwedish))
                        {
                            postalLocationName = $"  {postalLocation.Code} {postalLocation.PostalLocationNameInFinnish}";
                        }
                        else if (string.IsNullOrWhiteSpace(postalLocation.PostalLocationNameInFinnish))
                        {
                            postalLocationName = $"  {postalLocation.Code} {postalLocation.PostalLocationNameInSwedish}";
                        }
                        else if (postalLocation.PrimaryMunicipality.IsFinnishMajority)
                        {
                            postalLocationName = $"  {postalLocation.Code} {postalLocation.PostalLocationNameInFinnish} - {postalLocation.PostalLocationNameInSwedish}";
                        }
                        else
                        {
                            postalLocationName = $"  {postalLocation.Code} {postalLocation.PostalLocationNameInSwedish} - {postalLocation.PostalLocationNameInFinnish}";
                        }
                        int numberOfAddresses = postalLocation.NumberOfStreetAddressesByMunicipalityCode[municipality.Code];
                        if (numberOfAddresses > 0)
                        {
                            postalLocationName += $" ({postalLocation.NumberOfStreetAddressesByMunicipalityCode[municipality.Code]})";
                        }
                        Console.WriteLine(postalLocationName);
                    }
                    isFirstMunicipality = false;
                }
                isFirstRegion = false;
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
