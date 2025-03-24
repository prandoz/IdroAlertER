using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Logging;

namespace IdroAlertER.Services.Providers;

public class Log4NetLogger(string name) : ILogger
{
	private readonly ILog _log = LogManager.GetLogger(name);

	public IDisposable? BeginScope<TState>(TState state) => null;

	public bool IsEnabled(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Critical => _log.IsFatalEnabled,
			LogLevel.Error => _log.IsErrorEnabled,
			LogLevel.Warning => _log.IsWarnEnabled,
			LogLevel.Information => _log.IsInfoEnabled,
			LogLevel.Debug => _log.IsDebugEnabled,
			LogLevel.Trace => _log.IsDebugEnabled,
			_ => false,
		};
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		string message = formatter(state, exception);

		switch (logLevel)
		{
			case LogLevel.Critical: _log.Fatal(message, exception); break;
			case LogLevel.Error: _log.Error(message, exception); break;
			case LogLevel.Warning: _log.Warn(message, exception); break;
			case LogLevel.Information: _log.Info(message, exception); break;
			case LogLevel.Debug:
			case LogLevel.Trace: _log.Debug(message, exception); break;
		}
	}
}

public class Log4NetLoggerProvider : ILoggerProvider
{
	public Log4NetLoggerProvider()
	{
		var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config");

		var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
		XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
	}

	public ILogger CreateLogger(string categoryName)
	{
		return new Log4NetLogger(categoryName);
	}

	public void Dispose() { }
}
