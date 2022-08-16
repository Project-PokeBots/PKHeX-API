using AspNetCore.ExceptionHandler.Attributes;
using Microsoft.AspNetCore.Http;

namespace PKHeX.API.Exceptions.Api
{

	[StatusCode(StatusCodes.Status400BadRequest)]
	public abstract class IllegalPokemonApiException : ApiBaseException
	{
		protected IllegalPokemonApiException()
		{
		}

		protected IllegalPokemonApiException(string? message) : base(message)
		{
		}
	}
}