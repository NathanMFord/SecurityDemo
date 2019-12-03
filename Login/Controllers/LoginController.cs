using System;
using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase {
        [HttpGet]
        public bool Get([FromBody]Models.Login login) {
            return login.Username == "admin" && login.Password == "password";
        }

        [HttpPost]
        public bool Post([FromBody]Models.Login login) {
            return login.Username == "admin" && login.Password == "password";
        }
    }
}