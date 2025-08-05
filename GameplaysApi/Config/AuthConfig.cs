using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Config
{
    public class AuthConfig
    {
        [Required]
        public string HmacSecretKey { get; set; } = default!;

        [Required]
        public string JwtCookieName { get; set; } = default!;

        [Required]
        public string JwtExpMins { get; set; } = default!;

        [Required]
        public string RefreshCookieName { get; set; } = default!;

        [Required]
        public string RefreshExpDays { get; set; } = default!;

        [Required]
        public string ValidIssuers { get; set; } = default!;

        [Required]
        public string ValidAudiences { get; set; } = default!;
    }
}
