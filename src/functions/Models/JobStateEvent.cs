using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace functions.Models
{
    public class JobStateEvent
    {
        [JsonProperty("PreviousState")]
        public string PreviousState { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }
    }
}
