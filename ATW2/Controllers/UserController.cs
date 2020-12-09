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
        //show login view
        public IActionResult Login()
        {
            var session = HttpContext.Session.GetString("UserSession");

            try
            {
                var userSession = JsonConvert.DeserializeObject<User>(session);
                return RedirectToAction("BlackJackTable", "BlackJack");
            }
            catch
            {
                return View();
            }
        }

        //recieve login information and validate if okay, direct to blackjack api
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if(email == null || password == null)
            {
                _logger.LogError("Parsed credentials are null");
                return View("PermissionsError");
            }

            if(!ValidateLogin(email, password))
            {
                return View("PermissionsError");
            }
                    
            return RedirectToAction("BlackJackTable", "BlackJack");
        }

        //return logout view
        public IActionResult Logout()
        {
            return View();
        }

        //Clears the user's session and takes the user back to the login screen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout(string e)
        {           
            HttpContext.Session.Clear();      

            return RedirectToAction("Login");
        }

        //query the database, gets all the users and outputs them to a table on the manage view
        public IActionResult Manage()
        {
            if (!CheckAdmin())
            {
                return View("PermissionsError");
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
        //return edit view and check user is an admin
        public IActionResult Edit()
        {
            if (!CheckAdmin())
            {
                return View("PermissionsError");
            }

            return View();
        }

        //update the user record in the database from the user input on the view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string email, string password, UserRoleEnum role)
        {
            if (!CheckAdmin())
            {
                return View("PermissionsError");
            }

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


        //check user is admin and returns create view
        public IActionResult Create()
        {
            if (!CheckAdmin())
            {
                return View("PermissionsError");
            }

            return View();
        }

        //takes recieved information from view and inserts user record into database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string email, string password, UserRoleEnum role)
        {
            if (!CheckAdmin())
            {
                return View("PermissionsError");
            }

            if (email == "" || password == "" || role.ToString() == "")
            {
                return View("Error");
            }

            if (!InsertUser(email, password, role))
            {
                return View("Error");
            }
            return RedirectToAction("Manage");
        }
        //checks the user is an admin and returns the delete confirmation view 
        public IActionResult Delete(List<User> users)
        {
            if (!CheckAdmin())
            {
                return View("PermissionsError");
            }

            return View();
        }

        //checks the user is an admin and deletes the chosen user record from the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!CheckAdmin())
            {
                return View("PermissionsError");
            }

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
        //checks the user's login details match what is in the database
        private bool ValidateLogin(string email, string password)
        {
            var sqlQuery = "SELECT * FROM User WHERE Email = @Email;";

            var serverConnection = _config.GetValue<string>("Database:Connection");
            MySqlConnection SqlConnection = new MySqlConnection(serverConnection);

            try
            {
                SqlConnection.Open();
                MySqlCommand sqlCommand = new MySqlCommand(sqlQuery, SqlConnection);
                sqlCommand.Parameters.AddWithValue("@Email", email);
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

            if (email == "" && password == "" && role.ToString() == "" || email == null && password == null && role.ToString() == null)
            {
                return false;
            }

            if (email != "" && email != null)
            {
                sqlQuery += " Email= @Email,";
            }
        
            if (password != "" && password != null)
            {
                sqlQuery += " Password = @Password,";
            }

            if (role.ToString() != "" && role.ToString() != null)
            {
                sqlQuery += " Role = @Role";
            }

            sqlQuery += " WHERE Id = @Id";

            try
            {
                var serverConnection = _config.GetValue<string>("Database:Connection");
                MySqlConnection SqlConnection = new MySqlConnection(serverConnection);

                SqlConnection.Open();
                MySqlCommand sqlCommand = new MySqlCommand(sqlQuery, SqlConnection);
                sqlCommand.Parameters.AddWithValue("@Email", email);
                sqlCommand.Parameters.AddWithValue("@Id", id);
                sqlCommand.Parameters.AddWithValue("@Password", password);
                sqlCommand.Parameters.AddWithValue("@Role", role.ToString());
                var result = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                SqlConnection.Close();             
            }
            catch (Exception exception)
            {
                _logger.LogError("Sql Exception: ", exception);
                return false;
            }
            
            return true;
        }

        private bool DeleteUser(int id)
        {
            var sqlQuery = "DELETE FROM User WHERE Id = @Id";

            try
            {
                var serverConnection = _config.GetValue<string>("Database:Connection");
                MySqlConnection SqlConnection = new MySqlConnection(serverConnection);

                SqlConnection.Open();
                MySqlCommand sqlCommand = new MySqlCommand(sqlQuery, SqlConnection);
                sqlCommand.Parameters.AddWithValue("@Id", id);
                var result = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                SqlConnection.Close();
            }
            catch (Exception exception)
            {
                _logger.LogError("Sql Exception: ", exception);
                return false;
            }

            return true;
        }
        
        private bool InsertUser(string email, string password, UserRoleEnum role)
        {
            var sqlQuery = "INSERT INTO User (Email, Password, Role) VALUES (@Email, @Password,@Role)";

            try
            {
                var serverConnection = _config.GetValue<string>("Database:Connection");
                MySqlConnection SqlConnection = new MySqlConnection(serverConnection);

                SqlConnection.Open();
                MySqlCommand sqlCommand = new MySqlCommand(sqlQuery, SqlConnection);
                sqlCommand.Parameters.AddWithValue("@Email", email);
                sqlCommand.Parameters.AddWithValue("@Password", password);
                sqlCommand.Parameters.AddWithValue("@Role", role.ToString());
                var result = sqlCommand.ExecuteReader();
                sqlCommand.Dispose();
                SqlConnection.Close();
            }
            catch (Exception exception)
            {
                _logger.LogError("Sql Exception: ", exception);
                return false;
            }

            return true;
        }

        private bool CheckAdmin()
        {
            var session = HttpContext.Session.GetString("UserSession");

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError("Exception deserializing userSession: ", ex);
                return false;
            }
            
            
        }
    }
}
