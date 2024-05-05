
using Microsoft.AspNetCore.Mvc;
using MVCAllSports.Models;
using MVCAllSports.Services;

namespace MVCAllSports.ViewComponents
{
    public class MenuDeportesViewComponent: ViewComponent
    {
        private ServiceDeportes service; 

        public MenuDeportesViewComponent(ServiceDeportes service)
        {
            this.service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Usuario user = await this.service.GetPerfilUsuarioAsync();
                ViewData["USERROLE"] = user.IdRolUsuario;
            }
            List<Deporte> deportes =await this.service.GetDeportesAsync();
            return View(deportes);

        }
    }
}
