using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Logica.Dashboard;
using RuteaYa.Web.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace RuteaYa.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardServicios _dashboard;

        public HomeController(IDashboardServicios dashboard)
        {
            _dashboard = dashboard;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var dto = await _dashboard.ObtenerAsync(usuarioId);
            return View(dto);
        }

        [AllowAnonymous]
        public IActionResult NoEncontrado()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
