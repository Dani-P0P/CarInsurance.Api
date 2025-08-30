using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace CarInsurance.Api.Testing;

public class InsuranceValidityTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public InsuranceValidityTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient GetClientWithCleanDb()
    {
        var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var car = new Car
            {
                Vin = "TESTVIN",
                Make = "Dacia",
                Model = "Logan",
                YearOfManufacture = 2023,
                Owner = new Owner { Name = "Test", Email = "t@test.com" }
            };
            db.Cars.Add(car);
            db.Policies.Add(new InsurancePolicy
            {
                Car = car,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31),
                Provider = "Allianz"
            });
            db.SaveChanges();
        }
        return client;
    }

    [Fact]
    public async Task InsuranceValid_InvalidDate_ShouldReturn400()
    {
        var client = GetClientWithCleanDb();
        var response = await client.GetAsync("/api/cars/1/insurance-valid?date=2025-99-99");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InsuranceValid_NonExistingCar_ShouldReturn404()
    {
        var client = GetClientWithCleanDb();
        var response = await client.GetAsync("/api/cars/999/insurance-valid?date=2025-06-01");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InsuranceValid_OnStartDate_ShouldBeTrue()
    {
        var client = GetClientWithCleanDb();
        var response = await client.GetAsync("/api/cars/1/insurance-valid?date=2025-01-01");
        var content = await response.Content.ReadFromJsonAsync<InsuranceValidityResponse>();
        content!.Valid.Should().BeTrue();
    }

    [Fact]
    public async Task InsuranceValid_OnEndDate_ShouldBeTrue()
    {
        var client = GetClientWithCleanDb();
        var response = await client.GetAsync("/api/cars/1/insurance-valid?date=2025-12-31");
        var content = await response.Content.ReadFromJsonAsync<InsuranceValidityResponse>();
        content!.Valid.Should().BeTrue();
    }

    [Fact]
    public async Task InsuranceValid_NoDateProvided_ShouldReturn400()
    {
        var client = GetClientWithCleanDb();
        var response = await client.GetAsync("/api/cars/1/insurance-valid");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InsuranceValid_EmptyDate_ShouldReturn400()
    {
        var client = GetClientWithCleanDb();
        var response = await client.GetAsync("/api/cars/1/insurance-valid?date=");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InsuranceValid_CarWithoutPolicy_ShouldBeFalse()
    {
        var client = GetClientWithCleanDb();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Cars.Add(new Car { Vin = "NOPOLICY", Make = "VW", Model = "Golf", YearOfManufacture = 2022, Owner = new Owner { Name = "NoPol", Email = "np@test.com" } });
            db.SaveChanges();
        }

        var response = await client.GetAsync("/api/cars/2/insurance-valid?date=2025-06-01");
        var content = await response.Content.ReadFromJsonAsync<InsuranceValidityResponse>();
        content!.Valid.Should().BeFalse();
    }
}
