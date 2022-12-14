using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace PKHeX.API.Models
{

	public static class TrainerInfoExtensions
	{
		public static PKM GetLegal(this ITrainerInfo sav, IBattleTemplate set, out string res)
		{
			var result = sav.GetLegalFromSet(set, out var type);
			res = type.ToString();
			return result;
		}
	}
}