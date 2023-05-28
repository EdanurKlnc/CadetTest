using CadetTest.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CadetTest.Models
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }

        public DateTime ExpirationDate { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }
        public AuthenticateResponse()
        {

        }
        public AuthenticateResponse(User user, string jwtToken, DateTime expirationDate, string refreshToken)
        {
            Id = user.Id;
            Username = user.Username;
            JwtToken = jwtToken;
            ExpirationDate = expirationDate;
            RefreshToken = refreshToken;
        }
    }
}
