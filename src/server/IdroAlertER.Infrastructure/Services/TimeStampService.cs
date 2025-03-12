using System.Globalization;
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
		DateTime modifiedTime = RoundDownToNearest15Minutes(localTime);

		// Convertire la data modificata in timestamp Unix (millisecondi)
		return ((DateTimeOffset)modifiedTime).ToUnixTimeMilliseconds();
	}

	public long GetBefore(long timeStamp)
	{
		var newDateTime = RoundDownToNearest15Minutes(DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).LocalDateTime);
		return Convert(newDateTime.ToString("dd/MM/yyyy"), newDateTime.ToString("HH:mm"));
	}

	public long GetBeforeOneHour()
	{
		var oldDateTime = DateTime.Now.AddHours(-1);
		return Convert(oldDateTime.ToString("dd/MM/yyyy"), oldDateTime.ToString("HH:mm"));
	}

	public long Convert(string date, string time)
	{
		string format = "dd/MM/yyyy HH:mm"; // Formato atteso

		// Convertire la stringa in DateTime con il formato specifico
		if (!DateTime.TryParseExact($"{date} {time}", format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime localTime))
		{
			throw new ArgumentException("Formato data/ora non valido. Usa il formato dd/MM/yyyy HH:mm");
		}

		// Definire il fuso orario italiano (CET/CEST a seconda del periodo dell'anno)
		TimeZoneInfo italianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

		// Otteniamo il corretto offset UTC per quella data
		TimeSpan offset = italianTimeZone.GetUtcOffset(localTime);

		// Convertiamo in UTC manualmente sottraendo l'offset
		DateTime utcTime = localTime - offset;

		// Convertiamo in timestamp Unix (millisecondi)
		return new DateTimeOffset(utcTime).ToUnixTimeMilliseconds();
	}

	private static DateTime RoundDownToNearest15Minutes(DateTime dateTime)
	{
		int minutes = dateTime.Minute / 15 * 15; // Trova il multiplo inferiore di 15 minuti

		// Se i minuti sono già multipli di 15 ed è maggiore di 0, scaliamo di 15
		if (dateTime.Minute == minutes && minutes != 0)
		{
			minutes -= 15;
		}

		return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, minutes, 0);
	}
}
