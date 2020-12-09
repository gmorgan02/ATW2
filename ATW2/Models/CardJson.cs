using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATW2.Models
{
    //model for the json that is returned for the deckofcards api 
    public class CardJson
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("deck_id")]
        public string Id { get; set; }
        [JsonProperty("remaining")]
        public int Remaining { get; set; }
        [JsonProperty("cards")]
        public IList<Card> Cards { get; set; }
    }
}
