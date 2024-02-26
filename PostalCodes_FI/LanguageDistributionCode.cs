// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024 Eliezer - mailto:eliezer@programmer.net?subject=PostalCodes_FI
// https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://joinup.ec.europa.eu/licence/european-union-public-licence-version-12-or-later-eupl

namespace PostalCodes_FI;

/// <summary>
/// Code defining the official languages of the municipality
/// </summary>
public enum LanguageDistributionCode
{
	/// <summary>
	/// Finnish as the only official language (of the national official languages;
	/// some municipalities in Lapland have 1 - 3 Sami languages in addition to Finnish as the official regional language.
	/// Data for this is not included in the postal location files, but may be retrieved from other sources.)
	/// </summary>
	Finnish = 1,
	/// <summary>
	/// Both Finnish and Swedish as official languages. (Currently in either preference as postal location files do not
	/// support the preference order of the languages.)
	/// </summary>
	Bilingual = 2,
	/// <summary>
	/// Both Swedish and Finnish as official languages. (Postal location files currently don't use this value -
	/// they do not support defining the order of the official languages, but it may be retrieved from other sources.)
	/// </summary>
	Bilingual2 = 3,
	/// <summary>
	/// Swedish as the only official language
	/// </summary>
	Swedish = 4
}
