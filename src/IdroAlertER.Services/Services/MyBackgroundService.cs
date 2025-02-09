using IdroAlertER.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;

public class MyBackgroundService : BackgroundService
{
	private readonly ILogger<MyBackgroundService> _logger;
	private readonly ILivelloIdrometricoBLService _livelloIdrometricoBLService;
	private readonly CrontabSchedule _schedule;
	private DateTime _nextRun;

	public MyBackgroundService(ILogger<MyBackgroundService> logger, IConfiguration configuration, ILivelloIdrometricoBLService livelloIdrometricoBLService)
	{
		_logger = logger;
		_schedule = CrontabSchedule.Parse(configuration.GetSection("Cron").Get<string>() ?? "1,16,31,46 * * * *");
		_livelloIdrometricoBLService = livelloIdrometricoBLService;
		_nextRun = _schedule.GetNextOccurrence(DateTime.Now);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Servizio avviato.");

		while (!stoppingToken.IsCancellationRequested)
		{
			var now = DateTime.Now;

			if (now >= _nextRun)
			{
				try
				{
					_logger.LogInformation("Esecuzione job alle {time}", now);
					await _livelloIdrometricoBLService.ExecuteAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Errore durante l'esecuzione del job");
				}

				_nextRun = _schedule.GetNextOccurrence(DateTime.Now); // Calcola la prossima esecuzione
			}

			await Task.Delay(1000, stoppingToken); // Controlla ogni secondo
		}
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Servizio in arresto.");
		return base.StopAsync(cancellationToken);
	}
}
