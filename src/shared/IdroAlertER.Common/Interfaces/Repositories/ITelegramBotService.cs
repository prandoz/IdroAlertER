namespace IdroAlertER.Common.Interfaces.Repositories;
public interface ITelegramBotService
{
	Task SendAsync(string message);
}
