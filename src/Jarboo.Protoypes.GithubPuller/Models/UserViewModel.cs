﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jarboo.Protoypes.GithubPuller.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}