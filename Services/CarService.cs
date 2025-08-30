using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            (p.EndDate == null || p.EndDate >= date)
        );
    }

    public async Task<ClaimDtoResponse> RegisterClaimAsync(long carId, ClaimDtoReq registerClaimReq)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        InsuranceClaim claim = new InsuranceClaim
        {
            ClaimDate = registerClaimReq.ClaimDate,
            Description = registerClaimReq.Description,
            Amount = registerClaimReq.Amount,
            CarId = carId
        };

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();

        return new ClaimDtoResponse(claim.Id, claim.ClaimDate, claim.Description, claim.Amount);
    }

    public async Task<CarHistoryDtoResponse> GetCarHistoryAsync(long carId)
    {
        var car = await _db.Cars.Include(c => c.Policies).Include(c => c.Claims)
            .FirstOrDefaultAsync(c => c.Id == carId);

        if (car == null)
        {
            throw new KeyNotFoundException($"Car {carId} not found");
        }

        var policies = car.Policies.Select(p =>
            new CarHistoryDto(
                p.StartDate,
                "Insurance Policy",
                p.Provider,
                p.EndDate,
                null,
                null
                ));

        var claims = car.Claims.Select(c =>
            new CarHistoryDto(
                c.ClaimDate,
                "Insurance Claim",
                null,
                null,
                c.Description,
                c.Amount
                ));

        var historyEvents = policies.Concat(claims)
            .OrderBy(e => e.Date)
            .ToList();

        return new CarHistoryDtoResponse(car.Id, historyEvents);
    }
}
