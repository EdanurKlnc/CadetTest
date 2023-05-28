using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CadetTest.Models
{
    public class NewUserRequest
    {
        [Required]
        public string TesisKodu { get; set; }

        [Required]
        public string TesisAdi { get; set; }
    }
}
