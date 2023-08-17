using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TFramework.Web
{
    public class WebServiceEntry
    {
        public Func<List<WebMetadata>, Task<object>> Func { get; set; }

        public string ServiceId { get; set; }

        public object Instance { get; set; }
    }
}
