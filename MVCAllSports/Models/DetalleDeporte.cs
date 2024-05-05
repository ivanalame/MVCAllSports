using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MVCAllSports.Models
{
    [Table("DetallesDeportes")]
    public class DetalleDeporte
    {
        [Key]
        [Column("IdDetalleDeporte")]
        public int IdDetalleDeporte { get; set; }

        [Column("IdDeporte")]
        public int IdDeporte { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("Imagen")]
        public string Imagen { get; set; }
    }
}
