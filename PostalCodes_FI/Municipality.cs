// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024 Eliezer - mailto:eliezer@programmer.net?subject=PostalCodes_FI
// https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://joinup.ec.europa.eu/licence/european-union-public-licence-version-12-or-later-eupl

namespace PostalCodes_FI;

/// <summary>
/// Finnish municipality information
/// </summary>
/// <param name="runningDate">The data update date.</param>
/// <param name="administrativeRegion">The administrative region the municipality belongs to.</param>
/// <param name="code">The municipality code (maintained by Digital and population data services agency).</param>
/// <param name="nameInFinnish">The municipality name in Finnish.</param>
/// <param name="nameInSwedish">The municipality name in Swedish.</param>
/// <param name="languageDistribution">Defines, which of the official languages of Finland are official language(s) in the municipality.</param>
public class Municipality(DateTime? runningDate, AdministrativeRegion? administrativeRegion, string code, string nameInFinnish, string nameInSwedish, LanguageDistributionCode? languageDistribution)
{
	/// <summary>
	/// The data update date
	/// </summary>
	public DateTime? RunningDate { get; set; } = runningDate;
	/// <summary>
	/// The administrative region the municipality belongs to
	/// </summary>
	public AdministrativeRegion? AdministrativeRegion { get; set; } = administrativeRegion;
	/// <summary>
	/// The municipality code (maintained by Digital and population data services agency)
	/// </summary>
	public string Code { get; set; } = code;
	/// <summary>
	/// The municipality name in Finnish
	/// </summary>
	public string NameInFinnish { get; set; } = nameInFinnish;
	/// <summary>
	/// The municipality name in Swedish
	/// </summary>
	public string NameInSwedish { get; set; } = nameInSwedish;
	/// <summary>
	/// Defines, which of the official languages of Finland are official language(s) in the municipality
	/// </summary>
	private readonly LanguageDistributionCode? _languageDistribution = languageDistribution;
	/// <summary>
	/// Defines, should Finnish name be displayed before Swedish name.
	/// NB! The information is currently insufficient for bilingual municipalities as postal location data does
	/// not specify the order of the languages in bilingual municipalities. TODO for future development.
	/// </summary>
	public bool IsFinnishMajority
	{
		get
		{
			return !this._languageDistribution.HasValue
				|| this._languageDistribution.Value == LanguageDistributionCode.Finnish
				|| this._languageDistribution.Value == LanguageDistributionCode.Bilingual;
		}
	}
}
