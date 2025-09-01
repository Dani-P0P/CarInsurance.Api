using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<InsurancePolicy> Policies => Set<InsurancePolicy>();
    public DbSet<InsuranceClaim> Claims => Set<InsuranceClaim>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasIndex(c => c.Vin)
            .IsUnique(); // TODO: set true and handle conflicts

        modelBuilder.Entity<InsurancePolicy>()
            .Property(p => p.StartDate)
            .IsRequired();

        modelBuilder.Entity<InsurancePolicy>()
            .Property(p => p.EndDate)
            .IsRequired();

        // EndDate intentionally left nullable for a later task

        modelBuilder.Entity<InsuranceClaim>()
            .Property(cl => cl.ClaimDate)
            .IsRequired();
    }
}

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Owners.Any()) return;

        var ana = new Owner { Name = "Ana Pop", Email = "ana.pop@example.com" };
        var bogdan = new Owner { Name = "Bogdan Ionescu", Email = "bogdan.ionescu@example.com" };
        db.Owners.AddRange(ana, bogdan);
        db.SaveChanges();

        var car1 = new Car { Vin = "VIN12345", Make = "Dacia", Model = "Logan", YearOfManufacture = 2018, OwnerId = ana.Id };
        var car2 = new Car { Vin = "VIN67890", Make = "VW", Model = "Golf", YearOfManufacture = 2021, OwnerId = bogdan.Id };
        var car3 = new Car { Vin = "VIN67890", Make = "VW", Model = "Passat", YearOfManufacture = 2020, OwnerId = bogdan.Id };
        var cars = new List<Car> { car1, car2, car3 };
        foreach (var car in cars)
        {
            if (db.Cars.Any(c => c.Vin == car.Vin) ||
                db.ChangeTracker.Entries<Car>().Any(c => c.Entity.Vin == car.Vin))
            {
                Console.WriteLine($"A car with VIN {car.Vin} already exists and will not be added!");
            }
            else
            {
                db.Cars.Add(car);
            }
        }
        //db.Cars.AddRange(car1, car2, car3);
        db.SaveChanges();

        db.Policies.AddRange(
            new InsurancePolicy { CarId = car1.Id, Provider = "Allianz", StartDate = new DateOnly(2024, 1, 1), EndDate = new DateTime(2025, 8, 30, 16, 10, 0) },
            new InsurancePolicy { CarId = car1.Id, Provider = "Groupama", StartDate = new DateOnly(2025, 1, 1), EndDate = new DateTime(2025, 8, 30, 17, 0, 0) }, // open-ended on purpose
            new InsurancePolicy { CarId = car2.Id, Provider = "Allianz", StartDate = new DateOnly(2025, 3, 1), EndDate = new DateTime(2025, 8, 30, 17, 15, 0) }
        );
        db.SaveChanges();
    }
}
