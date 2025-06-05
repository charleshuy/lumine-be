namespace Application.Interfaces.Auth
{
    public interface IFirebaseAuthService
    {
        Task<string> SignInWithFirebaseAsync(string idToken, string? fcmToken);
        Task<string> SignInWithEmailPasswordAsync(string email, string password);
        Task RegisterWithEmailPasswordFireBaseAsync(string email, string password);
        Task<string> LoginWithEmailPasswordFirebaseAsync(string email, string password, string? fcmToken = null);
    }
}
