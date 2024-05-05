
namespace MVCAllSports.Models
{
    public class ValoracionConNombreUsuario
    {
        public int IdValoracion { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
    }
}
