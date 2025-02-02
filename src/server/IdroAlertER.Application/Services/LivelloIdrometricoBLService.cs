using System.Text;
using IdroAlertER.Application.Entities;
using IdroAlertER.Common.Entities.Results;
using IdroAlertER.Common.Interfaces.Repositories;
using IdroAlertER.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace IdroAlertER.Application.Services;
internal class LivelloIdrometricoBLService : ILivelloIdrometricoBLService
{
	private readonly ITimeStampService _timeStampService;
	private readonly ILivelloIdrometricoHttpService _livelloIdrometricoHttpService;
	private readonly long _timeStampAttuale;
	private readonly List<string> _nomiStazioni;
	private List<IdroAlertHttpResult>? _valoriStazioniAttuali;
	private List<IdroAlertHttpResult>? _valoriStazioniPrecedenti;
	private List<IdroAlertHttpResult>? _valoriStazioni;

	public LivelloIdrometricoBLService(IConfiguration configuration, ITimeStampService timeStampService, ILivelloIdrometricoHttpService livelloIdrometricoHttpService)
	{
		_timeStampService = timeStampService;
		_livelloIdrometricoHttpService = livelloIdrometricoHttpService;
		_timeStampAttuale = timeStampService.Get();
		_nomiStazioni = configuration.GetSection("NomiStazioni").Get<List<string>>() ?? new();
	}

	public async Task ExecuteAsync()
	{
		_valoriStazioniAttuali = await _livelloIdrometricoHttpService.GetAsync(_timeStampAttuale);
		// TODO: rimuovere
		_valoriStazioniAttuali = await _livelloIdrometricoHttpService.GetAsync(1738437300000);
		_valoriStazioniPrecedenti = await _livelloIdrometricoHttpService.GetAsync(_timeStampService.GetBefore(_timeStampAttuale));

		foreach (var nomeStazione in _nomiStazioni)
		{
			var message = new StringBuilder();
			var valoreStazioneAttuale = valoriStazioniAttuali?.FirstOrDefault(vsa => vsa.NomeStaz == nomeStazione) ?? new IdroAlertHttpResult();
			var valoreStazionePrecedente = valoriStazioniPrecedente?.FirstOrDefault(vsp => vsp.NomeStaz == nomeStazione) ?? new IdroAlertHttpResult();

			// Se è cambiato valore dalla lettura precedente
			if (valoreStazioneAttuale.Value != valoreStazionePrecedente.Value)
			{
				message.AppendFormat("Il livello della stazione {0} ", valoreStazioneAttuale.NomeStaz);
				var differenzaLivello = Math.Round((decimal)(valoreStazioneAttuale.Value > valoreStazionePrecedente.Value
										? valoreStazioneAttuale.Value - valoreStazionePrecedente.Value : valoreStazionePrecedente.Value - valoreStazioneAttuale.Value), 2);

				// Se il valore è aumentato
				if (valoreStazioneAttuale.Value >= valoreStazioneAttuale.Value)
				{
					if (valoreStazioneAttuale.Value >= valoreStazioneAttuale.Soglia1)
					{
						if (valoreStazioneAttuale.Value >= valoreStazioneAttuale.Soglia2)
						{
							if (valoreStazioneAttuale.Value >= valoreStazioneAttuale.Soglia3)
							{
								if (valoreStazionePrecedente.Value < valoreStazionePrecedente.Soglia3)
								{
									message.Append("è passato in soglia ROSSA ed è aumentato di {0} metri", differenzaLivello);
								}
							}
							else
							{
								// Sopra soglia 2
							}
						}
						else
						{
							// Sopra Soglia 1
						}
					}
					else
					{
						// Sotto Soglia1
					}
				}
				else // Se il valore è diminuito
				{

				}
			}
		}
	}

	private ValoriStazione GetValoreStazione(string nomeStazione)
	{
		var valoriStazione = _valoriStazioniAttuali?.FirstOrDefault(vsa => vsa.NomeStaz == nomeStazione);

		if (valoriStazione == null || valoriStazione.Value == null)
		{

		}
	}

	private void Alert() { }
}
