using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PKHeX.Core;
using PKHeX.API.Exceptions;
using PKHeX.API.Models;
using PKHeX.API.Services;
using PKHeX.API.Util;
using QRCoder;

namespace PKHeX.API.Controllers {

	[Route("/pokemon/{game}")]
	[ApiController]
	public class PokemonController : ControllerBase
	{
		private PokemonService PokemonService { get; }

		private DownloaderService DownloaderService { get; }

		private bool IsEncryptionWanted
			=> (Request.Headers["X-Pokemon-Encrypted"].FirstOrDefault() ?? "").ToLower() == "true";

		public PokemonController(PokemonService pokemonService, DownloaderService downloaderService)
		{
			PokemonService = pokemonService;
			DownloaderService = downloaderService;
		}

		/// <summary>
		/// Returns Pokemon Data from a Showdown set
		/// </summary>
		/// <param name="body">Request body containing Showdown set</param>
		/// <param name="game">Wanted game for the returned file format</param>
		/// <returns>Pokemon Data as File</returns>
		/// <response code="200">Returns the Pokemon Data of Showdown set</response>
		/// <response code="400">The Pokemon set is invalid or illegal</response>     
		[HttpPost("showdown")]
		[Produces(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetPokemonFromShowdown([FromRoute] string game, [FromBody, Required] PokemonShowdownRequest body)
		{
			var pkm = await PokemonService.GetPokemonFromShowdown(body.ShowdownSet, SupportGameUtil.GetFromString(game));

			return await ReturnPokemonFile(pkm, IsEncryptionWanted);
		}

		/// <summary>
		/// Checks if a Showdown set is legal
		/// </summary>
		/// <param name="body">Request body containing Showdown set</param>
		/// <param name="game">Wanted game for the returned file format</param>
		/// <returns>Nothing</returns>
		/// <response code="204">Pokemon is legal</response>
		/// <response code="400">Pokemon is illegal</response>  
		[HttpPost("showdown/legality")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CheckShowdownLegality([FromRoute] string game, [FromBody, Required] PokemonShowdownRequest body)
		{
			var pkm = await PokemonService.GetPokemonFromShowdown(body.ShowdownSet, SupportGameUtil.GetFromString(game));

			return await ReturnPokemonLegality(pkm);

		}

		/// <summary>
		/// Returns Pokemon Data from a file by url
		/// </summary>
		/// <param name="game">Wanted game for the returned file format</param>
		/// <param name="url">The url of the file</param>
		/// <param name="size">Optional, the size of the file if already known ahead of making a request</param>
		/// <returns>Pokemon Data of file</returns>
		/// <response code="204">Returns the Pokemon Data of the file</response>
		/// <response code="400">Pokemon is illegal or invalid</response>  
		[HttpGet("url")]
		[Produces(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetPokemonFromUrl([FromRoute] string game, [FromQuery, Required] string url, [FromQuery] long? size)
		{
			var pkm = await DownloaderService.DownloadPkmAsync(new Uri(url), size, SupportGameUtil.GetFromString(game));

			return await ReturnPokemonFile(pkm, IsEncryptionWanted);
		}

		/// <summary>
		/// Checks if a Pokemon file (from an url) is legal
		/// </summary>
		/// <param name="game">Wanted game for the returned file format</param>
		/// <param name="url">The url of the file</param>
		/// <param name="size">Optional, the size of the file if already known ahead of making a request</param>
		/// <returns>Nothing</returns>
		/// <response code="204">Pokemon is legal</response>
		/// <response code="400">Pokemon is illegal</response>  
		[HttpGet("url/legality")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CheckUrlLegality([FromRoute] string game, [FromQuery, Required] string url, [FromQuery] long? size)
		{
			var pkm = await DownloaderService.DownloadPkmAsync(new Uri(url), size, SupportGameUtil.GetFromString(game));

			return await ReturnPokemonLegality(pkm);

		}

		/// <summary>
		/// Returns Pokemon Data from file
		/// </summary>
		/// <param name="game">Wanted game for the returned file feedback</param>
		/// <param name="file">Uploaded Pokemon file</param>
		/// <returns>Pokemon Data of file</returns>
		/// <response code="200">Pokemon is evaluted</response>
		/// <response code="400">Pokemon file is corrupted</response>  
		[HttpPost("file")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[RequestFormLimits(MultipartBodyLengthLimit = 376)]

		public async Task<IActionResult> GetPokemonFromFile([FromRoute] string game, [Required] IFormFile file)
		{
			var pkm = await PokemonService.GetPokemonFromFormFileAsync(file, SupportGameUtil.GetFromString(game));


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

			return Ok(pokemonsummary);

		}

		/// <summary>
		/// Checks if a Pokemon file is legal
		/// </summary>
		/// <param name="game">Wanted game for the returned file format</param>
		/// <param name="file">Uploaded Pokemon file</param>
		/// <returns>Nothing</returns>
		/// <response code="204">Pokemon is legal</response>
		/// <response code="400">Pokemon is illegal</response>  
		[HttpPost("file/legality")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[RequestFormLimits(MultipartBodyLengthLimit = 376)]
		public async Task<IActionResult> CheckFileLegality([FromRoute] string game, [Required] IFormFile file)
		{
			var pkm = await PokemonService.GetPokemonFromFormFileAsync(file, SupportGameUtil.GetFromString(game));

			return await ReturnPokemonLegality(pkm);

		}

		/// <summary>
		/// Converts file to showdown set
		/// </summary>
		/// <param name="game">Wanted game for the returned file feedback</param>
		/// <param name="file">Uploaded Pokemon file</param>
		/// <returns>Pokemon showdown set</returns>
		/// <response code="200">Pokemon is evaluted</response>
		/// <response code="400">Pokemon file is corrupted</response>  
		[HttpPost("file/showdown")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[RequestFormLimits(MultipartBodyLengthLimit = 376)]
		public async Task<IActionResult> GetShowdownPokemonFromFile([FromRoute] string game, [Required] IFormFile file)
		{
			var pkm = await PokemonService.GetPokemonFromFormFileAsync(file, SupportGameUtil.GetFromString(game));

			string showdown = ShowdownParsing.GetShowdownText(pkm);
			return Content(showdown);
		}

		/// <summary>
		/// Converts file to QR code
		/// </summary>
		/// <param name="game">Wanted game for the returned file feedback</param>
		/// <param name="file">Uploaded Pokemon file</param>
		/// <returns>QR Image</returns>
		/// <response code="200">Pokemon is evaluted</response>
		/// <response code="400">Pokemon file is corrupted</response>  
		[HttpPost("file/QR")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[RequestFormLimits(MultipartBodyLengthLimit = 376)]
		public async Task<IActionResult> GetFileToQR([FromRoute] string game, [Required] IFormFile file)
		{

			var pkm = await PokemonService.GetPokemonFromFormFileAsync(file, SupportGameUtil.GetFromString(game));

			var QRImage = QRMessageUtil.GetMessage(pkm);

			QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
			QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(QRImage, QRCodeGenerator.ECCLevel.Q);
			PngByteQRCode pngByteQRCode = new PngByteQRCode(qRCodeData);
			byte[] qrCodeBytes = pngByteQRCode.GetGraphic(20);
			string qrCodeString = Convert.ToBase64String(qrCodeBytes);

			Response.Headers.Add("X-Pokemon-Species", ((Species)pkm.Species).ToString());

			return Ok(qrCodeString);
		}

		private async Task<FileContentResult> ReturnPokemonFile(PKM pkm, bool encrypted = false)
		{
			Response.Headers.Add("X-Pokemon-Species", ((Species)pkm.Species).ToString());
			if (pkm is ISanityChecksum sanityChecksum) Response.Headers.Add("X-Pokemon-Checksum", sanityChecksum.Checksum.ToString());

			pkm.ResetPartyStats();

			return File(await PokemonService.CheckLegalAndGetBytes(pkm, encrypted), MediaTypeNames.Application.Octet);
		}

		private async Task<NoContentResult> ReturnPokemonLegality(PKM pkm, bool encrypted = false)
		{
			Response.Headers.Add("X-Pokemon-Legality", (pkm.IsLegal()).ToString());

			var LA = new LegalityAnalysis(pkm);
			string Legality_Report = LA.Report();

			if (Legality_Report == "Legal!")
				return NoContent();

			throw new LegalityException(Legality_Report);
		}
	}
	}