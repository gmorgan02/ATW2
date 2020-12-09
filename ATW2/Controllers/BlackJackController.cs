using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ATW2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ATW2.Controllers
{
    public class BlackJackController : Controller
    {
        private readonly ILogger<UserController> _logger;

        public BlackJackController(ILogger<UserController> logger)
        {
            _logger = logger;
        }
        // GET: BlackJackController
        public ActionResult BlackJackTable()
        {
            var session = HttpContext.Session.GetString("UserSession");

            try
            {
                var userSession = JsonConvert.DeserializeObject<User>(session);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception deserializing userSession: ", ex);
                return View("Error");
            }

            return View();
        }       
    }
}
