namespace GameplaysApi.DTOs
{
    public class AuthRequestDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public required string Password { get; set; }
    }
}
