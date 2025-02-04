using IdroAlertER.Common.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace IdroAlertER.Infrastructure.Repositories;
internal class TelegramBotService : ITelegramBotService
{
	private readonly string _botToken;
	private readonly string _idCanale;

	public TelegramBotService(IConfiguration configuration)
	{
		_botToken = configuration.GetSection("BOT Token").Get<string>() ?? string.Empty;
		_idCanale = configuration.GetSection("ID canale Telegram").Get<string>() ?? string.Empty;
	}

	public async Task SendAsync(string message)
	{
		var botClient = new TelegramBotClient(_botToken);
		await botClient.SendTextMessageAsync(_idCanale, message);
	}
}
