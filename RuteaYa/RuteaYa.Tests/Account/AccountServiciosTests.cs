using Microsoft.AspNetCore.Identity;
using Moq;
using RuteaYa.Entidades;
using RuteaYa.Logica.Account;

namespace RuteaYa.Tests.Account;

public class AccountServiciosTests
{
    private readonly Mock<UserManager<Usuario>> _userManager;
    private readonly AccountServicios _servicio;

    public AccountServiciosTests()
    {
        var store = new Mock<IUserStore<Usuario>>();
        _userManager = new Mock<UserManager<Usuario>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _servicio = new AccountServicios(_userManager.Object);
    }

    // RegistrarAsync

    [Fact]
    public async Task RegistrarAsync_DatosCorrectos_DevuelveExitoYElUsuario()
    {
        // Arrange
        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<Usuario>(), "Clave1234"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var resultado = await _servicio.RegistrarAsync("Ana", "Perez", "ana@mail.com", "Clave1234");

        // Assert
        Assert.True(resultado.Exito);
        Assert.Empty(resultado.Errores);
        Assert.NotNull(resultado.Usuario);
        Assert.Equal("Ana", resultado.Usuario.Nombre);
        Assert.Equal("Perez", resultado.Usuario.Apellido);
        Assert.Equal("ana@mail.com", resultado.Usuario.Email);
        Assert.Equal("ana@mail.com", resultado.Usuario.UserName);
    }

    [Fact]
    public async Task RegistrarAsync_EmailRepetido_DevuelveUnSoloErrorEnEspanol()
    {
        // Arrange: Identity manda dos errores cuando el email ya existe
        var fallo = IdentityResult.Failed(
            new IdentityError { Code = "DuplicateUserName", Description = "Username 'ana@mail.com' is already taken." },
            new IdentityError { Code = "DuplicateEmail", Description = "Email 'ana@mail.com' is already taken." });

        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(fallo);

        // Act
        var resultado = await _servicio.RegistrarAsync("Ana", "Perez", "ana@mail.com", "Clave1234");

        // Assert
        Assert.False(resultado.Exito);
        Assert.Null(resultado.Usuario);
        var mensaje = Assert.Single(resultado.Errores);
        Assert.Equal("Ya existe una cuenta con ese email", mensaje);
    }

    [Fact]
    public async Task RegistrarAsync_ContrasenaCorta_DevuelveMensajeTraducido()
    {
        // Arrange
        var fallo = IdentityResult.Failed(
            new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 8 characters." });

        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(fallo);

        // Act
        var resultado = await _servicio.RegistrarAsync("Ana", "Perez", "ana@mail.com", "abc");

        // Assert
        Assert.False(resultado.Exito);
        var mensaje = Assert.Single(resultado.Errores);
        Assert.Equal("La contraseña debe tener al menos 8 caracteres", mensaje);
    }

    [Fact]
    public async Task RegistrarAsync_ErrorSinTraduccion_DevuelveElMensajeOriginal()
    {
        // Arrange
        var fallo = IdentityResult.Failed(
            new IdentityError { Code = "ErrorInventado", Description = "Mensaje original" });

        _userManager
            .Setup(m => m.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(fallo);

        // Act
        var resultado = await _servicio.RegistrarAsync("Ana", "Perez", "ana@mail.com", "Clave1234");

        // Assert
        var mensaje = Assert.Single(resultado.Errores);
        Assert.Equal("Mensaje original", mensaje);
    }

    // ValidarCredencialesAsync

    [Fact]
    public async Task ValidarCredencialesAsync_EmailInexistente_DevuelveNullYNoRevisaLaContrasena()
    {
        // Arrange
        _userManager
            .Setup(m => m.FindByEmailAsync("nadie@mail.com"))
            .ReturnsAsync((Usuario?)null);

        // Act
        var usuario = await _servicio.ValidarCredencialesAsync("nadie@mail.com", "Clave1234");

        // Assert
        Assert.Null(usuario);
        _userManager.Verify(
            m => m.CheckPasswordAsync(It.IsAny<Usuario>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidarCredencialesAsync_ContrasenaIncorrecta_DevuelveNull()
    {
        // Arrange
        var existente = new Usuario { Email = "ana@mail.com" };

        _userManager
            .Setup(m => m.FindByEmailAsync("ana@mail.com"))
            .ReturnsAsync(existente);
        _userManager
            .Setup(m => m.CheckPasswordAsync(existente, "mala"))
            .ReturnsAsync(false);

        // Act
        var usuario = await _servicio.ValidarCredencialesAsync("ana@mail.com", "mala");

        // Assert
        Assert.Null(usuario);
    }

    [Fact]
    public async Task ValidarCredencialesAsync_CredencialesCorrectas_DevuelveElUsuario()
    {
        // Arrange
        var existente = new Usuario { Email = "ana@mail.com" };

        _userManager
            .Setup(m => m.FindByEmailAsync("ana@mail.com"))
            .ReturnsAsync(existente);
        _userManager
            .Setup(m => m.CheckPasswordAsync(existente, "Clave1234"))
            .ReturnsAsync(true);

        // Act
        var usuario = await _servicio.ValidarCredencialesAsync("ana@mail.com", "Clave1234");

        // Assert
        Assert.Same(existente, usuario);
    }
}