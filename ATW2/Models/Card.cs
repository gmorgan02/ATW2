using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATW2.Models
{
    public class Card : Deck
    { 
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("suit")]
        public string Suit { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
