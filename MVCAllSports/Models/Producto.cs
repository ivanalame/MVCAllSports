using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MVCAllSports.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        [Column("IdProducto")]
        public int IdProducto { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("Precio")]
        public int Precio { get; set; }
        [Column("Marca")]
        public string Marca { get; set; }
        [Column("Descripcion")]
        public string Descripcion { get; set; }
        [Column("IdTalla")]
        public int Talla { get; set; }
        [Column("Imagen")]
        public string Imagen { get; set; }
        [Column("IdCategoriaProducto")]
        public int IdCategoriaProducto { get; set; }
        [Column("DescLarga")]
        public string Descripcion_Larga { get; set; }
    }
}
