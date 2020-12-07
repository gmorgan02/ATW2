using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ATW2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ATW2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CardController(IConfiguration config)
        {
            _config = config;
        }
        // GET: api/<CardController>
        [HttpGet]
        public async Task<IEnumerable<string>> GetDeckAsync()
        {
            var client = new HttpClient();
            var ApiEndpoint = _config.GetValue<string>("Api:Url") + "new/shuffle/?deck_count=1";
            var response = await client.GetStringAsync(ApiEndpoint);
            Deck deck = JsonConvert.DeserializeObject<Deck>(response);
           
            return new string[] { "value1", "value2" };
        }

        // GET api/<CardController>/{deckId}/draw
        [HttpGet("{deckId}/draw/{numberOfCards}")]
        public async Task<string> GetCardAsync(string deckId, string numberOfCards)
        {
            var client = new HttpClient();
            var ApiEndpoint = _config.GetValue<string>("Api:Url") + deckId + "/draw/?count=" + numberOfCards;
            var response = await client.GetStringAsync(ApiEndpoint);
            
            CardJson cards = JsonConvert.DeserializeObject<CardJson>(response);
            
            return "value";
        }

        // GET api/<CardController>/{deckId}/shuffle
        [HttpGet("{deckId}/shuffle")]
        public async Task<string> ShuffleDeckAsync(string deckId)
        {
            var client = new HttpClient();
            var ApiEndpoint = _config.GetValue<string>("Api:Url") + deckId + "/shuffle";
            var response = await client.GetStringAsync(ApiEndpoint);

            return "";
        }
    }
}
