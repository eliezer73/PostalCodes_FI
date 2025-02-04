// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024‒ Eliezer - https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://eur-lex.europa.eu/eli/dec_impl/2017/863#d1e32-60-1

namespace PostalCodes_FI;

/// <summary>
/// Type of the postal code
/// </summary>
public enum PostalCodeType
{
	/// <summary>
	/// Normal postal code (covering a physical area on a map)
	/// </summary>
	Normal = 1,
	/// <summary>
	/// Post office box
	/// </summary>
	POBox = 2,
	/// <summary>
	/// Corporate postal code
	/// </summary>
	Corporate = 3,
	/// <summary>
	/// Compilation
	/// </summary>
	Compilation = 4,
	/// <summary>
	/// Reply mail
	/// </summary>
	ReplyMail = 5,
	/// <summary>
	/// SmartPOST parcel machine
	/// </summary>
	SmartPOSTParcelMachine = 6,
	/// <summary>
	/// Pick-up point
	/// </summary>
	PickUpPoint = 7,
	/// <summary>
	/// Technical code
	/// </summary>
	Technical = 8
}
