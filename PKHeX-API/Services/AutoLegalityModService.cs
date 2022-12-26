using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeX.API.Util.Util;

namespace PKHeX.API.Services
{

	public class AutoLegalityModService : IHostedService
	{
		private bool Initialized { get; set; }

		public AutoLegalityModService()
		{
			SetupSettings();
			InitializeCoreStrings();
		}

		public Task StartAsync(CancellationToken cancellationToken)
			=> Initialized ? Task.CompletedTask : InitializeAsync(cancellationToken);

		public Task StopAsync(CancellationToken cancellationToken)
			=> Task.CompletedTask;
		private async Task InitializeAsync(CancellationToken cancellationToken)
		{
			await Task.WhenAll(
				Task.Run(() => EncounterEvent.RefreshMGDB("./mgdb"), cancellationToken),
				Task.Run(InitializeTrainerDatabase, cancellationToken));
			Initialized = true;
		}

		private static void SetupSettings()
		{
			APILegality.SetAllLegalRibbons = true;
			APILegality.SetMatchingBalls = true;
			APILegality.ForceSpecifiedBall = true;
			APILegality.UseXOROSHIRO = true;
			APILegality.AllowTrainerOverride = true;
			APILegality.AllowBatchCommands = true;
			Legalizer.EnableEasterEggs = false;
		}

		private static void InitializeTrainerDatabase()
		{
			var ot = TryGetEnv("PKHEX_DEFAULT_OT", "PKXAPI");
			var trainerId = int.Parse(TryGetEnv("PKHEX_DEFAULT_TID", "123456"));
			var secretId = int.Parse(TryGetEnv("PKHEX_DEFAULT_SID", "1234"));
			var languageName = TryGetEnv("PKHEX_DEFAULT_LANGUAGE", "English");

			if (!Enum.TryParse<LanguageID>(languageName, true, out var language))
				throw new Exception($"Invalid default language {languageName}");

			for (var i = 1; i < PKX.Generation + 1; i++)
			{
				var versions = GameUtil.GetVersionsInGeneration(i, PKX.Generation);
				foreach (var v in versions)
				{
					var fallback = new SimpleTrainerInfo(v)
					{
						Language = (int)language,
						TID = trainerId,
						SID = secretId,
						OT = ot,
					};
					var exist = TrainerSettings.GetSavedTrainerData(v, i, fallback);
					if (exist is SimpleTrainerInfo)
						TrainerSettings.Register(fallback);
				}
			}

			var trainer = TrainerSettings.GetSavedTrainerData(PKX.Generation);
			RecentTrainerCache.SetRecentTrainer(trainer);
		}

		private static void InitializeCoreStrings()
		{
			var lang = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName[..2];
			LocalizationUtil.SetLocalization(typeof(LegalityCheckStrings), lang);
			LocalizationUtil.SetLocalization(typeof(MessageStrings), lang);
			RibbonStrings.ResetDictionary(GameInfo.Strings.ribbons);

			ParseSettings.ChangeLocalizationStrings(GameInfo.Strings.movelist, GameInfo.Strings.specieslist);
		}
	}
}