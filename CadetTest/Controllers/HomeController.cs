using CadetTest.Entities;
using CadetTest.Models;
using CadetTest.Models.ResultModels;
using CadetTest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.OpenApi.Validations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CadetTest.Controllers
{
    public class HomeController : Controller
    {
        private IDataService _dataService;

        public HomeController(IDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ConsentsIndex(int totalCount = 0)
        {
            try
            {

                string urlAuthenticate = "http://localhost:26173/api/User/authenticate";
                AuthenticateRequest authenticateRequest = new AuthenticateRequest()
                {
                    Username = "ConnectTssService",
                    Password = "aqgrVjoEP8OtAwD7UB3Y0hNcKKHU4gWrCbupoa2Gr5w="
                };
                var dataString = JsonConvert.SerializeObject(authenticateRequest);
                AuthenticateResponse authenticateResponse;
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    string resultJSON = client.UploadString(urlAuthenticate, "POST", dataString);

                    authenticateResponse = JsonConvert.DeserializeObject<AuthenticateResponse>(resultJSON);
                }


                // api aracılığıyla 10as 10ar kayıtları alıyorum
                string urlConsents = "http://localhost:26173/api/Consents";
                List<Consent> consents = new List<Consent>();
                int count = totalCount / 10 > 0 ? totalCount : (_dataService.GetTotalCountofConsents() / 10) + 1;

                for (int i = 1; i < count; i++)  //10ar 10 ar alıyoruz
                {
                    ConsentRequest request = new ConsentRequest()
                    {
                        Count = 10,
                        StartId = consents.Count + 1
                    };

                    dataString = JsonConvert.SerializeObject(request);
                    List<Consent> data = new List<Consent>();
                    using (WebClient client = new WebClient()) // HttpClient WebClient
                    {

                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                        client.Headers.Add("Authorization", "Bearer " + authenticateResponse.JwtToken);
                        string resultJSON = client.UploadString(urlConsents, "POST", dataString);
                        data = JsonConvert.DeserializeObject<List<Consent>>(resultJSON);
                        consents.AddRange(data);
                    }
                }

                return View(consents.OrderByDescending(x => x.Id).ToList());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Beklenmedik hata oluştu!");
                return View(new List<Consent>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Consent());

        }

        [HttpPost]
        public IActionResult Create(Consent model)
        {
            try
            {
                var result = _dataService.AddNewConsent(model, out int consentsCount);
                if (result.IsSuccess)
                {
                    TempData["CreateSuccessMsg"] = result.Message;
                    return RedirectToAction("ConsentsIndex", "Home", new { totalCount = consentsCount });
                }
                else
                {
                    ViewBag.Error = result.Message;
                    return View(model);
                }

            }
            catch (Exception)
            {

                ViewBag.Error = "Beklenmedik bir hata oluştu! Tekrar deneyiniz!";
                return View(model);
            }

        }

        public IActionResult Edit(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    TempData["EditErrorMsg"] = "id değeri gelmediğinde işlem gerçekleşemez!";
                    return RedirectToAction("ConsentsIndex", "Home");
                }

                var data = _dataService.GetById(id.Value).Data;
                return View(data);
            }
            catch (Exception ex)
            {

                TempData["EditErrorMsg"] = "Beklenmedik bir hata oluştu! Tekrar deneyiniz!";
                return RedirectToAction("ConsentsIndex", "Home", new { totalCount = _dataService.GetTotalCountofConsents() });
            }


        }

        [HttpPost]
        public IActionResult Edit(Consent model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Error = "Verileri düzgün formatta girmelisiniz!";
                    return View(model);
                }

                var data = _dataService.GetById(model.Id).Data;
                if (data==null)
                {
                    ViewBag.Error = "Verileri düzgün formatta girmelisiniz!";
                    return View(model);
                }
                data.Status = model.Status;
                data.Recipient = model.Recipient;
                data.RecipientType = model.RecipientType;
                data.Type = model.Type;
                var result = _dataService.UpdateConsent(data);
                if (result.IsSuccess)
                {
                    TempData["EditSuccessMsg"] = "Beklenmedik bir hata oluştu! Tekrar deneyiniz!";
                    return RedirectToAction("ConsentsIndex", "Home", new { totalCount = _dataService.GetTotalCountofConsents() });

                }
                else
                {
                    ViewBag.Error = "Verileri düzgün formatta girmelisiniz!";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["EditErrorMsg"] = "Beklenmedik bir hata oluştu! Tekrar deneyiniz!";
                return RedirectToAction("ConsentsIndex", "Home", new { totalCount = _dataService.GetTotalCountofConsents() });
            }
        }
    }
}
