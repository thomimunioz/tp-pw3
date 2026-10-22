using Microsoft.AspNetCore.Identity;
using RuteaYa.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Account
{
    public class ResultadoRegistro
    {
        public bool Exito { get; set; }
        public Usuario? Usuario { get; set; }
        public List<string> Errores { get; set; } = new();
    }

    public interface IAccountServicios
    {
        Task<ResultadoRegistro> RegistrarAsync(string nombre, string apellido, string email, string password);
        Task<Usuario?> ValidarCredencialesAsync(string email, string password);
    }

    public class AccountServicios : IAccountServicios
    {
        private readonly UserManager<Usuario> _userManager;

        public AccountServicios(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ResultadoRegistro> RegistrarAsync(string nombre, string apellido, string email, string password)
        {
            var usuario = new Usuario
            {
                UserName = email,
                Email = email,
                Nombre = nombre,
                Apellido = apellido,
                CreadoEn = DateTime.UtcNow
            };

            var resultado = await _userManager.CreateAsync(usuario, password);

            if (resultado.Succeeded)
            {
                return new ResultadoRegistro { Exito = true, Usuario = usuario };
            }

            var errores = new List<string>();
            foreach (var error in resultado.Errors)
            {
                if (error.Code == "DuplicateUserName")
                    continue;

                errores.Add(Traducir(error));
            }

            return new ResultadoRegistro { Exito = false, Errores = errores };
        }

        public async Task<Usuario?> ValidarCredencialesAsync(string email, string password)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null)
                return null;

            var passwordCorrecta = await _userManager.CheckPasswordAsync(usuario, password);
            return passwordCorrecta ? usuario : null;
        }

        private static string Traducir(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateEmail" => "Ya existe una cuenta con ese email",
                "PasswordTooShort" => "La contraseña debe tener al menos 8 caracteres",
                "PasswordRequiresUpper" => "La contraseña debe tener al menos una mayúscula",
                "PasswordRequiresLower" => "La contraseña debe tener al menos una minúscula",
                "PasswordRequiresDigit" => "La contraseña debe tener al menos un número",
                "PasswordRequiresNonAlphanumeric" => "La contraseña debe tener al menos un símbolo",
                _ => error.Description
            };
        }
    }
}
