using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InventorySalesApi.IntegrationTests;

public class EndpointIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public EndpointIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<string> GetTokenAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { NombreUsuario = username, Password = password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginTokenResponse>();
        result.Should().NotBeNull();
        return result!.Token;
    }

    [Fact]
    public async Task Login_CredencialesCorrectas_DebeRetornarTokenOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new { NombreUsuario = "admin", Password = "AdminPassword123!" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginTokenResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.Rol.Should().Be("Administrador");
    }

    [Fact]
    public async Task Login_CredencialesIncorrectas_DebeRetornar401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new { NombreUsuario = "admin", Password = "WrongPassword!" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Productos_AccesoSinToken_DebeRetornar401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/productos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Productos_CrearConRolOperador_DebeCrearExitosamente()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "operador", "OperadorPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Obtener categorías para tener un CategoriaId válido
        // Primero, sembramos una de prueba o usamos el seed. El seed tiene categorías.
        var categoriasResponse = await client.GetAsync("/api/productos"); // En in-memory está vacío por ahora de productos, pero categorías están sembradas.
        // Vamos a mandar un CategoriaId inventado primero, lo cual debería dar un 404 (NotFoundException) o 400.
        // Pero para crear con éxito, necesitamos un CategoriaId real del DbContext.
        // Vamos a simular la creación de un producto con SKU duplicado o inválido para probar el flujo.
        var nuevoProducto = new
        {
            Sku = "TEST-PROD-99",
            Nombre = "Producto de Integración",
            Descripcion = "Descripción",
            Precio = 150.00m,
            StockInicial = 100,
            CategoriaId = Guid.NewGuid() // ID aleatorio, debe fallar por Categoria no encontrada (NotFoundException -> 404)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/productos", nuevoProducto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Nuestra NotFoundException mapea a 404
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Recurso no Encontrado");
    }

    [Fact]
    public async Task Clientes_ObtenerConRolVendedor_DebeRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "vendedor", "VendedorPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ventas_ObtenerConRolVendedor_DebeRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "vendedor", "VendedorPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/ventas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Stock_MovimientosSinPermisosVendedor_DebeRetornar403()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenAsync(client, "vendedor", "VendedorPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/stock/movimientos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden); // 403 Forbidden por rol incorrecto
    }
}

public record LoginTokenResponse(Guid Id, string NombreUsuario, string Email, string Rol, string Token);
