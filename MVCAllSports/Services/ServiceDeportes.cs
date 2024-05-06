using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using MVCAllSports.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json.Linq;
using AllSports.Helpers;
using Azure.Storage.Blobs;
using StackExchange.Redis;
using MVCAllSports.Helpers;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace MVCAllSports.Services
{
    public class ServiceDeportes
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue header;
        //private HelperCryptography helper;
        private IDatabase database;
        private BlobServiceClient client;
        private IHttpContextAccessor httpContextAccessor;

        public ServiceDeportes(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,/*HelperCryptography helper*/ BlobServiceClient client)
        {
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
            this.UrlApi = configuration.GetValue<string>("ApiUrls:ApiAllSports");
            //this.helper = helper;
            this.httpContextAccessor = httpContextAccessor;
            this.client = client;
            this.database = HelperCacheMultiplexer.Connection.GetDatabase();

            //var apiUrlsConfig = configuration.GetSection("KeyVault");
            //var keyVaultUrl = apiUrlsConfig.GetValue<string>("VaultUri");
            //var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            //var secretName = "secretoUrlApi";
            //KeyVaultSecret secret = secretClient.GetSecret(secretName);
            //this.UrlApi = secret.Value;
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }
        private async Task<int> ObtenerUltimoIdProductoAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/productos";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<Producto> productos = JsonConvert.DeserializeObject<List<Producto>>(json);

                    if (productos.Count > 0)
                    {
                        int maxId = productos.Max(p => p.IdProducto);
                        return maxId + 1;
                    }
                }
            }
            // Si no se puede obtener el último ID, puedes manejarlo de alguna manera adecuada, como lanzar una excepción o devolver un valor predeterminado.
            return 0;
        }
       

        #region  GET PRICIPALES
        //GET DEPORTES
        public async Task<List<Deporte>> GetDeportesAsync()
        {
            string request = "api/deportes";
            List<Deporte> data = await this.CallApiAsync<List<Deporte>>(request);
            return data;
        }
        //GET NUTRICION
        public async Task<List<Nutricion>> GetNutricionAsync()
        {
            string request = "api/deportes/getnutricion";
            List<Nutricion> data = await this.CallApiAsync<List<Nutricion>>(request);
            return data;
        }
        //GET DETALLES DEPORTES
        public async Task<List<DetalleDeporte>> GetDetallesDeporteAsync()
        {
            string request = "api/deportes/getdetalledeporte";
            List<DetalleDeporte> data = await this.CallApiAsync<List<DetalleDeporte>>(request);
            return data;
        }
        //GET DETALLES DEPORTE POR ID DEPORTE
        public async Task<List<DetalleDeporte>> GetDetalleDeporteByIdAsync(int idDeporte)
        {
            string request = "api/deportes/getdetallesdeporteById/" + idDeporte;
            List<DetalleDeporte> data = await this.CallApiAsync<List<DetalleDeporte>>(request);
            return data;
        }
        //GET CATEGORIAS
        public async Task<List<CategoriaProduto>> GetAllCategoriasAsync()
        {
            string request = "api/Deportes/GetAllCategorias";
            List<CategoriaProduto> data = await this.CallApiAsync<List<CategoriaProduto>>(request);
            return data;
        }
        //GET Categoria por id detalle
        public async Task<List<CategoriaProduto>> GetCategoriasByIdDetalleAsync(int idDetalle)
        {
            string request = "api/deportes/getCategoriaProductoByIdDetalle/" + idDetalle;
            List<CategoriaProduto> data = await this.CallApiAsync<List<CategoriaProduto>>(request);
            return data;
        }
        #endregion

        #region  PRODUCTOS
        //GET ALL PRODUCTOS
        public async Task<List<Producto>> GetAllProductosAsync()
        {
            string request = "api/productos";
            List<Producto> data = await this.CallApiAsync<List<Producto>>(request);
            return data;
        }
        //Get Productos de una categoria
        public async Task<List<Producto>> GetProductosByIdCategoriaAsyncAsync(int idcategoria)
        {
            string request = "api/productos/GetProductosByIdCategoria/" + idcategoria;
            List<Producto> data = await this.CallApiAsync<List<Producto>>(request);
            return data;
        }
        //Get producto por id Producto 
        public async Task<Producto> GetProductoByIdAsync(int idproducto)
        {
            string request = "api/productos/GetProductoByIdProducto/" + idproducto;
            Producto data = await this.CallApiAsync<Producto>(request);
            return data;
        }
        //INSERT PRODUCTO 
        public async Task InsertProductoAsync(string nombre, int precio, string marca, string descripcion, int talla, string imagen, int idcategoria, string desclarga)
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            
            int maxid = await ObtenerUltimoIdProductoAsync();
            using (HttpClient client = new HttpClient())
            {
                string request = "api/productos";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //INSTANCIAMOS NUESTRO MODEL
                Producto producto = new Producto();

                producto.IdProducto = maxid;
                producto.Nombre = nombre;
                producto.Precio = precio;
                producto.Marca = marca;
                producto.Descripcion = descripcion;
                producto.Talla = talla;
                producto.Imagen = imagen;
                producto.IdCategoriaProducto = idcategoria;
                producto.Descripcion_Larga = desclarga;

                //CONVERTIMOS NUESTRO MODEL A JSON 
                string json = JsonConvert.SerializeObject(producto);
                //PARA ENVIAR DATOS (data) AL SERVICIO DEBEMOS  
                //UTILIZAR LA CLASE StringContent QUE NOS PEDIRA 
                //LOS DATOS, SU ENCODING Y EL TIPO DE FORMATO 

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }
        //ELIMINAR UN PRODUCTO
        public async Task DeleteProductoAsync(int idproducto)
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

            using (HttpClient client = new HttpClient())

            {
                string request = "api/productos/" + idproducto;
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.DeleteAsync(request);
            }
        }
        //MODIFICAR UN PRODUCTO
        public async Task UpdateProductoAsync(int idproducto, string nombre, int precio, string marca, string descripcion, int talla, string imagen, int idcategoria, string desclarga)
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

          
            using (HttpClient client = new HttpClient())

            {

                string request = "api/productos";

                client.BaseAddress = new Uri(this.UrlApi);

                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Producto producto = new Producto();

                producto.IdProducto = idproducto;
                producto.Nombre = nombre;
                producto.Precio = precio;
                producto.Marca = marca;
                producto.Descripcion = descripcion;
                producto.Talla = talla;
                producto.Imagen = imagen;
                producto.IdCategoriaProducto = idcategoria;
                producto.Descripcion_Larga = desclarga;

                string json = JsonConvert.SerializeObject(producto);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(request, content);
            }
        }

        //PAGINACION PRODUCTOS
        public async Task<ModelPaginacionProductos> GetProductosPaginacionAsync(int posicion)
        {
            string request = "api/productos/getproductospaginacion/" + posicion;
            ModelPaginacionProductos data = await this.CallApiAsync<ModelPaginacionProductos>(request);
            return data;
        }
        #endregion

        #region VALORACIONES
        //GET TODAS LAS VALORACIONES DE UN PRODUCTO 
        public async Task<List<ValoracionConNombreUsuario>> GetValoracionById(int IdProducto)
        {
            string request = "api/valoraciones/getvaloracionbyidproducto/" + IdProducto;
            List<ValoracionConNombreUsuario> data = await this.CallApiAsync<List<ValoracionConNombreUsuario>>(request);
            return data;
        }
        //INSERT Valoracion 
        public async Task InsertValoracionAsync(int idUsuario, int idProducto, string comentario, int puntuacion)
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

            Random random = new Random();
            int idRandom = random.Next(10, 1001);
          
            using (HttpClient client = new HttpClient())
            {
                string request = "api/valoraciones";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //INSTANCIAMOS NUESTRO MODEL
                Valoracion valoracion = new Valoracion();

                valoracion.IdValoracion = idRandom;
                valoracion.IdUsuario = idUsuario;
                valoracion.IdProducto = idProducto;
                valoracion.Comentario = comentario;
                valoracion.Puntuacion = puntuacion;

                string json = JsonConvert.SerializeObject(valoracion);


                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }
        //DELETE VALORACION
        public async Task DeleteValoracionAsync(int idvaloracion)
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

            using (HttpClient client = new HttpClient())

            {
                string request = "api/valoraciones/" + idvaloracion;
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.DeleteAsync(request);
            }
            
        }
        #endregion

        #region COMPRAS
        //GET COMPRA DE IDUSUARIO
        public async Task <List<Compra>> GetComprasByIdUser(int idusuario)
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

            string request = "api/compras/getcomprasbyiduser/" + idusuario;
            List<Compra> data = await this.CallApiAsync<List<Compra>>(request,token);
            return data;
        }
        //INSERT COMPRA
        public async Task InsertCompraAsync(int IdUsuario, int IdProducto, int Cantidad, DateTime FechaCompra, string Descuento, string metodo, string direccion, string provincia)
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

            //int maxid = await this.context.Compras.MaxAsync(z => z.IdCompra) + 1;  //get all compra y luego sacar ultimo id
            Random random = new Random();
            int albaran = random.Next(1, 1001);
          

            using (HttpClient client = new HttpClient())
            {
                string request = "api/compras";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //INSTANCIAMOS NUESTRO MODEL

                Compra compra = new Compra();
               // compra.IdCompra = maxid;
                compra.Albaran = albaran;
                compra.IdUsuario = IdUsuario;
                compra.IdProducto = IdProducto;
                compra.Cantidad = Cantidad;
                compra.FechaCompra = FechaCompra;
                compra.Descuento = Descuento;
                compra.Metodo_Pago = metodo;
                compra.Direccion = direccion;
                compra.Provincia = provincia;

                string json = JsonConvert.SerializeObject(compra);


                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }
        #endregion
        #region Login/OAuth-REGISTER
        public async Task<string> GetTokenAsync(string email, string password)
        {
            using(HttpClient client = new HttpClient())
            {
                string request = "api/usuarios/login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                LoginModel model = new LoginModel
                {
                    Email = email,Password = password
                };
                string jsonData = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data =  await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }
        //GET PERFIL USUARIO
        public async Task<Usuario> GetPerfilUsuarioAsync()
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/Usuarios/PerfilUsuario";
            Usuario usuario = await this.CallApiAsync<Usuario>(request, token);
            return usuario;
        }
        public async Task RegisterUsuario(string nombre, string apellidos, int nif, string email, string password, int idRole)
        {
            Random random = new Random();
            int idUsuario = random.Next(1, 1001);
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Usuarios/RegisterUsuario";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                //INSTANCIAMOS NUESTRO MODEL

                Usuario user = new Usuario
                {
                    IdUsuario = idUsuario,
                    Nombre = nombre,
                    Apellidos = apellidos,
                    Email = email,
                    Nif = nif,
                    //SACAMOS EL SALT
                    Salt = HelperCryptography.GenerateSalt(),
                };
                //user.Password = HelperCryptography.EncryptPassword(password, user.Salt);
                user.Password = password;
                user.IdRolUsuario = idRole;

                string json = JsonConvert.SerializeObject(user);


                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }
        //TENDREMOS UN METODO GENERICO QUE RECIBIRA EL REQUEST  

        //Y EL TOKEN 

        private async Task<T> CallApiAsync<T>(string request, string token)

        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                client.DefaultRequestHeaders.Add ("Authorization", "bearer " + token);

                HttpResponseMessage response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }

        }
        #endregion

        #region AZURE
        //METODO PARA OBTENER LA RUTA DE MI CONTAINER
        public async Task<string> GetContainerPathAsync()
        {
            BlobContainerClient containerClient = this.client.GetBlobContainerClient("allsports");
            return containerClient.Uri.ToString();
        }
        #endregion
        #region CACHE REDIS
        //METODO PARA ALMACENAR EN EL CARRO LOS PRODUCTOS
        public async Task AddProductoCarritoAsync(Producto producto)
        {
            string jsonProductos = await this.database.StringGetAsync("favoritos");
            List<Producto> productosList;
            if (jsonProductos == null)
            {
                //NO TENEMOS NADA EN EL CARRITO, LO CREAMOS
                productosList = new List<Producto>();
            }
            else
            {
                //YA TENEMOS PRODUCTOS Y LOS RECUPERAMOS
                productosList = JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
            }
            productosList.Add(producto);
            jsonProductos = JsonConvert.SerializeObject(productosList);
            await this.database.StringSetAsync("favoritos", jsonProductos);
        }

        //METODO PARA RECUPERAR LOS PRODUCTOS
        public async Task <List<Producto>> GetProductosCarritoAsync()
        {
            string jsonProductos = await this.database.StringGetAsync("favoritos");
            if(jsonProductos == null)
            {
                return null;
            }
            else
            {
                List<Producto> favoritos = JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
                return favoritos;
            }
        }
        //METODO PARA ELIMINAR PRODUCTO
        public async Task DeleteProductoCarritoAsync(int idproducto)
        {
            List<Producto> favoritos = await this.GetProductosCarritoAsync();
            if (favoritos !=null)
            {
                Producto productoDelete = favoritos.FirstOrDefault(x=>x.IdProducto== idproducto);
                favoritos.Remove(productoDelete);

                if(favoritos.Count == 0)
                {
                    await this.database.KeyDeleteAsync("favoritos");
                }
                else
                {
                    string jsonProductos = JsonConvert.SerializeObject(favoritos);
                    await this.database.StringSetAsync("favoritos",jsonProductos,TimeSpan.FromMinutes(30));
                }
            }
        }
        #endregion
    }
}
