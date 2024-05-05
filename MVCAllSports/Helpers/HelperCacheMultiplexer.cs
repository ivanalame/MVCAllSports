using StackExchange.Redis;

namespace MVCAllSports.Helpers
{
    public class HelperCacheMultiplexer
    {
        private static Lazy<ConnectionMultiplexer> CreateConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheRedisKeys = "cacherredisivan.redis.cache.windows.net:6380,password=oWRaUsvJtI7fG1Mar8EFDfspBatScrokPAzCaP8Cy6Q=,ssl=True,abortConnect=False";
            return ConnectionMultiplexer.Connect(cacheRedisKeys);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return CreateConnection.Value;
            }
        }
    }
}
