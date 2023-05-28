using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CadetTest.Entities
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public int ExpirationInMinutes { get; set; }

        public int ConsentCount { get; set; }
    }
}
