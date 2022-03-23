﻿using System;
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

namespace PKHeX.API.Controllers;

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
	/// Returns Pokemon Data from a Showdown set.
	/// </summary>
	/// <param name="body">Request body containing Showdown set</param>
	/// <param name="game">Wanted game for the returned file format</param>
	/// <returns>Pokemon Data as File</returns>
	/// <response code="200">Returns the Pokemon Data of Showdown set</response>
	/// <response code="400">The Pokemon set is invalid or illegal</response>     
	[HttpGet, HttpPost]
	[Route("showdown")]
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
	[HttpGet, HttpPost]
	[Route("showdown/legality")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> CheckShowdownLegality([FromRoute] string game, [FromBody, Required] PokemonShowdownRequest body)
	{
		var pkm = await PokemonService.GetPokemonFromShowdown(body.ShowdownSet, SupportGameUtil.GetFromString(game));

		if (pkm.IsLegal())
			return NoContent();
			
		throw new LegalityException("Pokemon couldn't be legalized!");
	}

	/// <summary>
	/// Returns Pokemon Data from a file by url.
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

		if (pkm.IsLegal())
			return NoContent();
			
		throw new LegalityException("Pokemon couldn't be legalized!");
	}

	/// <summary>
	/// Returns Pokemon Data from an uploaded file.
	/// </summary>
	/// <param name="game">Wanted game for the returned file format</param>
	/// <param name="file">Uploaded Pokemon file</param>
	/// <returns>Pokemon Data of file</returns>
	[HttpGet, HttpPost]
	[Route("file")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[RequestFormLimits(MultipartBodyLengthLimit = 260)]
	public async Task<IActionResult> GetPokemonFromFile([FromRoute] string game, [FromForm, Required] IFormFile file)
	{
		var pkm = await PokemonService.GetPokemonFromFormFileAsync(file, SupportGameUtil.GetFromString(game));
			
		return await ReturnPokemonFile(pkm, IsEncryptionWanted);
	}

	/// <summary>
	/// Checks if a Pokemon file is legal
	/// </summary>
	/// <param name="game">Wanted game for the returned file format</param>
	/// <param name="file">Uploaded Pokemon file</param>
	/// <returns>Nothing</returns>
	/// <response code="204">Pokemon is legal</response>
	/// <response code="400">Pokemon is illegal</response>  
	[HttpGet, HttpPost]
	[Route("file/legality")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[RequestFormLimits(MultipartBodyLengthLimit = 260)]
	public async Task<IActionResult> CheckFileLegality([FromRoute] string game, [FromForm, Required] IFormFile file)
	{
		var pkm = await PokemonService.GetPokemonFromFormFileAsync(file, SupportGameUtil.GetFromString(game));

		if (pkm.IsLegal())
			return NoContent();
			
		throw new LegalityException("Pokemon couldn't be legalized!");
	}

	private async Task<FileContentResult> ReturnPokemonFile(PKM pkm, bool encrypted = false)
	{
		Response.Headers.Add("X-Pokemon-Species", ((Species) pkm.Species).ToString());
		Response.Headers.Add("X-Pokemon-Language", ((LanguageID) pkm.Language).ToString());
		if (pkm is ISanityChecksum sanityChecksum) Response.Headers.Add("X-Pokemon-Checksum", sanityChecksum.Checksum.ToString());

		pkm.ResetPartyStats();

		return File(await PokemonService.CheckLegalAndGetBytes(pkm, encrypted), MediaTypeNames.Application.Octet);
	}
}