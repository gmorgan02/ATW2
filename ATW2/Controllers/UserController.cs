using ATW2.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ATW2.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<UserController> _logger;


        public UserController(IConfiguration config, ILogger<UserController> logger)
        {
            _config = config;
            _logger = logger;
        }

        public IActionResult Login()
        {
            ValidateLogin("test", "test");
            return View();
        }

        public IActionResult ManageUser()
        {
            if(User.IsInRole("Admin"))
            {
                return View();
            }
            else
            {
                return View("Error");
            }
        }

        public bool ValidateLogin(string email, string password)
        {
            var sqlQuery = "SELECT * FROM User";

            ExcecuteSql(sqlQuery);

            return true;
        }

        public bool UpdateUser(string id, string email, string password, UserRoleEnum role)
        {            
            var sqlQuery = "Update (Email, Password, Role) SET Email=" + email + "; Password=" + password + "; Role=" + role.ToString() +"; FROM User WHERE Id =" + id;
            
            ExcecuteSql(sqlQuery);

            return true;
        }

        public bool DeleteUser(string id)
        {
            var sqlQuery = "DELETE FROM User WHERE Id =" + id;

            ExcecuteSql(sqlQuery);

            return true;
        }

        private void ExcecuteSql(string sqlQuery)
        {
            var serverConnection = _config.GetValue<string>("Database:Connection");
            MySqlConnection SqlConnection = new MySqlConnection(serverConnection);

            try
            {
                SqlConnection.Open();
                MySqlCommand sqlCommand = new MySqlCommand(sqlQuery, SqlConnection);
                var result = sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                SqlConnection.Close();

            }
            catch (MySqlException exception)
            {
                _logger.LogError("Sql Exception: ", exception);
                return;
            }

        }
    }
}
