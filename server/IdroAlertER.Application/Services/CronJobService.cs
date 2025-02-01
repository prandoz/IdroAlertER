using IdroAlertER.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NCrontab;

namespace IdroAlertER.Application.Services;

// Classe che rappresenta il servizio eseguito come HostedService
internal class CronJobService : IHostedService
{
	private readonly IConfiguration _configuration;
	private readonly ILivelloIdrometricoBLService _livelloIdrometricoBLService;
	private Timer _timer;
	private readonly CrontabSchedule _schedule;
	private DateTime _nextRun;

	public CronJobService(IConfiguration configuration, ILivelloIdrometricoBLService livelloIdrometricoBLService)
	{
		_configuration = configuration;
		_livelloIdrometricoBLService = livelloIdrometricoBLService;
		//string cronExpression = _configuration["CronSettings:CronExpression"];
		string cronExpression = "1,16,31,46 * * * *";
		_schedule = CrontabSchedule.Parse(cronExpression);
		_nextRun = _schedule.GetNextOccurrence(DateTime.Now);
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		// TODO: rimuovere
		await _livelloIdrometricoBLService.ExecuteAsync();

		// Calcola il tempo rimanente fino al prossimo job
		TimeSpan timeUntilNextRun = _nextRun - DateTime.Now;
		_timer = new Timer(ExecuteJobAsync, null, timeUntilNextRun, Timeout.InfiniteTimeSpan);

		//return Task.CompletedTask;
	}

	private async void ExecuteJobAsync(object state)
	{
		await _livelloIdrometricoBLService.ExecuteAsync();

		// Pianifica la prossima esecuzione
		_nextRun = _schedule.GetNextOccurrence(DateTime.Now);
		TimeSpan timeUntilNextRun = _nextRun - DateTime.Now;
		_timer.Change(timeUntilNextRun, Timeout.InfiniteTimeSpan);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_timer?.Change(Timeout.Infinite, 0);
		return Task.CompletedTask;
	}
}
