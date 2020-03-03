using Core.Contract.Bll;
using Core.Lib;
using Core.Model;
using Core.Model.DTO;
using Core.Model.DTO.Request;
using Core.Model.Settings;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business
{
    public class AuthenticationBll : IAuthenticationBll
    {
		private readonly TokenConfiguration _tokenConfiguration;
		private readonly Api _api;
		private const int UTC_BRASILIA_TIME = -3;
		private const string DESCRIPTION = "Bearer token authentication";
		private const string EXCEPTION_MESSAGE = "ClientId or ClientSecret invalid.";
		private const string TIME_STAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";

		public AuthenticationBll(AppSettings appSettings)
		{
			_api = appSettings.Api;
			_tokenConfiguration = appSettings.TokenConfiguration;
		}

		public BaseResponse<CredentialsDTO> GenerateBearerToken(BearerRequestDTO request)
		{
			var response = new BaseResponse<CredentialsDTO>();
			try
			{
				if (!request.ClientId.Equals(_tokenConfiguration.ClientId) || !request.ClientSecret.Equals(_tokenConfiguration.ClientSecret))
				{
					throw new Exception(EXCEPTION_MESSAGE);
				}

				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.ASCII.GetBytes(_tokenConfiguration.IssuerSigningKey);

				var identity = new ClaimsIdentity();
				identity.AddClaim(new Claim(_api.Name, DESCRIPTION));

				var creationUtcDate = DateTime.UtcNow.AddHours(UTC_BRASILIA_TIME);
				var expirationUtcDate = creationUtcDate.AddMinutes(_tokenConfiguration.ExpirationTimeInMinutes);

				var creationDate = DateTime.Now;
				var expirationDate = creationDate.AddMinutes(_tokenConfiguration.ExpirationTimeInMinutes);

				var securityToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
				{
					Issuer = _tokenConfiguration.Issuer,
					Audience = _tokenConfiguration.Audience,
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
					Subject = identity,
					IssuedAt = creationDate,
					NotBefore = creationDate,
					Expires = expirationDate,
				});
				var token = tokenHandler.WriteToken(securityToken);

				response.Data = new CredentialsDTO()
				{
					Authenticated = true,
					Created = creationUtcDate.ToString(TIME_STAMP_FORMAT),
					Expiration = expirationUtcDate.ToString(TIME_STAMP_FORMAT),
					AccessToken = token,
					TypeToken = _tokenConfiguration.TokenType
				};
			}
			catch (Exception ex)
			{
				response = ex.PopulateResponseObject(response);
			}

			return response;
		}
	}
}