using PKHeX.Core;
using PKHeX.API.Services;

namespace PKHeX.API.Models
{

	public static class PkmExtensions
	{
		public static bool IsLegal(this PKM pkm) => pkm.CanBeTraded();

		private static bool CanBeTraded(this PKM pkm) => !FormInfo.IsFusedForm(pkm.Species, pkm.Form, pkm.Format);
	}
}