using System;
using System.Threading.Tasks;
using PKHeX.Core;
using System.Collections.Generic;

namespace PKHeX.API.Services
{

	public class PKXService
	{

		public Task<Dictionary<string, string>> GetInfoFromFormFile(PKM pkm)
		{

			var pokemonsummary = new Dictionary<string, string>();

			pokemonsummary.Add("Species", ((Species)pkm.Species).ToString());
			pokemonsummary.Add("Dex_Number", pkm.Species.ToString());
			pokemonsummary.Add("Nickname", pkm.Nickname.ToString());
			pokemonsummary.Add("HeldItem", pkm.HeldItem.ToString());
			pokemonsummary.Add("Gender", ((Gender)pkm.Gender).ToString());
			pokemonsummary.Add("Nature", ((Nature)pkm.Nature).ToString());
			pokemonsummary.Add("StatNature", pkm.StatNature.ToString());
			pokemonsummary.Add("Ability", ((Ability)pkm.Ability).ToString());
			pokemonsummary.Add("CurrentFriendship", pkm.CurrentFriendship.ToString());
			pokemonsummary.Add("Form ", pkm.Form.ToString());
			pokemonsummary.Add("IsEgg", pkm.IsEgg.ToString());
			pokemonsummary.Add("IsNicknamed", pkm.IsNicknamed.ToString());
			pokemonsummary.Add("EXP", pkm.EXP.ToString());
			pokemonsummary.Add("TID", pkm.TID.ToString());
			pokemonsummary.Add("OT_Name", pkm.OT_Name.ToString());
			pokemonsummary.Add("OT_Gender", pkm.OT_Gender.ToString());
			pokemonsummary.Add("Ball", ((Ball)pkm.Ball).ToString());
			pokemonsummary.Add("Met_Level", pkm.Met_Level.ToString());
			pokemonsummary.Add("Move1", ((Move)pkm.Move1).ToString());
			pokemonsummary.Add("Move2", ((Move)pkm.Move2).ToString());
			pokemonsummary.Add("Move3", ((Move)pkm.Move3).ToString());
			pokemonsummary.Add("Move4", ((Move)pkm.Move4).ToString());
			pokemonsummary.Add("Move1_PP", pkm.Move1_PP.ToString());
			pokemonsummary.Add("Move2_PP ", pkm.Move2_PP.ToString());
			pokemonsummary.Add("Move3_PP", pkm.Move3_PP.ToString());
			pokemonsummary.Add("Move4_PP ", pkm.Move4_PP.ToString());
			pokemonsummary.Add("Move1_PPUps", pkm.Move1_PPUps.ToString());
			pokemonsummary.Add("Move2_PPUps", pkm.Move2_PPUps.ToString());
			pokemonsummary.Add("Move3_PPUps", pkm.Move3_PPUps.ToString());
			pokemonsummary.Add("Move4_PPUps", pkm.Move4_PPUps.ToString());
			pokemonsummary.Add("EV_HP", pkm.EV_HP.ToString());
			pokemonsummary.Add("EV_ATK", pkm.EV_ATK.ToString());
			pokemonsummary.Add("EV_DEF", pkm.EV_DEF.ToString());
			pokemonsummary.Add("EV_SPE", pkm.EV_SPE.ToString());
			pokemonsummary.Add("EV_SPA", pkm.EV_SPA.ToString());
			pokemonsummary.Add("EV_SPD", pkm.EV_SPD.ToString());
			pokemonsummary.Add("IV_HP", pkm.IV_HP.ToString());
			pokemonsummary.Add("IV_ATK", pkm.IV_ATK.ToString());
			pokemonsummary.Add("IV_DEF", pkm.IV_DEF.ToString());
			pokemonsummary.Add("IV_SPE", pkm.IV_SPE.ToString());
			pokemonsummary.Add("IV_SPA", pkm.IV_SPA.ToString());
			pokemonsummary.Add("IV_SPD", pkm.IV_SPD.ToString());
			pokemonsummary.Add("Status_Condition", pkm.Status_Condition.ToString());
			pokemonsummary.Add("Stat_Level", pkm.Stat_Level.ToString());
			pokemonsummary.Add("Stat_HPMax", pkm.Stat_HPMax.ToString());
			pokemonsummary.Add("Stat_HPCurrent", pkm.Stat_HPCurrent.ToString());
			pokemonsummary.Add("Stat_ATK", pkm.Stat_ATK.ToString());
			pokemonsummary.Add("Stat_DEF", pkm.Stat_DEF.ToString());
			pokemonsummary.Add("Stat_SPE", pkm.Stat_SPE.ToString());
			pokemonsummary.Add("Stat_SPA", pkm.Stat_SPA.ToString());
			pokemonsummary.Add("Stat_SPD", pkm.Stat_SPD.ToString());
			pokemonsummary.Add("Version", pkm.Version.ToString());
			pokemonsummary.Add("SID", pkm.SID.ToString());
			pokemonsummary.Add("PKRS_Strain", pkm.PKRS_Strain.ToString());
			pokemonsummary.Add("PKRS_Days", pkm.PKRS_Days.ToString());
			pokemonsummary.Add("EncryptionConstant", pkm.EncryptionConstant.ToString());
			pokemonsummary.Add("PID", pkm.PID.ToString());
			pokemonsummary.Add("Language", ((LanguageID)pkm.Language).ToString());
			pokemonsummary.Add("FatefulEncounter", pkm.FatefulEncounter.ToString());
			pokemonsummary.Add("TSV", pkm.TSV.ToString());
			pokemonsummary.Add("PSV", pkm.PSV.ToString());
			pokemonsummary.Add("Characteristic", pkm.Characteristic.ToString());
			pokemonsummary.Add("MarkValue", pkm.MarkValue.ToString());
			pokemonsummary.Add("Met_Location", pkm.Met_Location.ToString());
			pokemonsummary.Add("Egg_Location", pkm.Egg_Location.ToString());
			pokemonsummary.Add("OT_Friendship", pkm.OT_Friendship.ToString());

            Task<Dictionary<string, string>> PokemonSummaryTask = Task.FromResult(pokemonsummary);
            return PokemonSummaryTask;
		}
	}
}