using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace functions.Models
{
    public class EventBlobStorage
    {
        public string Api { get; set; }

        public string ClientRequestId { get; set; }

        public string RequestId { get; set; }

        public string Url { get; set; }

        public string ContentType { get; set; }

        public string ContentLength { get; set; }
    }
}
