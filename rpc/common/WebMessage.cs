using System.Collections.Generic;

namespace TFramework.Web
{
    [System.Serializable]
    public struct WebMessage
    {
        public string ServiceId { get; set; }

        public List<WebMetadata> Parameters { get; set; }
    }
}