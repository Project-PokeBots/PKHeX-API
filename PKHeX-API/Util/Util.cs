using System;
using System.Linq.Expressions;

namespace PKHeX.API.Util
{

	public static class Util
	{
		public static string GetEnvOrThrow(string env)
			=> Environment.GetEnvironmentVariable(env) ??
			   throw new Exception($"Missing environment variable \"{env}\"");

		public static string TryGetEnv(string env, string def)
			=> Environment.GetEnvironmentVariable(env) is string v && v.Length > 0 ? v : def;
	}
}