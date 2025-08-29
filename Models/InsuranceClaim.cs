namespace CarInsurance.Api.Models
{
    public class InsuranceClaim
    {
        public long Id { get; set; }
        public DateOnly ClaimDate { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }

        public long CarId { get; set; }
        public Car Car { get; set; } = default!;
    }
}
