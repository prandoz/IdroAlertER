using IdroAlertER.Common.Interfaces.Services;

namespace IdroAlertER.Infrastructure.Services;
internal class TimeStampService : ITimeStampService
{
	public long Get()
	{
		// Ottenere l'ora attuale in Italia (fuso orario CET)
		TimeZoneInfo italianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
		DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, italianTimeZone);

		// Sottrarre un minuto
		DateTime modifiedTime = localTime.AddMinutes(-1);

		// Convertire la data modificata in timestamp Unix (millisecondi)
		long unixTimestamp = ((DateTimeOffset)modifiedTime).ToUnixTimeMilliseconds();

		// Restituisce il timestamp Unix
		return unixTimestamp;
	}

	public long GetBefore(long timeStamp)
	{
		// Timestamp Unix in millisecondi (esempio)
		long unixTimestamp = 1738256400000;

		// Convertire il timestamp Unix in DateTime (UTC)
		DateTime dateTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp).UtcDateTime;

		// Sottrarre 15 minuti
		DateTime newDateTimeUtc = dateTimeUtc.AddMinutes(-15);

		// Convertire la nuova data in timestamp Unix (millisecondi)
		long newUnixTimestamp = ((DateTimeOffset)newDateTimeUtc).ToUnixTimeMilliseconds();

		return newUnixTimestamp;
	}
}
