using IdroAlertER.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{
	static async Task Main(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args)
			.UseWindowsService() // Configura il processo come un servizio Windows
			.ConfigureServices((hostContext, services) =>
			{
				services.AddHostedService<MyBackgroundService>(); // Registra il servizio personalizzato
				services.AddIdroAlertERApplication();
			})
			.ConfigureLogging(logging =>
			{
				logging.AddEventLog(); // Aggiunge il logging sugli eventi di Windows
			});

		await builder.Build().RunAsync();
	}

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration((context, config) =>
			{
				//config.SetBasePath(AppContext.BaseDirectory)
				//	.AddJsonFile($"appsettings.Development.json", optional: false, reloadOnChange: true)
				//	.AddEnvironmentVariables();

				config.SetBasePath(AppContext.BaseDirectory)
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					.AddEnvironmentVariables();
			})
			.ConfigureServices((context, services) =>
			{
				// Aggiungi la configurazione per leggere appsettings.json
				services.AddSingleton<IConfiguration>(context.Configuration);

				// Aggiungi i servizi dell'applicazione
				//services.AddIdroAlertERApplication();
			});
}
