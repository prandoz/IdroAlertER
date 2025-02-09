using IdroAlertER.Application.Services;
using IdroAlertER.Common.Interfaces.Services;
using IdroAlertER.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IdroAlertER.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddIdroAlertERApplication(this IServiceCollection services)
	{
		services.AddIdroAlertERInfrastructure();
		services.AddScoped<ILivelloIdrometricoBLService, LivelloIdrometricoBLService>();

		return services;
	}
}
