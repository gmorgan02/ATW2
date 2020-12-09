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
        // GET: <CardController>
        // gets the deck asyncronusly enable the webpage to still be interacted with during loading
        [HttpGet]
        public async Task<IEnumerable<string>> GetDeckAsync()
        {
            var client = new HttpClient();
            var ApiEndpoint = _config.GetValue<string>("Api:Url") + "new/shuffle/?deck_count=1";
            var response = await client.GetStringAsync(ApiEndpoint);
            Deck deck = JsonConvert.DeserializeObject<Deck>(response);

            if (!deck.Success)
            {
                return new string[] { "Error" };
            }
           
            return new string[] { deck.Id, deck.Remaining.ToString(), deck.Shuffled.ToString(), deck.Success.ToString() };
        }

        // GET <CardController>/{deckId}/draw
        //draws two cards from the deck
        [HttpGet("{deckId}/draw/2")]
        public async Task<IEnumerable<string>> GetCardsAsync(string deckId)
        {
            //makes the api call
            var client = new HttpClient();
            var ApiEndpoint = _config.GetValue<string>("Api:Url") + deckId + "/draw/?count=2";
            var response = await client.GetStringAsync(ApiEndpoint);
            
            //gets the response and sets it up into the c# model
            CardJson cards = JsonConvert.DeserializeObject<CardJson>(response);

            //check for success
            if (!cards.Success)
            {
                return new string[] { "Error" };
            }

            //assign result to card object that will be returned in an array string 
            Card card1 = new Card();
            Card card2 = new Card();

            card1.Code = cards.Cards[0].Code;
            card1.Image = cards.Cards[0].Image;
            card1.Suit = cards.Cards[0].Suit;
            card1.Value = cards.Cards[0].Value;

            card2.Code = cards.Cards[1].Code;
            card2.Image = cards.Cards[1].Image;
            card2.Suit = cards.Cards[1].Suit;
            card2.Value = cards.Cards[1].Value;

            return new string[] { card1.Code, card1.Image, card1.Suit, card1.Value, card2.Code, card2.Image, card2.Suit, card2.Value };            
        }

        // GET <CardController>/{deckId}/draw
        [HttpGet("{deckId}/draw/1")]
        //draws 1 card from the deck
        public async Task<IEnumerable<string>> GetCardAsync(string deckId)
        {
            //makes the api call
            var client = new HttpClient();
            var ApiEndpoint = _config.GetValue<string>("Api:Url") + deckId + "/draw/?count=1";
            var response = await client.GetStringAsync(ApiEndpoint);

            //maps the JSON response to c# model
            CardJson cards = JsonConvert.DeserializeObject<CardJson>(response);

            //check result is okay
            if (!cards.Success)
            {
                return new string[] { "Error" };
            }

            //assign result to card object that will be returned in an array string 
            Card card1 = new Card();

            card1.Code = cards.Cards[0].Code;
            card1.Image = cards.Cards[0].Image;
            card1.Suit = cards.Cards[0].Suit;
            card1.Value = cards.Cards[0].Value;


            return new string[] { card1.Code, card1.Image, card1.Suit, card1.Value };
        }

        // GET <CardController>/{deckId}/shuffle
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
