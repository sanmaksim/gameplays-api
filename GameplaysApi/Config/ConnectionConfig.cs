using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Config
{
    public class ConnectionConfig
    {
        [Required]
        public string ConnectionString { get; set; } = default!;

        [Required]
        public string CorsOrigin { get; set; } = default!;

        [Required]
        public string KestrelPort { get; set; } = default!;
    }
}
