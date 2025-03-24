using System.Reflection;
using IdroAlertER.Application;
using IdroAlertER.Services.Providers;
using IdroAlertER.Services.Services;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{
	private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

	static async Task Main(string[] args)
	{
		string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config");
		Console.WriteLine($"Loading log4net config from: {configFilePath}");

		var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
		XmlConfigurator.Configure(logRepository, new FileInfo(configFilePath));

		var builder = Host.CreateDefaultBuilder(args)
			.UseWindowsService() // Configura il processo come un servizio Windows
			.ConfigureAppConfiguration((context, config) =>
			{
				config.SetBasePath(AppContext.BaseDirectory)
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					.AddEnvironmentVariables();
			})
			.ConfigureLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddProvider(new Log4NetLoggerProvider()); // Usa log4net per file logging
				logging.AddEventLog(); // Aggiunge il logging sugli eventi di Windows
			})
			.ConfigureServices((hostContext, services) =>
			{
				services.AddHostedService<MyBackgroundService>(); // Registra il servizio personalizzato
				services.AddIdroAlertERApplication();
			});

		await builder.Build().RunAsync();
	}
}
