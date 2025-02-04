// SPDX-License-Identifier: EUPL-1.2+
//
// Copyleft © 2024‒ Eliezer - https://github.com/eliezer73/PostalCodes_FI
// Licensed under the EUPL: https://eur-lex.europa.eu/eli/dec_impl/2017/863#d1e32-60-1

namespace PostalCodes_FI;

/// <summary>
/// Defines if the street address range is for the even or odd numbers
/// (street or road address numbers are always odd on one side and even on the opposite side).
/// </summary>
public enum Parity
{
	/// <summary>
	/// The range does not have street address numbers
	/// </summary>
	None = 0,
	/// <summary>
	/// The range defines odd street address numbers.
	/// </summary>
	Odd = 1,
	/// <summary>
	/// The range defines even street address numbers.
	/// </summary>
	Even = 2
}
