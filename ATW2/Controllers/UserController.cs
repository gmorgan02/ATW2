using ATW2.Enum;
using ATW2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if(email == null || password == null)
            {
                _logger.LogError("Parsed credentials are null");
                return View("Error");
            }

            if(!ValidateLogin(email, password))
            {
                return View("Error");
            }
                    
            return RedirectToAction("BlackJackTable");
        }

        public IActionResult Manage()
        {
            if (!CheckAdmin())
            {
                return View("Error");
            }           

            var sqlQuery = "SELECT * FROM User";

            var serverConnection = _config.GetValue<string>("Database:Connection");
            MySqlConnection SqlConnection = new MySqlConnection(serverConnection);

            try
            {
                SqlConnection.Open();
                MySqlCommand sqlCommand = new MySqlCommand(sqlQuery, SqlConnection);
                var result = sqlCommand.ExecuteReader();

                
                List<User> listUsers = new List<User>();

                while (result.Read())
                {
                    User user = new User();
                    user.Id = (int)result["Id"];
                    user.Email = (string)result["Email"];
                    user.Password = (string)result["Password"];
                    var str = (string)result["Role"];
                    user.Role = (UserRoleEnum)System.Enum.Parse(typeof(UserRoleEnum), str);
                   

                    listUsers.Add(user);
                }

                sqlCommand.Dispose();
                SqlConnection.Close();

                return View(listUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError("exception: ", ex);
                return View("error");
            }
        }

        public IActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string email, string password, UserRoleEnum role)
        {
            if (email == "" || password == "" || role.ToString() == "")
            {
                return View("Error");
            }

            if (!UpdateUser(id, email, password, role))
            {
                return View("Error");
            }
            return RedirectToAction("Manage");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string email, string password, UserRoleEnum role)
        {
            if(email == "" || password == "" || role.ToString() == "")
            {
                return View("Error");
            }

            if (!InsertUser(email, password, role))
            {
                return View("Error");
            }
            return RedirectToAction("Manage");
        }

        public IActionResult Delete(List<User> users)
        {            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return View("Error");
            }

            if (!DeleteUser(id))
            {
                return View("Error");
            }
            return RedirectToAction("Manage");
        }

        private bool ValidateLogin(string email, string password)
        {
            var sqlQuery = "SELECT * FROM User WHERE Email = " +"'" + email + "'";

            var serverConnection = _config.GetValue<string>("Database:Connection");
            MySqlConnection SqlConnection = new MySqlConnection(serverConnection);

            try
            {
                SqlConnection.Open();
                MySqlCommand sqlCommand = new MySqlCommand(sqlQuery, SqlConnection);
                var result = sqlCommand.ExecuteReader();

                User user = new User();

                while (result.Read())
                {
                    user.Id = (int)result["Id"];
                    user.Email = (string)result["Email"];
                    user.Password = (string)result["Password"];
                    var str = (string)result["Role"];
                    user.Role = (UserRoleEnum)System.Enum.Parse(typeof(UserRoleEnum), str);
                }

                sqlCommand.Dispose();
                SqlConnection.Close();

                if (user.Password == password)
                {

                    var userSession = new User();

                    userSession.Id = user.Id;
                    userSession.Email = user.Email;
                    userSession.Role = user.Role;

                    var userSessionJson = JsonConvert.SerializeObject(userSession);

                    HttpContext.Session.SetString("UserSession", userSessionJson);

                    return true;
                }

                

            }
            catch (MySqlException exception)
            {
                _logger.LogError("Sql Exception: ", exception);
                return false;
            }

            return true;
        }

        private bool UpdateUser(int id, string email, string password, UserRoleEnum role)
        {
            var sqlQuery = "Update User SET";

            if (email == "" || password == "" || role.ToString() == "")
            {
                return false;
            }

            if (email == "")
            {

            }
            else
            {
                sqlQuery += " Email=" + "'" + email + "'" + ",";
            }

            if (password == "")
            {
                
            }
            else
            {
                sqlQuery += " Password = " + "'" + password + "'" + ",";
            }

            if (role.ToString() == "")
            {

            }
            else
            {
                sqlQuery += " Role = " + "'" + role.ToString() + "'";
            }

            sqlQuery += " WHERE Id =" + id;

            try
            {
                ExcecuteSql(sqlQuery);
            }
            catch
            {
                return false;
            }
            
            return true;
        }

        private bool DeleteUser(int id)
        {
            var sqlQuery = "DELETE FROM User WHERE Id =" + id;

            try
            {
                ExcecuteSql(sqlQuery);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private bool InsertUser(string email, string password, UserRoleEnum role)
        {
            var sqlQuery = "INSERT INTO User (Email, Password, Role) VALUES ('" + email + "'" + ", '" + password + "'" + ", '" + role.ToString() + "'" + ")";

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
                var result = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                SqlConnection.Close();
                
            }
            catch (MySqlException exception)
            {
                _logger.LogError("Sql Exception: ", exception);
                return;
            }

        }

        private bool CheckAdmin()
        {
            var session = HttpContext.Session.GetString("UserSession");

            var userSession = JsonConvert.DeserializeObject<User>(session);

            if (userSession.Role != UserRoleEnum.Admin)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
