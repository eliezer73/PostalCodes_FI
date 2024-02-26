// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024 Eliezer - mailto:eliezer@programmer.net?subject=PostalCodes_FI
// https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://joinup.ec.europa.eu/licence/european-union-public-licence-version-12-or-later-eupl

namespace PostalCodes_FI;

/// <summary>
/// Finnish administrative region (NUTS 2012 level 3) information.
/// </summary>
/// <param name="runningDate">The data update date.</param>
/// <param name="code">The NUTS 3 code for the region.</param>
/// <param name="nameInFinnish">The region name in Finnish.</param>
/// <param name="nameInSwedish">The region name in Swedish.</param>
public class AdministrativeRegion(DateTime? runningDate, string code, string nameInFinnish, string nameInSwedish)
{
	/// <summary>
	/// The data update date
	/// </summary>
	public DateTime? RunningDate { get; set; } = runningDate;
	/// <summary>
	/// The NUTS 3 code for the region
	/// </summary>
	public string Code { get; set; } = code;
	/// <summary>
	/// The region name in Finnish
	/// </summary>
	public string NameInFinnish { get; set; } = nameInFinnish;
	/// <summary>
	/// The region name in Swedish
	/// </summary>
	public string NameInSwedish { get; set; } = nameInSwedish;
}
