namespace GameplaysApi.Interfaces
{
    public interface IUserService
    {
        string HashPassword(string password);
        bool VerifyPassword(string enteredPassword, string storedPassword);
    }
}
