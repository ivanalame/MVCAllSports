using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Caching.Distributed;
using MVCAllSports.Models;
using Newtonsoft.Json;

namespace MVCAllSports.Services
{
    public class ServiceAWSCache
    {
        private IDistributedCache cache;

        public ServiceAWSCache(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public async Task<List<Producto>> GetProductosFavoritosAsync()
        {
            //ALMACENAREMOS UNA COLECCION DE COCHES EN FORMATO JSON
            //LAS KEYS DEBEN SER UNICAS PARA CADA USER
            string jsonproductos =
                await this.cache.GetStringAsync("productosfavoritos");
            if (jsonproductos == null)
            {
                return null;
            }
            else
            {
                List<Producto> cars = JsonConvert.DeserializeObject<List<Producto>>(jsonproductos);
                return cars;
            }
        }

        public async Task AddProductosFavoritosAsynct(Producto pro)
        {
            List<Producto> productos = await this.GetProductosFavoritosAsync();
            //SI NO EXISTEN COCHES FAVORITOS TODAVIA, CREAMOS 
            //LA COLECCION
            if (productos == null)
            {
                productos = new List<Producto>();
            }
            //AÑADIMOS EL NUEVO COCHE A LA COLECCION
            productos.Add(pro);
            //SERIALIZAMOS A JSON LA COLECCION
            string jsonproductos = JsonConvert.SerializeObject(productos);
            DistributedCacheEntryOptions options =
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                };
            //ALMACENAMOS LA COLECCION DENTRO DE CACHE REDIS
            //INDICAREMOS QUE LOS DATOS DURARAN 30 MINUTOS
            await this.cache.SetStringAsync("productosfavoritos"
                , jsonproductos, options);
        }

        public async Task DeleteProductosFavoritosAsync(int idproducto)
        {
            List<Producto> cars = await this.GetProductosFavoritosAsync();
            if (cars != null)
            {
                Producto productoEliminar =
                    cars.FirstOrDefault(x => x.IdProducto == idproducto);
                cars.Remove(productoEliminar);
                //COMPROBAMOS SI LA COLECCION TIENE COCHES FAVORITOS
                //TODAVIA O NO TIENE
                //SI NO TENEMOS COCHES, ELIMINAMOS LA KEY DE CACHE REDIS
                if (cars.Count == 0)
                {
                    await this.cache.RemoveAsync("productosfavoritos");
                }
                else
                {
                    //ALMACENAMOS DE NUEVO LOS COCHES SIN EL CAR ELIMINADO
                    string jsonproductos = JsonConvert.SerializeObject(cars);
                    DistributedCacheEntryOptions options =
                        new DistributedCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(30)
                        };
                    //ACTUALIZAMOS EL CACHE REDIS
                    await this.cache.SetStringAsync("productosfavoritos", jsonproductos
                        , options);
                }
            }
        }
    }
}
