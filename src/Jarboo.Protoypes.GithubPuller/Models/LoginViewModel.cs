using DataAnnotationsExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Jarboo.Protoypes.GithubPuller.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Du mangler at udfylde din username")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Du mangler at udfylde dit kodeord"), DisplayName("Kodeord")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}