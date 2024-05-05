using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MVCAllSports.Models
{
    [Table("CATEGORIAPRODUCTOS")]
    public class CategoriaProduto
    {
        [Key]
        [Column("IdCategoriaProducto")]
        public int IdCategoriaProducto { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("IdDetalleDeporte")]
        public int IdDetalleDeporte { get; set; }
    }

 
}
