namespace Application.DTOs.Auth
{
    public class FirebaseLoginRequest
    {
        public required string IdToken { get; set; }
        public string? fmcToken { get; set; }
    }

    public class AdminLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
