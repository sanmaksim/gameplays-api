namespace GameplaysApi.DTOs
{
    public class LoginDto
    {
        public required string Username { get; set; }
        public string? Email { get; }
        public required string Password { get; set; }
    }
}
