namespace Application.DTOs.Auth
{
    public class FirebaseLoginRequest
    {
        public required string IdToken { get; set; }
        public string? fmcToken { get; set; }
    }
}
