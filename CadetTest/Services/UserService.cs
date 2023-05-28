using CadetTest.Entities;
using CadetTest.Helpers;
using CadetTest.Helpers.CryptoHelper;
using CadetTest.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static CadetTest.Helpers.ContextPersistency;

namespace CadetTest.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        IEnumerable<User> GetAll();
        User GetById(int id);
    }

    public class UserService : IUserService
    {
        private const string _aesPassword = "W6}3_9k9V$S%IU?";
        private DataContext _context;
        private IContextPersistency _persistentContext;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UserService> _logger;

        public UserService(DataContext context, IContextPersistency persistentContext, IOptions<AppSettings> appSettings, ILogger<UserService> logger)
        {
            _context = context;
            _persistentContext = persistentContext;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress = "127.0.0.1")
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);
                var password = AES.Encrypt(model.Password, _aesPassword);
                //var user = _persistentContext.ContextUsers.SingleOrDefault(x => x.Username == model.Username && x.Password == password);
                if (user == null) return null;

                var expirationDate = DateTime.UtcNow.AddMinutes(_appSettings.ExpirationInMinutes);

                var jwtToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken(ipAddress);
                user.RefreshTokens = new List<RefreshToken>();
                user.RefreshTokens.Add(refreshToken);//NNI.0001
                //_persistentContext.UpdateUser(user);
                _context.Update(user);
                _context.SaveChanges();

                return new AuthenticateResponse(user, jwtToken, expirationDate, refreshToken.Token);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw new Exception($"Hata: {e.Message}");
            }
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            try
            {
                //var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
                var user = _persistentContext.ContextUsers.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

                if (user == null) return null;

                var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

                if (!refreshToken.IsActive) return null;

                var newRefreshToken = GenerateRefreshToken(ipAddress);
                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReplacedByToken = newRefreshToken.Token;
                user.RefreshTokens.Add(newRefreshToken);
                _persistentContext.UpdateUser(user);
                //_context.Update(user);
                //_context.SaveChanges();

                var expirationDate = DateTime.UtcNow.AddMinutes(_appSettings.ExpirationInMinutes);
                var jwtToken = GenerateJwtToken(user);

                return new AuthenticateResponse(user, jwtToken, expirationDate, newRefreshToken.Token);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw new Exception($"Hata: {e.Message}");
            }
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            try
            {
                //var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
                var user = _persistentContext.ContextUsers.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

                if (user == null) return false;

                var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

                if (!refreshToken.IsActive) return false;

                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                _persistentContext.UpdateUser(user);
                //_context.Update(user);
                //_context.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw new Exception($"Hata: {e.Message}");
            }
        }

        public IEnumerable<User> GetAll()
        {
            return _persistentContext.ContextUsers;
        }

        public User GetById(int id)
        {
            return _persistentContext.ContextUsers.First(u => u.Id == id);
        }

        // helper methods

        private string GenerateJwtToken(User user)  
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(_appSettings.ExpirationInMinutes),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw new Exception($"Hata: {e.Message}");
            }
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            try
            {
                using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
                {
                    var randomBytes = new byte[64];
                    rngCryptoServiceProvider.GetBytes(randomBytes);
                    return new RefreshToken
                    {
                        Token = Convert.ToBase64String(randomBytes),
                        Expires = DateTime.UtcNow.AddMinutes(_appSettings.ExpirationInMinutes),
                        Created = DateTime.UtcNow,
                        CreatedByIp = ipAddress
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw new Exception($"Hata: {e.Message}");
            }
        }
    }
}
