using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PKHeX.Core;
using PKHeX.API.Exceptions;
using PKHeX.API.Models;

namespace PKHeX.API.Services
{

	public class DownloaderService
	{
		private HttpClient HttpClient { get; }

		public DownloaderService(HttpClient httpClient)
			=> HttpClient = httpClient;

		public async Task<PKM> DownloadPkmAsync(Uri uri, long? length, SupportedGame wantedGame)
		{
			long? SIZE = null;
			if (length != null) SIZE = (long)length;
			else
			{
				var request = new HttpRequestMessage
				{
					RequestUri = uri,
					Method = HttpMethod.Head
				};

				var response = await HttpClient.SendAsync(request);

				response.Headers.TryGetValues("content-length", out var values);

				var first = values?.First();

				if (first != null)
					SIZE = long.Parse(first);
			}

			if (SIZE != null)
			{
				if (!EntityDetection.IsSizePlausible((long)SIZE))
					throw new BadRequestException("Invalid size");
			}

			var fileName = Path.GetExtension($"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Authority}{uri.AbsolutePath}");

			var buffer = await HttpClient.GetByteArrayAsync(uri);

			var pkm = EntityFormat.GetFromBytes(buffer);
			if (pkm == null)
				throw new BadRequestException("Invalid pkm file");

			return wantedGame switch
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
				_ => throw new ArgumentOutOfRangeException(nameof(wantedGame), wantedGame, null)
			};
		}
	}
}