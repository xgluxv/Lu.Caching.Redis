using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Lu.Caching.Redis
{
    public class CachingProviderBase:ICache
    {
        private static string _connectString;

        public CachingProviderBase(string connectString)
        {
            _connectString = connectString;
        }
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(_connectString);
        });

        private static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        private static IDatabase Database
        {
            get
            {
                return Connection.GetDatabase();
            }
        }

        static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        public void AddItem(string key, object value)
        {
            Database.StringSet(key, Serialize(value));
        }

        public void AddItem(string key, object value, DateTimeOffset dateTimeOffset)
        {
            var span=dateTimeOffset.UtcDateTime-DateTimeOffset.UtcNow;
            Database.StringSet(key, Serialize(value), span);
        }

        [Obsolete("Don't use this method anywhere!!!")]
        public void AddItem(string key, object value, CacheItemPolicy policy)
        {
            throw new NotImplementedException();
        }

        public T GetItem<T>(string key, bool remove)
        {
            var t=Deserialize<T>(Database.StringGet(key));
            Database.KeyDelete(key);
            return t;
        }

        public T GetItem<T>(string key)
        {
            return Deserialize<T>(Database.StringGet(key));
        }

        public object GetItem(string key, bool remove)
        {
            var t = Deserialize<object>(Database.StringGet(key));
            Database.KeyDelete(key);
            return t;
        }

        public object GetItem(string key)
        {
            return Deserialize<object>(Database.StringGet(key));
        }

        public void RemoveItem(string key)
        {
            Database.KeyDelete(key);
        }


        public Task<bool> AddItemAsync(string key, object value)
        {
            return Database.StringSetAsync(key, Serialize(value));
        }
        
        
        public Task<bool> AddItemAsync(string key, object value, DateTimeOffset dateTimeOffset)
        {
            var span = dateTimeOffset.UtcDateTime - DateTimeOffset.UtcNow;
            return Database.StringSetAsync(key, Serialize(value),span);
        }
        [Obsolete("Don't use this method anywhere!!!")]
        public Task<bool> AddItemAsync(string key, object value, CacheItemPolicy policy)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetItemAsync<T>(string key, bool remove)
        {
            var t= Deserialize<T>(await Database.StringGetAsync(key));
            await Database.KeyDeleteAsync(key);
            return t;
            //Database.Wait(t);
            //if(t.IsCompleted)
            //{
            //return Task.FromResult(Deserialize<T>(t.Result));
            //}
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            return Deserialize<T>(await Database.StringGetAsync(key));
        }

        public async Task<object> GetItemAsync(string key, bool remove)
        {
            var t = Deserialize<object>(await Database.StringGetAsync(key));
            await Database.KeyDeleteAsync(key);
            return t;
        }

        public async Task<object> GetItemAsync(string key)
        {
            return Deserialize<object>(await Database.StringGetAsync(key));
        }

        public async Task<bool> RemoveItemAsync(string key)
        {
            return await Database.KeyDeleteAsync(key);
        }
    }
}
