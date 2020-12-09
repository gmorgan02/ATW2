using ATW2.Enum;
using System;

namespace ATW2.Models
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRoleEnum Role { get; set; }
    }
}
