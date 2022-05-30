using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PKHeX.Core;
using PKHeX.API.Exceptions;
using PKHeX.API.Models;

namespace PKHeX.API.Services;

public class TrainerService
{
	public async Task<ITrainerInfo> GetTrainerInfo(IFormFile data, SupportedGame game)
	{
		ITrainerInfo saveFile;
		
		await using var fileStream = new MemoryStream();
		await data.CopyToAsync(fileStream);
		var fileData = fileStream.ToArray();
		
		switch (game)
		{
			case SupportedGame.RS:
			{
				var sav3file  = new SAV3RS();
				saveFile = sav3file;
				break;
			}
			case SupportedGame.PT:
			{
				var sav4file  = new SAV4Pt();
				saveFile = sav4file;
				break;
			}
			case SupportedGame.B2W2:
			{
				var sav5file  = new SAV5B2W2();
				fileData.CopyTo(sav5file.PlayerData.Data);
				saveFile = sav5file;
				break;
			}
			case SupportedGame.ORAS:
			{
				var sav6file  = new SAV6AO();
				fileData.CopyTo(sav6file.Status.Data);
				saveFile = sav6file;
				break;
			}
			case SupportedGame.USUM:
			{
				var sav7file  = new SAV7SM();
				fileData.CopyTo(sav7file.MyStatus.Data);
				saveFile = sav7file;
				break;
			}
			case SupportedGame.LGPE:
			{
				var sav7bfile = new SAV7b();
				fileData.CopyTo(sav7bfile.Status.Data);
				saveFile = sav7bfile;
				break;
			}
			case SupportedGame.SWSH:
			{
				var sav8file = new SAV8SWSH();
				fileData.CopyTo(sav8file.MyStatus.Data);
				saveFile = sav8file;
				break;
			}
			case SupportedGame.BDSP:
			{
				var sav8bfile = new SAV8BS();
				fileData.CopyTo(sav8bfile.MyStatus.Data);
				saveFile = sav8bfile;
				break;
			}
			case SupportedGame.PLA:
				var sav8afile = new SAV8LA();
				fileData.CopyTo(sav8afile.MyStatus.Data);
				saveFile = sav8afile;
				break;
			default:
				throw new NotImplementedException("Requested Game not implemented yet");
		}

		return saveFile;
	}
}
