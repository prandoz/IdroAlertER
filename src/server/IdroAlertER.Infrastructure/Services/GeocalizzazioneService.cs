using System.Globalization;
using IdroAlertER.Common.Entities;
using IdroAlertER.Common.Interfaces.Services;

namespace IdroAlertER.Infrastructure.Services;
internal class GeocalizzazioneService : IGeocalizzazioneService
{
	public string ConvertiLatitudine(ValoriStazione valoriStazione) => ConvertiDDDToDMSUri(valoriStazione.Lat!, true);

	public string ConvertiLongitudine(ValoriStazione valoriStazione) => ConvertiDDDToDMSUri(valoriStazione.Lon!, false);

	public string ConvertiDDDToDMSUri(string coordinataStr, bool isLatitudine)
	{
		if (!double.TryParse(coordinataStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double coordinata))
		{
			return "Formato non valido";
		}

		coordinata /= 100000.0;
		int gradi = (int)coordinata;
		double minutiDecimali = (Math.Abs(coordinata) - Math.Abs(gradi)) * 60;
		int minuti = (int)minutiDecimali;
		double secondi = (minutiDecimali - minuti) * 60;

		char direzione = isLatitudine
			? (coordinata >= 0 ? 'N' : 'S')
			: (coordinata >= 0 ? 'E' : 'W');

		return $"{Math.Abs(gradi)}%C2%B0{minuti}'{secondi.ToString("F1").Replace(',', '.')}%22{direzione}";
	}
}
