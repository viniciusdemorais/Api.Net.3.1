namespace Core.Model.Settings
{
	public class TokenConfiguration
	{
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string Audience { get; set; }
		public string Issuer { get; set; }
		public string IssuerSigningKey { get; set; }
		public int ExpirationTimeInMinutes { get; set; }
		public string TokenType { get; set; }
		public string TokenDescription { get; set; }
	}
}
