using IdroAlertER.Common.Interfaces.Repositories;
using IdroAlertER.Common.Interfaces.Services;
using IdroAlertER.Infrastructure.Repositories;
using IdroAlertER.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdroAlertER.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddIdroAlertERInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<ILivelloIdrometricoHttpService, LivelloIdrometricoHttpService>();
		services.AddScoped<ITimeStampService, TimeStampService>();

		return services;
	}
}
