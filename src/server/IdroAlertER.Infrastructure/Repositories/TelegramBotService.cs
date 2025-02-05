using IdroAlertER.Common.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace IdroAlertER.Infrastructure.Repositories;
internal class TelegramBotService(IConfiguration configuration) : ITelegramBotService
{
	private readonly string _botToken = configuration.GetSection("BOT Token").Get<string>() ?? string.Empty;
	private readonly string _idCanale = configuration.GetSection("ID canale Telegram").Get<string>() ?? string.Empty;

	public async Task SendAsync(string message)
	{
		var botClient = new TelegramBotClient(_botToken);
		await botClient.SendTextMessageAsync(_idCanale, message);
	}
}
