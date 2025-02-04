using System.Text;
using IdroAlertER.Application.Entities;
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
	private readonly string _sogliaMinima;

	public LivelloIdrometricoBLService(IConfiguration configuration, ITimeStampService timeStampService, ILivelloIdrometricoHttpService livelloIdrometricoHttpService)
	{
		_timeStampService = timeStampService;
		_livelloIdrometricoHttpService = livelloIdrometricoHttpService;

		var dataConfigurazione = configuration.GetSection("Test data").Get<string>();
		var oraConfigurazione = configuration.GetSection("Test ora").Get<string>();

		if (!string.IsNullOrEmpty(dataConfigurazione) && !string.IsNullOrEmpty(oraConfigurazione))
		{
			_timeStampAttuale = timeStampService.Convert(dataConfigurazione, oraConfigurazione);
		}
		else
		{
			_timeStampAttuale = timeStampService.Get();
		}

		_nomiStazioni = configuration.GetSection("Nomi stazioni").Get<List<string>>() ?? [];
		_sogliaMinima = (configuration.GetSection("Livello minimo alert").Get<string>() ?? "arancione").ToLower();

		if (_sogliaMinima != "gialla" || _sogliaMinima != "arancione" || _sogliaMinima != "rossa")
		{
			_sogliaMinima = "arancione";
		}
	}

	public async Task ExecuteAsync()
	{
		foreach (var nomeStazione in _nomiStazioni)
		{
			var valoriStazione = await GetValoriStazioneAsync(_timeStampAttuale, nomeStazione);
			var message = new StringBuilder();

			// Se è cambiato valore dalla lettura precedente
			if (valoriStazione.ValoreAttuale != valoriStazione.ValorePrecedente)
			{
				message.AppendFormat("Il livello della stazione {0} ", valoriStazione.NomeStaz);
				var differenzaLivello = Math.Round((decimal)(valoriStazione.ValoreAttuale > valoriStazione.ValorePrecedente
										? valoriStazione.ValoreAttuale - valoriStazione.ValorePrecedente : valoriStazione.ValoreAttuale - valoriStazione.ValorePrecedente), 2);

				// Se il valore è aumentato
				if (valoriStazione.ValoreAttuale >= valoriStazione.ValorePrecedente)
				{
					if (valoriStazione.ValoreAttuale >= valoriStazione.SogliaRossa)
					{
						if (valoriStazione.ValorePrecedente < valoriStazione.SogliaRossa)
						{
							message.AppendFormat("è PASSATO in soglia ROSSA ed è aumentato di {0} metri", differenzaLivello);
						}
						else
						{
							message.AppendFormat("è ancora in soglia ROSSA ed è aumentato di {0} metri", differenzaLivello);
						}
					}
					else
					{
						if (_sogliaMinima == "arancione" || _sogliaMinima == "gialla")
						{
							if (valoriStazione.ValoreAttuale >= valoriStazione.SogliaArancione)
							{
								if (valoriStazione.ValorePrecedente < valoriStazione.SogliaArancione)
								{
									message.AppendFormat("è PASSATO in soglia ARANCIONE ed è aumentato di {0} metri", differenzaLivello);
								}
								else
								{
									message.AppendFormat("è ancora in soglia ARANCIONE ed è aumentato di {0} metri", differenzaLivello);
								}
							}
							else
							{
								if (_sogliaMinima == "gialla")
								{
									if (valoriStazione.ValoreAttuale >= valoriStazione.SogliaGialla)
									{
										if (valoriStazione.ValorePrecedente < valoriStazione.SogliaGialla)
										{
											message.AppendFormat("è PASSATO in soglia GIALLA ed è aumentato di {0} metri", differenzaLivello);
										}
										else
										{
											message.AppendFormat("è ancora in soglia GIALLA ed è aumentato di {0} metri", differenzaLivello);
										}
									}
								}
							}
						}
					}
				}
				else // Se il valore è diminuito
				{

					if (valoriStazione.ValoreAttuale >= valoriStazione.SogliaRossa)
					{
						message.AppendFormat("è ancora in soglia ROSSA ed è diminuito di {0} metri", differenzaLivello);
					}
					else
					{
						if (valoriStazione.ValoreAttuale >= valoriStazione.SogliaArancione)
						{
							if (valoriStazione.ValorePrecedente >= valoriStazione.SogliaRossa)
							{
								message.AppendFormat("è TORNATO in soglia ARANCIONE ed è diminuito di {0} metri", differenzaLivello);
							}
							else
							{
								if (_sogliaMinima == "arancione" || _sogliaMinima == "gialla")
								{
									message.AppendFormat("è ancora in soglia ARANCIONE ed è diminuito di {0} metri", differenzaLivello);
								}
							}
						}
						else
						{
							if (valoriStazione.ValoreAttuale >= valoriStazione.SogliaGialla)
							{
								if (valoriStazione.ValorePrecedente >= valoriStazione.SogliaArancione)
								{
									if (_sogliaMinima == "arancione" || _sogliaMinima == "gialla")
									{
										message.AppendFormat("è TORNATO in soglia GIALLA ed è diminuito di {0} metri", differenzaLivello);
									}
								}
								else
								{
									if (_sogliaMinima == "gialla")
									{
										message.AppendFormat("è ancora in soglia GIALLA ed è diminuito di {0} metri", differenzaLivello);
									}
								}
							}
						}
					}
				}
			}
		}
	}

	private async Task<ValoriStazione> GetValoriStazioneAsync(long timeStamp, string nomeStazione)
	{
		var valoriStazioneAttuale = await _livelloIdrometricoHttpService.GetAsync(timeStamp);

		if (valoriStazioneAttuale == null)
		{
			return await GetValoriStazioneAsync(_timeStampService.GetBefore(timeStamp), nomeStazione);
		}
		else
		{
			var valoriStazione = valoriStazioneAttuale.FirstOrDefault(vsa => vsa.NomeStaz == nomeStazione);

			if (valoriStazione == null || valoriStazione.Value == null)
			{
				return await GetValoriStazioneAsync(_timeStampService.GetBefore(timeStamp), nomeStazione);
			}
			else
			{
				var valoriStazioniPrecedenti = await _livelloIdrometricoHttpService.GetAsync(_timeStampService.GetBefore(timeStamp));
				var valoriStazionePrecedente = valoriStazioniPrecedenti?.FirstOrDefault(vsa => vsa.NomeStaz == nomeStazione);

				return new ValoriStazione
				{
					IdStazione = valoriStazione.IdStazione,
					Lat = valoriStazione.Lat,
					Lon = valoriStazione.Lon,
					NomeStaz = valoriStazione.NomeStaz,
					Ordinamento = valoriStazione.Ordinamento,
					SogliaArancione = valoriStazione.Soglia2,
					SogliaGialla = valoriStazione.Soglia1,
					SogliaRossa = valoriStazione.Soglia3,
					ValoreAttuale = valoriStazione.Value.Value,
					ValorePrecedente = valoriStazionePrecedente?.Value ?? 0
				};
			}
		}
	}

	private void Alert() { }
}
