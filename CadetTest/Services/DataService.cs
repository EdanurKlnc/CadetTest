using CadetTest.Entities;
using CadetTest.Helpers;
using CadetTest.Models;
using CadetTest.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CadetTest.Services
{
    public interface IDataService
    {
        string GetRandomString(int stringLength);
        void InitConsents();
        List<Consent> GetRangeById(int id, int adet);
        IDataResult<Consent> AddNewConsent(Consent data, out int consentsCount);
        IDataResult<Consent> UpdateConsent(Consent data);
        IDataResult<Consent> GetById(int id);
        IResult DeleteConsent(int id);
        public int GetTotalCountofConsents();


    }
    public class DataService : IDataService
    {
        private DataContext _context;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UserService> _logger;
        private Random _random;

        public DataService(DataContext context, IOptions<AppSettings> appSettings, ILogger<UserService> logger)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _logger = logger;

            _random = new Random();
        }

        #region Public Methods

        public List<Consent> GetRangeById(int id, int adet)
        {
            var cevap = _context.Consents.Where(k => k.Id >= id).Take(adet).ToList();
            return cevap;
        }
        #endregion


        public void InitConsents()
        {
            if (_context.Consents.Any()) return;

            for (int i = 1; i < _appSettings.ConsentCount; i++)
            {
                var consent = new Consent
                {
                    Recipient = $"{GetRandomString(10)}_{i}@ornek.com",
                    RecipientType = "EPOSTA",
                    Status = "ONAY",
                    Type = "EPOSTA"
                };

                _context.Consents.Add(consent);
            }
        }

        #region Private Methods

        public string GetRandomString(int stringLength)
        {
            var sb = new StringBuilder();
            int numGuidsToConcat = (((stringLength - 1) / 32) + 1);
            for (int i = 1; i <= numGuidsToConcat; i++)
            {
                sb.Append(Guid.NewGuid().ToString("N"));
            }

            return sb.ToString(0, stringLength);
        }
        #endregion
        public IDataResult<Consent> AddNewConsent(Consent data, out int consentsCount)
        {
            try
            {
                consentsCount = 0;
                if (_context.Consents.FirstOrDefault(x => x.Recipient == data.Recipient) != null)
                {
                    return new DataResult<Consent>(data, "Bu emailde bir kayut zaten mevcuttur!", false);
                }
                data.Id = _context.Consents.Count() + 1;
                _context.Consents.Add(data);
                var result = _context.SaveChanges();
                consentsCount = _context.Consents.Count();
                return result > 0 ?
                    new DataResult<Consent>(data, "Ekleme başarılı", true) :
                    new DataResult<Consent>(data, "Ekleme BAŞARISIZ", false);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public IDataResult<Consent> UpdateConsent(Consent data)
        {
            try
            {
                var consent = _context.Consents.FirstOrDefault(x => x.Id == data.Id);
                if (consent == null)
                {
                    return new DataResult<Consent>(false, null);
                }

                consent.Type = data.Type;
                consent.Recipient = data.Recipient;
                consent.Status = data.Status;
                consent.RecipientType = data.RecipientType;
                return _context.SaveChanges() > 0 ?
                    new DataResult<Consent>(data, "Güncelleme başarılı", true) :
                    new DataResult<Consent>(data, "Güncelleme BAŞARISIZ", false);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IResult DeleteConsent(int id)
        {
            try
            {
                var consent = _context.Consents.FirstOrDefault(x => x.Id == id);
                if (consent == null)
                {
                    return new DataResult<Consent>(false, null);
                }
                _context.Consents.Remove(consent);
                return _context.SaveChanges() > 0 ?
                                   new Result(true, "Silme işlemi Başarılıdır!") :
                                   new Result(true, "Silme işlemi BAŞARISIZDIR!");

            }
            catch (Exception)
            {

                throw;
            }
        }


        public IDataResult<Consent> GetById(int id)
        {
            try
            {
                var consent = _context.Consents.FirstOrDefault(x => x.Id == id);
                if (consent == null)
                {
                    return new DataResult<Consent>(false, null);
                }

                return new DataResult<Consent>(true, consent);
            }
            catch (Exception)
            {

                throw;
            }
        }



        public int GetTotalCountofConsents()
        {
            try
            {
                return _context.Consents.Count();
            }
            catch (Exception)
            {

                return 0;
            }
        }
    }
}
