using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed.ToDateTime(new TimeOnly(0, 0)));
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("cars/{carId:long}/claims")]
    public async Task<ActionResult<ClaimDtoResponse>> RegisterClaim(long carId, [FromBody] ClaimDtoReq registerClaimReq)
    {
        try
        {
            ClaimDtoResponse claim = await _service.RegisterClaimAsync(carId, registerClaimReq);
            return CreatedAtAction(nameof(RegisterClaim), new { carId }, claim);
            //Should have been nameof(GetClaim) if implemented instead of nameof(RegisterClaim)
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }


    [HttpGet("cars/{carId:long}/history")]
    public async Task<ActionResult<CarHistoryDtoResponse>> GetCarHistory(long carId)
    {
        try
        {
            CarHistoryDtoResponse carHistory = await _service.GetCarHistoryAsync(carId);
            return Ok(carHistory);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
