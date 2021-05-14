using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Auth {
	/// <summary>
	/// https://www.youtube.com/watch?v=2c4p6RGtkps
	/// </summary>
	public class JwtParser {

		public static IEnumerable<Claim> parseClaimsFromJwt(string jwt) {

			var claims = new List<Claim>();

			var payload = jwt.Split('.')[1];

			var jsonBytes = parseBase64WithoutPadding(payload);

			var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, Object>>(jsonBytes);

			extractRolesFromJwt(claims,keyValuePairs);

			claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key,kvp.Value.ToString())));

			return claims;
		}

		private static void extractRolesFromJwt(List<Claim> claims, Dictionary<string, object> keyValuePair) {
			keyValuePair.TryGetValue(ClaimTypes.Role, out object roles);

			if (roles is not null) {
				var parsedRoles = roles.ToString().Trim().TrimStart('[').TrimEnd(']').Split(',');

				if (parsedRoles.Length > 1) {
					foreach (var paresedRole in parsedRoles) {
						claims.Add(new Claim(ClaimTypes.Role,paresedRole.Trim('"')));
					}
				}
				else {
					claims.Add(new Claim(ClaimTypes.Role,parsedRoles[0]));
				}
			}

			keyValuePair.Remove(ClaimTypes.Role);
		}

		private static byte[] parseBase64WithoutPadding(string base64) {
			switch (base64.Length %4) {

				case 2:
					base64 += "==";
					break;
				case 3:
					base64 += "=";
					break;
			}

			return Convert.FromBase64String(base64);
		}

	}
}
