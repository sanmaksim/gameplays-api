using System.ComponentModel.DataAnnotations;

namespace GameplaysApi.Config
{
    public class ConnectionConfig
    {
        [Required]
        public string ConnectionString { get; set; } = default!;

        [Required]
        public string CorsOriginDev { get; set; } = default!;

        [Required]
        public string CorsOriginTest { get; set; } = default!;
        
        [Required]
        public string DevCertPath { get; set; } = default!;

        [Required]
        public string KestrelHttpsPort { get; set; } = default!;

        [Required]
        public string ProdCertPath { get; set; } = default!;

        [Required]
        public string ProdKeyPath { get; set; } = default!;
    }
}
