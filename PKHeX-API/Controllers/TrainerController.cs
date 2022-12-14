using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PKHeX.API.Models;
using PKHeX.API.Services;
using PKHeX.API.Util;

namespace PKHeX.API.Controllers
{

	/// <summary>
	/// Controller which serves routes for Trainer related data
	/// </summary>
	[Route("/trainer")]
	[ApiController]
	public class TrainerController : ControllerBase
	{
		private TrainerService TrainerService { get; }

		public TrainerController(TrainerService trainerService)
			=> TrainerService = trainerService;

		/// <summary>
		/// Gets Trainer data from the data block
		/// </summary>
		/// <param name="data">The data block holding the MyStatus/Status block</param>
		/// <param name="game">The game of this data block</param>
		/// <returns>Trainer information</returns>
		/// <exception cref="NotImplementedException">If the specified Game is not yet implemented</exception>
		[HttpPost("{game}")]
		public async Task<IActionResult> GetTrainerAsync([Required] IFormFile data, string game)
		{
			var trainerInfo = await TrainerService.GetTrainerInfo(data, SupportGameUtil.GetFromString(game));

			return Ok(new TrainerInfoResponse(trainerInfo.TID,
				trainerInfo.SID,
				trainerInfo.OT,
				trainerInfo.Gender,
				trainerInfo.Game,
				trainerInfo.Language,
				trainerInfo.Generation));
		}
	}
}