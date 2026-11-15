using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Logica.Historial;
using RuteaYa.Logica.Historial.Dtos;
using RuteaYa.Web.Models.Historial;
using RuteaYa.Entidades.Enums;

namespace RuteaYa.Web.Controllers;

    [Authorize]
    public class HistorialController : Controller
        {

        private readonly IHistorialServicios _historialServicios;
        private readonly UserManager<Usuario> _userManager;

        public HistorialController(IHistorialServicios historialServicios, UserManager<Usuario> userManager)
            {
            _historialServicios = historialServicios;
            _userManager = userManager;
            }

        [HttpGet]
        public async Task<IActionResult> Index(int? mes, int? anio, int? estado)
            {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                {
                return Challenge();
                }

        EstadoEnvio? estadoEnum = estado.HasValue ? (EstadoEnvio)estado.Value : null;

        var filtro = new FiltroHistorialDto
            {
                Mes = mes,
                Anio = anio,
                Estado = estadoEnum
            };

        var resumen = await _historialServicios.ObtenerHistorialAsync(usuario.Id, filtro);
        var viewModel = new HistorialViewModel
            {
                Resumen = resumen,
                MesSeleccionado = mes,
                AnioSeleccionado = anio,
                EstadoSeleccionado = estadoEnum
            };

        return View(viewModel);
        }
    }