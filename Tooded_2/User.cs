using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Tooded_2
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // Kasutaja e-post: sellele saadetakse tšekid (saatja on poe postkast)
        public string Email { get; set; } = string.Empty;

        // Kasutaja rahakott (teenitakse klikkermängus "Töö")
        [Column(TypeName = "numeric(12,2)")]
        public decimal Raha { get; set; }

        // Klikkermängu täiendused
        public int TooriistaTase { get; set; } = 1; // raha ühe kliki eest
        public int AbilisteArv { get; set; }        // automaatne tulu sekundis
    }
}
