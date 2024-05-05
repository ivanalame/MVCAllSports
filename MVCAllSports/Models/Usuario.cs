using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MVCAllSports.Models
{
    [Table("USUARIOS")]
    public class Usuario
    {
        [Key]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("Apellidos")]
        public string Apellidos { get; set; }
        [Column("Nif")]
        public int Nif { get; set; }
        [Column("Email")]
        public string Email { get; set; }
        [Column("SALT")]
        public string Salt { get; set; }
        [Column("Contraseña")]
        public string Password { get; set; }
        [Column("IdRolUsuario")]
        public int IdRolUsuario { get; set; }
    }
}
