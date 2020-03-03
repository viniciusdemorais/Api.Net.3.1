namespace Core.Model.DTO
{
    public class CredentialsDTO
    {
        public bool Authenticated { get; set; }
        public string Created { get; set; }
        public string Expiration { get; set; }
        public string TypeToken { get; set; }
        public string AccessToken { get; set; }
    }
}
