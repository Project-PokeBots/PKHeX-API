using PKHeX.API.Exceptions.Api;

namespace PKHeX.API.Exceptions
{

	public class LegalityException : IllegalPokemonApiException
	{
		public LegalityException()
		{
		}

		public LegalityException(string? message) : base(message)
		{
		}
	}
}
