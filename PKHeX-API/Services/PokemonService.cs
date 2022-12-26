using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using PKHeX.API.Exceptions;
using PKHeX.API.Models;
using static PKHeX.API.Util.Util;

namespace PKHeX.API.Services
{

	public class PokemonService
	{
		private AutoLegalityModService AutoLegalityModService { get; }

		public PokemonService(AutoLegalityModService autoLegalityModService)
			=> AutoLegalityModService = autoLegalityModService;

		public async Task<PKM> GetPokemonFromFormFileAsync(IFormFile file, SupportedGame game)
		{
			await using var stream = new MemoryStream();
			await file.CopyToAsync(stream);

			var pokemon = EntityFormat.GetFromBytes(stream.GetBuffer());

			if (pokemon == null) throw new BadRequestException("Couldn't parse provided file to any possible pokemon save file format.");

			var correctGame = game switch
			{
				SupportedGame.RBY => pokemon is PK1,
				SupportedGame.GSC => pokemon is PK2,
				SupportedGame.RS => pokemon is PK3,
				SupportedGame.PT => pokemon is PK4,
				SupportedGame.B2W2 => pokemon is PK5,
				SupportedGame.ORAS => pokemon is PK6,
				SupportedGame.USUM => pokemon is PK7,
				SupportedGame.LGPE => pokemon is PB7,
				SupportedGame.SWSH => pokemon is PK8,
				SupportedGame.BDSP => pokemon is PB8,
				SupportedGame.PLA => pokemon is PA8,
				SupportedGame.SV => pokemon is PK9,
				_ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
			};

			if (!correctGame) throw new LegalityException("Invalid Game Version");

			return pokemon;
		}

		public Task<PKM> GetPokemonFromShowdown(string showdownSet, SupportedGame game)
		{
			var set = new ShowdownSet(showdownSet);
			var template = new RegenTemplate(set);
			var ot = GetEnvOrThrow("PKHEX_DEFAULT_OT");

			var sav = game switch
			{
				// GetBlankSAV takes at least 1 arg but you need to define OT in request for gen 1/2
				SupportedGame.RBY => SaveUtil.GetBlankSAV(GameVersion.RBY, "PKHXAPI"),
				SupportedGame.GSC => SaveUtil.GetBlankSAV(GameVersion.C, "PKHXAPI"),
				SupportedGame.RS => SaveUtil.GetBlankSAV(GameVersion.RS, ot),
				SupportedGame.PT => SaveUtil.GetBlankSAV(GameVersion.Pt, ot),
				SupportedGame.B2W2 => SaveUtil.GetBlankSAV(GameVersion.B2W2, ot),
				SupportedGame.ORAS => SaveUtil.GetBlankSAV(GameVersion.ORAS, ot),
				SupportedGame.USUM => SaveUtil.GetBlankSAV(GameVersion.US, ot),
				SupportedGame.LGPE => SaveUtil.GetBlankSAV(GameVersion.GE, ot),
				SupportedGame.SWSH => SaveUtil.GetBlankSAV(GameVersion.SWSH, ot),
				SupportedGame.BDSP => SaveUtil.GetBlankSAV(GameVersion.BD, ot),
				SupportedGame.PLA => SaveUtil.GetBlankSAV(GameVersion.PLA, ot),
				SupportedGame.SV => SaveUtil.GetBlankSAV(GameVersion.SV, ot),
				_ => throw new ArgumentOutOfRangeException(nameof(game))
			};

			var pkm = sav.GetLegal(template, out _);

			pkm = game switch
			{
				SupportedGame.RBY => EntityConverter.ConvertToType(pkm, typeof(PK1), out _) ?? pkm,
				SupportedGame.GSC => EntityConverter.ConvertToType(pkm, typeof(PK2), out _) ?? pkm,
				SupportedGame.RS => EntityConverter.ConvertToType(pkm, typeof(PK3), out _) ?? pkm,
				SupportedGame.PT => EntityConverter.ConvertToType(pkm, typeof(PK4), out _) ?? pkm,
				SupportedGame.B2W2 => EntityConverter.ConvertToType(pkm, typeof(PK5), out _) ?? pkm,
				SupportedGame.ORAS => EntityConverter.ConvertToType(pkm, typeof(PK6), out _) ?? pkm,
				SupportedGame.USUM => EntityConverter.ConvertToType(pkm, typeof(PK7), out _) ?? pkm,
				SupportedGame.LGPE => EntityConverter.ConvertToType(pkm, typeof(PB7), out _) ?? pkm,
				SupportedGame.SWSH => EntityConverter.ConvertToType(pkm, typeof(PK8), out _) ?? pkm,
				SupportedGame.BDSP => EntityConverter.ConvertToType(pkm, typeof(PB8), out _) ?? pkm,
				SupportedGame.PLA => EntityConverter.ConvertToType(pkm, typeof(PA8), out _) ?? pkm,
				SupportedGame.SV => EntityConverter.ConvertToType(pkm, typeof(PK9), out _) ?? pkm,
				_ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
			};

			return Task.FromResult(pkm);
		}

		public Task<byte[]> CheckLegalAndGetBytes(PKM pkm, bool encrypted)
			=> !pkm.IsLegal()
				? Task.FromException<byte[]>(new LegalityException("Pokemon not valid."))
				: Task.FromResult(encrypted ? pkm.EncryptedPartyData : pkm.DecryptedPartyData);
	}
}