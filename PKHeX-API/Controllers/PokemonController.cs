using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PKHeX.Core;
using PKHeX.API.Exceptions;
using PKHeX.API.Models;
using PKHeX.API.Services;
using PKHeX.API.Util;

namespace PKHeX.API.Controllers {

	[Route("/pokemon/{game}")]
	[ApiController]
	public class PokemonController : ControllerBase
	{
		private PokemonService PokemonService { get; }

		private DownloaderService DownloaderService { get; }

		private PKXService PKXService { get; }

		private QRService QRService { get; }

		private JSONService JSONService { get; }

		private bool IsEncryptionWanted
			=> (Request.Headers["X-Pokemon-Encrypted"].FirstOrDefault() ?? "").ToLower() == "true";

		public PokemonController(PokemonService pokemonService, DownloaderService downloaderService, PKXService pkxService, QRService qrService, JSONService jsonService)
		{
			PokemonService = pokemonService;
			DownloaderService = downloaderService;
			QRService = qrService;
			PKXService = pkxService;
			JSONService = jsonService;

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

			var pkmSummary = await PKXService.GetInfoFromFormFile(pkm);
			return Ok(pkmSummary);

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

			var JsonResponse = await JSONService.ProcessJsonMessage(Response.StatusCode, showdown);

			return Ok(JsonResponse);
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

			var QR = await QRService.GetQRFromFormFile(pkm);

			Response.Headers.Add("content-disposition", string.Format("inline;FileName=\"{0}\"", ((Species)pkm.Species).ToString()));

			var JsonResponse = await JSONService.ProcessJsonMessage(Response.StatusCode, QR);

			return Ok(JsonResponse);
		}

		private async Task<FileContentResult> ReturnPokemonFile(PKM pkm, bool encrypted = false)
		{
			Response.Headers.Add("content-disposition", string.Format("inline;FileName=\"{0}\"", ((Species)pkm.Species).ToString()));

			if (pkm is ISanityChecksum sanityChecksum) Response.Headers.Add("X-Pokemon-Checksum", sanityChecksum.Checksum.ToString());

			pkm.ResetPartyStats();

			return File(await PokemonService.CheckLegalAndGetBytes(pkm, encrypted), MediaTypeNames.Application.Octet);
		}

		private Task<NoContentResult> ReturnPokemonLegality(PKM pkm)
		{

			var LA = new LegalityAnalysis(pkm);
			string Legality_Report = LA.Report();

			if (Legality_Report == "Legal!")
				return Task.FromResult(NoContent());

			throw new LegalityException(Legality_Report);
		}
	}
	}