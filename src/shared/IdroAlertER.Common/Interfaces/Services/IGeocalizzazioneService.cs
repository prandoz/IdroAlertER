using IdroAlertER.Common.Entities;

namespace IdroAlertER.Common.Interfaces.Services;
public interface IGeocalizzazioneService
{
	string ConvertiLatitudine(ValoriStazione valoriStazione);

	string ConvertiLongitudine(ValoriStazione valoriStazione);

	string ConvertiDDDToDMSUri(string coordinataStr, bool isLatitudine);
}
