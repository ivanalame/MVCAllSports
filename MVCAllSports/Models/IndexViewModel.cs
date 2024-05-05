namespace MVCAllSports.Models
{
    public class IndexViewModel
    {
        public List<Deporte> Deportes { get; set; }
        public List<DetalleDeporte> DetalleDeporte { get; set; }
        public List<Producto> Productos { get; set; }
        public List<Nutricion> Nutricion { get; set; }
    }
}
