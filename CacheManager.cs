using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lu.Caching.Redis
{
    public class CacheManager : CachingProviderBase,ICache 
    {
        public CacheManager(string connectString)
            : base(connectString)
        {

        }
    }
}
