using Application.DTOs.Auth;

namespace Application.Interfaces.Auth
{
    public interface IFirebaseAuthService
    {
        Task<string> SignInWithFirebaseAsync(string idToken, string? fcmToken);
        Task<string> SignInWithEmailPasswordAsync(string email, string password);
        Task RegisterWithEmailPasswordFireBaseAsync(RegisterEmailRequest request);
        Task<string> LoginWithEmailPasswordFirebaseAsync(string email, string password, string? fcmToken = null);
        Task RegisterArtistWithEmailPasswordFireBaseAsync(RegisterEmailRequest request);
    }
}
