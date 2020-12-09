using Newtonsoft.Json;

namespace ATW2.Models
{
    public class Deck
    {
        //set json property so that in the newtonsoft deserialize the data models across correctly
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("deck_id")]
        public string Id { get; set; }
        [JsonProperty("remaining")]
        public int Remaining { get; set; }
        [JsonProperty("shuffled")]
        public bool Shuffled { get; set; }
    }
}
