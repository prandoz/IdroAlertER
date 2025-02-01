using IdroAlertER.Common.Interfaces.Repositories;
using IdroAlertER.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace IdroAlertER.Application.Services;
internal class LivelloIdrometricoBLService : ILivelloIdrometricoBLService
{
	private readonly IConfiguration _configuration;
	private readonly ITimeStampService _timeStampService;
	private readonly ILivelloIdrometricoHttpService _livelloIdrometricoHttpService;

	public LivelloIdrometricoBLService(IConfiguration configuration, ITimeStampService timeStampService, ILivelloIdrometricoHttpService livelloIdrometricoHttpService)
	{
		_configuration = configuration;
		_timeStampService = timeStampService;
		_livelloIdrometricoHttpService = livelloIdrometricoHttpService;
	}

	public async Task ExecuteAsync()
	{
		var timeStampAttuale = _timeStampService.Get();
		var valoriStazioniAttuali = await _livelloIdrometricoHttpService.GetAsync(timeStampAttuale);
		// TODO: rimuovere
		valoriStazioniAttuali = await _livelloIdrometricoHttpService.GetAsync(1738437300000);
		var valoriStazioniPrecedente = await _livelloIdrometricoHttpService.GetAsync(_timeStampService.GetBefore(timeStampAttuale));

		foreach (var nomeStazione in _configuration.GetSection("NomiStazioni").Get<List<string>>())
		{
			var valoreStazioneAttuale = valoriStazioniAttuali.FirstOrDefault(vsa => vsa.NomeStaz == nomeStazione);
			var valoreStazionePrecedente = valoriStazioniPrecedente.FirstOrDefault(vsp => vsp.NomeStaz == nomeStazione);

			// Se è cambiato valore dalla lettura precedente
			if (valoreStazioneAttuale.Value != valoreStazionePrecedente.Value)
			{
				// Se il valore è aumentato
				if (valoreStazioneAttuale.Value >= valoreStazioneAttuale.Value)
				{
					if (valoreStazioneAttuale.Value >= valoreStazioneAttuale.Soglia1)
					{
						if (valoreStazionePrecedente.Value < valoreStazionePrecedente.Soglia1)
						{
							Alert();
						}
						else
						{
							if (valoreStazioneAttuale.Value >= valoreStazioneAttuale.Soglia2)
							{
								if (valoreStazioneAttuale.Value >= valoreStazioneAttuale.Soglia3)
								{
									// Se è cambiata la soglia
									if (valoreStazionePrecedente.Value < valoreStazionePrecedente.Soglia3)
									{
										Alert();
									}
								}
								else
								{
									if (valoreStazionePrecedente.Value < valoreStazionePrecedente.Soglia2)
									{
										Alert();
									}
								}
							}
							else
							{
								if (valoreStazionePrecedente.Value < valoreStazionePrecedente.Soglia1)
								{
									Alert();
								}
							}
						}
					}
				}
				else // Se il valore è diminuito
				{
					if (valoreStazioneAttuale.Value < valoreStazioneAttuale.Soglia3)
					{
						if (valoreStazioneAttuale.Value < valoreStazioneAttuale.Soglia2)
						{
							if (valoreStazioneAttuale.Value < valoreStazioneAttuale.Soglia1)
							{
								// Se è cambiata la soglia
								if (valoreStazionePrecedente.Value >= valoreStazionePrecedente.Soglia1)
								{
									Alert();
								}
							}
							else
							{
								if (valoreStazionePrecedente.Value >= valoreStazionePrecedente.Soglia2)
								{
									Alert();
								}
							}
						}
						else
						{
							if (valoreStazionePrecedente.Value >= valoreStazionePrecedente.Soglia3)
							{
								Alert();
							}
						}
					}
				}
			}
		}
	}

	private void Alert() { }
}
