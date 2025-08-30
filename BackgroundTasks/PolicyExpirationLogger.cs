using CarInsurance.Api.Services;

namespace CarInsurance.Api.BackgroundTasks
{
    public class PolicyExpirationLogger : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<PolicyExpirationLogger> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(55);

        public PolicyExpirationLogger(IServiceProvider services, ILogger<PolicyExpirationLogger> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var carService = scope.ServiceProvider.GetRequiredService<CarService>();

                    var expiredPolicies = await carService.GetExpiredPoliciesLastHourAsync();

                    foreach (var policy in expiredPolicies)
                    {
                        _logger.LogInformation($"Insurance policy for car {policy.Car.Vin} expired at {policy.EndDate}");
                    }

                    await carService.MarkPoliciesAsLoggedAsync(expiredPolicies);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while logging expired policies");
                }

                await Task.Delay(_checkInterval, cancellationToken);
            }
        }
    }
}
