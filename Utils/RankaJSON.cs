using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ranka.Utils
{
    public static class RankaJSON
    {
        public class CommunicationableMessage
        {
            [JsonProperty("trigger")]
            public string Trigger { get; set; }

            [JsonProperty("response")]
            public string Response { get; set; }
        }

        public class Responses
        {
            [JsonProperty("communicationable_messages")]
            public List<CommunicationableMessage> CommunicationableMessages { get; set; }
        }
    }
}