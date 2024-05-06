using AllSports.Extensions;
using AllSports.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using MVCAllSports.Filters;
using MVCAllSports.Models;
using MVCAllSports.Services;
using System.Reflection;
using System.Security.Claims;

namespace MVCAllSports.Controllers
{
    public class DeportesController : Controller
    {
        private ServiceDeportes service;
        private HelperMails helperMail;
        //private HelperUploadFiles helperUploadFiles;
        private TelemetryClient telemetryClient;

        public DeportesController(ServiceDeportes service,HelperMails helperMails,/*HelperUploadFiles helperUploadFiles,*/TelemetryClient telemetryClient)
        {
            this.service = service;
            this.telemetryClient = telemetryClient;
            this.helperMail = helperMails;
           // this.helperUploadFiles = helperUploadFiles;
        }

        #region PRINCIPAL
          public async Task <IActionResult> Index(int? idProducto, int? precio, int? posicion)
           {
            if (idProducto != null && precio != null)
            {
                List<int> idsProductos;
                int sumaPrecios = 0;

                if (HttpContext.Session.GetString("SUMAPRECIOS") != null)
                {
                    //recupero de session el precio 
                    sumaPrecios = int.Parse(HttpContext.Session.GetString("SUMAPRECIOS"));
                }
                sumaPrecios += precio.Value;
                HttpContext.Session.SetString("SUMAPRECIOS", sumaPrecios.ToString());

                Producto producto = await this.service.GetProductoByIdAsync(idProducto.Value);
                await this.service.AddProductoCarritoAsync(producto);
                ViewData["MENSAJE"] = "Producto almacenado en el carrito";

                if (HttpContext.Session.GetString("IDSPRODUCTOS") == null)
                {
                    //TODAVIA NO TENEMOS DATOS EN SESSION Y CREAMOS LA COLECCION 
                    idsProductos = new List<int>();
                }
                else
                {
                    idsProductos = HttpContext.Session.GetObject<List<int>>("IDSPRODUCTOS");
                }
                idsProductos.Add(idProducto.Value);
                ////GUARDAMOS LA COLECCION EN SESSION
                HttpContext.Session.SetObject("IDSPRODUCTOS", idsProductos);
            }
            if (posicion == null)
            {
                posicion = 1;
            }
            ModelPaginacionProductos model = await this.service.GetProductosPaginacionAsync(posicion.Value);
            ViewData["REGISTROS"] = model.NumeroRegistros;
            ViewData["POSICIONACTUAL"] = posicion.Value;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Usuario user = await this.service.GetPerfilUsuarioAsync();
                ViewData["USERROLE"] = user.IdRolUsuario;
            }
            string rutaContainerAzure = await this.service.GetContainerPathAsync();
            ViewData["RutaContainer"] = rutaContainerAzure;

            List<Deporte> deportes =  await service.GetDeportesAsync();
            //List<Nutricion> nutricion = await service.GetNutricionAsync();
            List<DetalleDeporte> detalleDeportes =await  service.GetDetallesDeporteAsync();
            List<Producto> productos = model.Productos;

            var viewModel = new IndexViewModel
            {
                Deportes = deportes,
                //Nutricion = nutricion,
                DetalleDeporte = detalleDeportes,
                Productos = productos

            };
            return View(viewModel);
        }

        public async Task<IActionResult>DetalleDeporte(int IdDeporte)
        {
            List<DetalleDeporte>deportes =await this.service.GetDetalleDeporteByIdAsync(IdDeporte);
            return View(deportes);
        }

        public async Task <IActionResult> CategoriasProducto(int IdDetalleDeporte)
        {
            List<CategoriaProduto> Categorias = await this.service.GetCategoriasByIdDetalleAsync(IdDetalleDeporte);
            return View(Categorias);
        }
        public async Task <IActionResult> Productos(int IdCategoriaProducto, int? idProducto, int? precio)
        {

            if (idProducto != null && precio != null)
            {
                List<int> idsProductos;
                int sumaPrecios = 0;

                if (HttpContext.Session.GetString("SUMAPRECIOS") != null)
                {
                    //recupero de session el precio 
                    sumaPrecios = int.Parse(HttpContext.Session.GetString("SUMAPRECIOS"));
                }
                sumaPrecios += precio.Value;
                HttpContext.Session.SetString("SUMAPRECIOS", sumaPrecios.ToString());

                Producto producto = await this.service.GetProductoByIdAsync(idProducto.Value);
                await this.service.AddProductoCarritoAsync(producto);
                ViewData["MENSAJE"] = "Producto almacenado en el carrito";
                if (HttpContext.Session.GetString("IDSPRODUCTOS") == null)
                {
                    //TODAVIA NO TENEMOS DATOS EN SESSION Y CREAMOS LA COLECCION 
                    idsProductos = new List<int>();
                }
                else
                {
                    idsProductos = HttpContext.Session.GetObject<List<int>>("IDSPRODUCTOS");
                }
                idsProductos.Add(idProducto.Value);
                //GUARDAMOS LA COLECCION EN SESSION
               HttpContext.Session.SetObject("IDSPRODUCTOS", idsProductos);
                //ViewData["MENSAJE"] = "Productos Almacenados" + idsProductos.Count;
            }

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Usuario user = await this.service.GetPerfilUsuarioAsync();
                ViewData["USERROLE"] = user.IdRolUsuario;
            }
            string rutaContainerAzure = await this.service.GetContainerPathAsync();
            ViewData["RutaContainer"] = rutaContainerAzure;

            List<Producto> Productos =await this.service.GetProductosByIdCategoriaAsyncAsync(IdCategoriaProducto);
            return View(Productos);
        }
        #endregion

        //falta hacer bien el update con imagen
        #region DELETE-UPDATE
        [AuthorizeUsuarios]
        public async Task<IActionResult> EliminarProducto(int idproducto)
        {
            Producto producto = await this.service.GetProductoByIdAsync(idproducto);
            await this.service.DeleteProductoAsync(idproducto);
            return RedirectToAction("Productos", new { IdCategoriaProducto = producto.IdCategoriaProducto });
        }
        [AuthorizeUsuarios]
        public async Task<IActionResult> ModificarProducto(int idproducto)
        {
           List<CategoriaProduto> categorias =await this.service.GetAllCategoriasAsync();
            ViewData["CATEGORIAS"] = categorias;
            Producto producto = await this.service.GetProductoByIdAsync(idproducto);
            return View(producto);
        }
        [HttpPost]
        public async Task<IActionResult> ModificarProducto(Producto producto, IFormFile fichero, string imagenAnterior)
        {
            if (fichero != null)
            {
                //await this.helperUploadFiles.UploadFileAsync(fichero, Folders.Images);
                producto.Imagen = fichero.FileName;
                await this.service.UpdateProductoAsync(producto.IdProducto, producto.Nombre, producto.Precio, producto.Marca, producto.Descripcion, producto.Talla, producto.Imagen, producto.IdCategoriaProducto, producto.Descripcion_Larga);
            }
            else
            {
                producto.Imagen = imagenAnterior;
                await this.service.UpdateProductoAsync(producto.IdProducto, producto.Nombre, producto.Precio, producto.Marca, producto.Descripcion, producto.Talla, producto.Imagen, producto.IdCategoriaProducto, producto.Descripcion_Larga);
            }

            return RedirectToAction("Productos", new { IdCategoriaProducto = producto.IdCategoriaProducto });
        }
        #endregion

        #region DETAIL-PRODUCTO
        public async Task <IActionResult> DetailProducto(int IdProducto)
        {
            bool primerAcceso = string.IsNullOrEmpty(HttpContext.Session.GetString("PrimerAcceso"));
            if (!primerAcceso)
            {
                Producto productoCarrito = await this.service.GetProductoByIdAsync(IdProducto);
                await this.service.AddProductoCarritoAsync(productoCarrito);
                ViewData["MENSAJE"] = "Producto almacenado en el carrito";
                // Bloque de código que se ejecutará solo en el primer acceso
                List<int> idsProductos;
               
                if (HttpContext.Session.GetString("IDSPRODUCTOS") == null)
                {
                    idsProductos = new List<int>();
                }
                else
                {
                    idsProductos = HttpContext.Session.GetObject<List<int>>("IDSPRODUCTOS");
                }
                idsProductos.Add(IdProducto);
                //GUARDAMOS LA COLECCION EN SESSION
                HttpContext.Session.SetObject("IDSPRODUCTOS", idsProductos);
                //ViewData["MENSAJE"] = "Productos Almacenados" + idsProductos.Count;
            }
          

            HttpContext.Session.SetString("PrimerAcceso", "true");
            List<ValoracionConNombreUsuario> Valoraciones = await this.service.GetValoracionById(IdProducto);
            Producto producto = await this.service.GetProductoByIdAsync(IdProducto);
            ViewData["Producto"] = producto;
            string rutaContainerAzure = await this.service.GetContainerPathAsync();
            ViewData["RutaContainer"] = rutaContainerAzure;

            return View(Valoraciones);
        }
        #endregion

        #region REALIZAR-COMPRA
        [AuthorizeUsuarios]
        public async Task<IActionResult>Compra(int id)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Usuario user = await this.service.GetPerfilUsuarioAsync();
                ViewData["IDUSUARIO"] = user.IdRolUsuario;
            }
          
            Producto producto = await this.service.GetProductoByIdAsync(id);
            ViewData["MENSAJECOMPRA"] = "Compra Realizada";
            string rutaContainerAzure = await this.service.GetContainerPathAsync();
            ViewData["RutaContainer"] = rutaContainerAzure;
            return View(producto);
        }
        [HttpPost]
        public async Task<IActionResult> Compra(Compra compra)
        {
            string descuento = "";
            if (compra.Descuento == null)
            {
                descuento = "No";
            }
            else
            {
                descuento = compra.Descuento;
            }
            Usuario usuario = await this.service.GetPerfilUsuarioAsync();
            await this.service.InsertCompraAsync(usuario.IdUsuario, compra.IdProducto, compra.Cantidad, compra.FechaCompra, descuento, compra.Metodo_Pago, compra.Direccion, compra.Provincia);
            HttpContext.Session.SetString("IDPRODUCTOCOMPRADO", compra.IdProducto.ToString());
            ViewData["MENSAJECOMPRA"] = "Compra Realizada";

            //EVENTO PARA GUARDAR DATOS (APPLICATION INSIGHTS)
            this.telemetryClient.TrackEvent("CompraRequest");
            MetricTelemetry metric = new MetricTelemetry();
            metric.Name = "Compra";
            metric.Sum = compra.Cantidad;
            metric.Properties.Add("FechaCompra", compra.FechaCompra.ToString());
            metric.Properties.Add("MetodoPago", compra.Metodo_Pago);


            return RedirectToAction("index");
        }
        #endregion

        #region VALORACIONES AJAX
        public async Task<IActionResult> _Valoraciones(int? idProducto)
        {
            if (idProducto != null)
            {
                ViewData["IDPRODUCTO"] = idProducto.Value;
            }
            Usuario usuario = await this.service.GetPerfilUsuarioAsync();
            //var idUsuario = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewData["IDUSUARIO"] = usuario.IdUsuario;
            return PartialView("_Valoraciones");
        }

        [HttpPost]
        public async Task<IActionResult> _Valoraciones(Valoracion valoracion)
        {
            await this.service.InsertValoracionAsync(valoracion.IdUsuario, valoracion.IdProducto, valoracion.Comentario, valoracion.Puntuacion);
            ViewData["MENSAJE"] = "Valoracion Añadida";
            return PartialView("_Valoraciones");
        }
        #endregion

       
        #region  REGISTER-PERFIL
        public IActionResult Register()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register
            (string nombre, int nif, string email, string password, string apellidos, int idRole)
        {
            await this.service.RegisterUsuario(nombre, apellidos, nif, email, password, idRole);
            //string serverUrl = this.helperPathProvider.MapUrlServerPath();
            ////https://localhos:8555/Usuarios/ActivateUser/TOKEN?    esta es la url que tengo que generar
            //serverUrl = serverUrl + "/Usuarios/ActivateUser/" + user.TokenMail;
            string mensaje = "<h3>Codigo de Descuento</h3>";
            mensaje += "<p>Ha recibido un codigo de descuento por registrarse en nuestra págian</p>";
            //mensaje += "<p>" + serverUrl + "</p>";
            //mensaje += "<a href='" + serverUrl + "'>" + serverUrl + "</a>";
            mensaje = "<p>Introduzca el codigo: DESCUENTAZO para tener un 15% de descuento en todas sus compras </p>";
            await this.helperMail.SendMailAsync(email, "Codigo Descuento", mensaje);

            ViewData["MENSAJE"] = "Usuario registrado correctamente Y correo enviado";
            return View();
        }

        [AuthorizeUsuarios]
        public async Task <IActionResult> PerfilUsuario()
        {
            Usuario usuario = await this.service.GetPerfilUsuarioAsync();   
            return View(usuario);
        }
        #endregion

        #region INSERT PRODUCTO
        [AuthorizeUsuarios]
        public async Task< IActionResult >InsertProducto()
        {
            List<CategoriaProduto> categorias = await this.service.GetAllCategoriasAsync();
            ViewData["CATEGORIAS"] = categorias;
            return View();
        }
       [AuthorizeUsuarios]
        [HttpPost]
        public async Task<IActionResult> InsertProducto(Producto producto, IFormFile fichero)
        {
            if (fichero != null)
            {
                //await this.helperUploadFiles.UploadFileAsync(fichero, Folders.Images);

                producto.Imagen = fichero.FileName;
                await this.service.InsertProductoAsync(producto.Nombre, producto.Precio, producto.Marca, producto.Descripcion, producto.Talla, producto.Imagen, producto.IdCategoriaProducto, producto.Descripcion_Larga);
            }
            else
            {
                ViewData["MENSAJE"] = "asigne imagen";
            }
            List<CategoriaProduto> categorias = await this.service.GetAllCategoriasAsync();
            ViewData["CATEGORIAS"] = categorias;
            return RedirectToAction("Productos", new { IdCategoriaProducto = producto.IdCategoriaProducto });

        }
        #endregion
        #region MIS COMPRAS
        [AuthorizeUsuarios]
        public async Task<IActionResult> MisCompras()
        {
            Usuario usuario = await this.service.GetPerfilUsuarioAsync();
            List<Compra> Compras = await this.service.GetComprasByIdUser(usuario.IdUsuario);
            return View(Compras);
        }
        [AuthorizeUsuarios]
        public async Task<IActionResult> DetalleMisCompras(int? idProducto)
        {
            string rutaContainerAzure = await this.service.GetContainerPathAsync();
            ViewData["RutaContainer"] = rutaContainerAzure;
            Producto producto = await this.service.GetProductoByIdAsync(idProducto.Value);
            return View(producto);
        }
        #endregion

        #region Carrito
        [AuthorizeUsuarios]
        public async Task<IActionResult> Favoritos(int? ideliminar)
        {
            List<Producto> productos = await this.service.GetProductosCarritoAsync();
            List<int> idsProductos = HttpContext.Session.GetObject<List<int>>("IDSPRODUCTOS");

            if (productos == null)
            {
                idsProductos = new List<int>(); // Crear una nueva lista vacía de IDs de productos
                HttpContext.Session.SetObject("IDSPRODUCTOS", idsProductos);

                return View();
            }

            if (productos != null)
            {
                if (ideliminar != null)
                {
                    await this.service.DeleteProductoCarritoAsync(ideliminar.Value);
                    return RedirectToAction("Favoritos");
                }
            }

            return View(productos);
        }

        #endregion
    }
}
