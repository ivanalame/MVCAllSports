using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MVCAllSports.Models;
using MVCAllSports.Services;
using System.Security.Claims;

namespace MVCAllSports.Controllers
{
    public class ManagedController : Controller
    {
        private ServiceDeportes service; 

        public ManagedController(ServiceDeportes service)
        {
            this.service = service;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            string token = await this.service.GetTokenAsync(model.Email, model.Password);
            if(token == null)
            {
                ViewData["MENSAJE"] = "Usuario/Password incorrectos";
                return View();
            }
            else
            {
                HttpContext.Session.SetString("TOKEN" ,token);

                   //SEGURIDAD 
                ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

                //Claim claimEmail = new Claim(ClaimTypes.Name, model.Email);
                // Claim claimPassword = new Claim("password", model.Password);
                identity.AddClaim(new Claim(ClaimTypes.Name, model.Email));
                identity.AddClaim(new Claim("password", model.Password));
                identity.AddClaim(new Claim("TOKEN", token));

                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);


                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);
                //LO VAMOS A LLEVAR A UNA VISTA CON LA INFORMACION QUE NOS DEVUELVE EL FILTER EN TEMPDATA
                string controller = TempData["controller"].ToString();

                string action = TempData["action"].ToString();

                if (TempData["id"] != null)
                {
                    string id = "";
                    id = TempData["id"].ToString();
                    return RedirectToAction(action, controller, new { id = id });
                }
                else
                {
                    return RedirectToAction(action, controller);
                }
            }
           }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync
                (CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Deportes");
        }
    }
}
