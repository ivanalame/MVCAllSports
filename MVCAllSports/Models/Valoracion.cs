using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MVCAllSports.Models
{
    [Table("Valoraciones")]
    public class Valoracion
    {
        [Key]
        [Column("IdValoracion")]
        public int IdValoracion { get; set; }
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        [Column("IdProducto")]
        public int IdProducto { get; set; }
        [Column("Comentario")]
        public string Comentario { get; set; }
        [Column("Puntuacion")]
        public int Puntuacion { get; set; }
    }
}
