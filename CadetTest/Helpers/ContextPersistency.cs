using CadetTest.Entities;
using CadetTest.Helpers.CryptoHelper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CadetTest.Helpers
{
    public interface IContextPersistency
    {
        List<User> ContextUsers { get; set; }

        void ReadUserContext();
        void AddUser(User user);
        void RemoveUser(User user);
        void UpdateUser(User user);
        void InitConnectTssServiceUser();
    }

    public class ContextPersistency : IContextPersistency
    {
        private const string SERVICE_USER = "ConnectTssService";
        private const string _aesPassword = "W6}3_9k9V$S%IU?";
        private static DataContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ContextPersistency> _logger;
        private const string _userContextFile = @"\data\users.json";
        private string _userContextPath;
        public List<User> ContextUsers { get; set; }

        public ContextPersistency(DataContext context, IWebHostEnvironment env, ILogger<ContextPersistency> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _userContextPath = $@"{_env.ContentRootPath}{_userContextFile}";

            ContextUsers = new List<User>();

            InitDataFile(_userContextPath);

            ReadUserContext();
        }

        public void ReadUserContext()
        {
            try
            {
                if (_userContextPath != null)
                {
                    var json = File.ReadAllText(_userContextPath);

                    if (string.IsNullOrEmpty(json)) return;

                    ContextUsers = JsonConvert.DeserializeObject<List<User>>(json);

                    foreach (var user in ContextUsers)
                    {
                        lock (ContextUsers)
                        {
                            if (!_context.Users.Any(u => u.Username == user.Username))
                            {
                                _context.Users.Add(user);
                            }
                        }
                    }

                    lock (ContextUsers)
                    {
                        _context.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Context dosyasi okunamadi");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new Exception(e.Message);
            }
        }

        public void AddUser(User user)
        {
            try
            {
                lock (ContextUsers)
                {
                    if (_context.Users.Any(u => u.Username == user.Username)) return;

                    user.Password = AES.Encrypt(user.Password, _aesPassword);

                    _context.Users.Add(user);
                    _context.SaveChanges();


                    ContextUsers.Add(user);

                    var json = JsonConvert.SerializeObject(ContextUsers);
                    File.WriteAllText(_userContextPath, json);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new Exception(e.Message);
            }
        }

        public void RemoveUser(User user)
        {
            try
            {
                lock (ContextUsers)
                {
                    ContextUsers.Remove(user);

                    var json = JsonConvert.SerializeObject(ContextUsers);
                    File.WriteAllText(_userContextPath, json);

                    _context.Users.Remove(user);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new Exception(e.Message);
            }
        }

        public void UpdateUser(User user)
        {
            try
            {
                lock (ContextUsers)
                {
                    if (!ContextUsers.Any(u => u.Id == user.Id))
                    {
                        throw new Exception("User bulunamadi");
                    }

                    ContextUsers.Remove(user);

                    _context.Users.Update(user);
                    _context.SaveChanges();

                    ContextUsers.Add(user);

                    var json = JsonConvert.SerializeObject(ContextUsers);
                    File.WriteAllText(_userContextPath, json);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new Exception(e.Message);
            }
        }

        public void InitConnectTssServiceUser()
        {
            try
            {
                if (ContextUsers.Any(u => u.Username == SERVICE_USER)) return;

                var user = new User
                {
                    Username = SERVICE_USER,
                    Password = SERVICE_USER,
                    RefreshTokens = new List<RefreshToken>()
                };

                AddUser(user);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new Exception(e.Message);
            }
        }

        private void InitDataFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}
