using IdroAlertER.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
	static async Task Main(string[] args)
	{
		var host = CreateHostBuilder(args).Build();
		await host.RunAsync();
	}

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureServices((context, services) =>
			{
				// Aggiungi la configurazione per leggere appsettings.json
				services.AddSingleton<IConfiguration>(context.Configuration);

				// Aggiungi i servizi dell'applicazione
				services.AddIdroAlertERApplication();
			});
}
