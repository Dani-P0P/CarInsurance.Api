using System.ComponentModel.DataAnnotations;

namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record ClaimDtoReq([Required] DateOnly ClaimDate, string? Description, [Range(0, double.MaxValue)] decimal Amount);
public record ClaimDtoResponse(long Id, DateOnly ClaimDate, string? Description, decimal Amount);
