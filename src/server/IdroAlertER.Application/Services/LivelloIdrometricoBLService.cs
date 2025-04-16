using System.Globalization;
using System.Text;
using System.Xml.Linq;
using IdroAlertER.Common.Entities;
using IdroAlertER.Common.Entities.Results;
using IdroAlertER.Common.Interfaces.Repositories;
using IdroAlertER.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IdroAlertER.Application.Services;
internal class LivelloIdrometricoBLService : ILivelloIdrometricoBLService
{
	private readonly ILogger<LivelloIdrometricoBLService> _logger;
	private readonly ILivelloIdrometricoHttpService _livelloIdrometricoHttpService;
	private readonly ITelegramBotService _telegramHttpService;
	private readonly IGeocalizzazioneService _geocalizzazioneService;
	private readonly ITimeStampService _timeStampService;
	private readonly string _dataConfigurazione;
	private readonly string _oraConfigurazione;
	private (long timeStamp, DateTime dateTime) _timeStampFinale;
	private readonly List<string> _nomiStazioni;
	private readonly string _sogliaMinima;
	private readonly string _xmlNomeFile = "storico.xml";
	private readonly string _xmlRadice = "Stazioni";
	private readonly string _xmlElemento = "Stazione";
	private readonly string _xmlNomeStazione = "NomeStazione";
	private readonly string _xmlValoreAttuale = "ValoreAttuale";

	public LivelloIdrometricoBLService(ILogger<LivelloIdrometricoBLService> logger, IConfiguration configuration, ILivelloIdrometricoHttpService livelloIdrometricoHttpService, ITelegramBotService telegramHttpService,
										IGeocalizzazioneService geocalizzazioneService, ITimeStampService timeStampService)
	{
		_logger = logger;
		_livelloIdrometricoHttpService = livelloIdrometricoHttpService;
		_telegramHttpService = telegramHttpService;
		_geocalizzazioneService = geocalizzazioneService;
		_timeStampService = timeStampService;

		_dataConfigurazione = configuration.GetSection("Test data").Get<string>() ?? string.Empty;
		_oraConfigurazione = configuration.GetSection("Test ora").Get<string>() ?? string.Empty;

		_nomiStazioni = configuration.GetSection("Nomi stazioni").Get<List<string>>() ?? [];
		_sogliaMinima = (configuration.GetSection("Livello minimo alert").Get<string>() ?? Soglia.Arancione).ToLower();

		if (_sogliaMinima != Soglia.Gialla || _sogliaMinima != Soglia.Arancione || _sogliaMinima != Soglia.Rossa)
		{
			_sogliaMinima = Soglia.Arancione;
		}
	}

	public async Task ExecuteAsync()
	{
		var dateTimeAttuale = _timeStampService.Get();

		if (!string.IsNullOrEmpty(_dataConfigurazione) && !string.IsNullOrEmpty(_oraConfigurazione))
		{
			dateTimeAttuale = (_timeStampService.Convert(_dataConfigurazione, _oraConfigurazione), DateTime.ParseExact($"{_dataConfigurazione} {_oraConfigurazione}", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
		}

		foreach (var nomeStazione in _nomiStazioni)
		{
			try
			{
				var valoriStazione = await GetValoriStazioneAsync(dateTimeAttuale, nomeStazione);
				var valoreAttuale = GetStoricoValore(nomeStazione, valoriStazione.ValoreAttuale);

				if (valoriStazione.ValoreAttuale != valoriStazione.ValorePrecedente
					&& (!valoreAttuale.HasValue || (valoreAttuale.HasValue && valoriStazione.ValoreAttuale != valoreAttuale)))
				{
					_logger.LogDebug($"Valore differente per la stazione {nomeStazione}");

					var messaggio = GeneraMessaggio(valoriStazione);

					if (!string.IsNullOrEmpty(messaggio))
					{
						var sogliaMinima = _sogliaMinima;

						switch (_sogliaMinima)
						{
							case Soglia.Gialla:
								await InviaNotificheDaSogliaGiallaAsync(valoriStazione, messaggio);
								break;

							case Soglia.Arancione:
								await InviaNotificheDaSogliaArancioneAsync(valoriStazione, messaggio);
								break;

							case Soglia.Rossa:
								await InviaNotificheDaSogliaRossaAsync(valoriStazione, messaggio);
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error:");
			}
		}
	}

	private async Task<ValoriStazione> GetValoriStazioneAsync((long, DateTime) timeStamp, string nomeStazione)
	{
		var valoriStazioneAttuale = await GetValoreStazioneAsync(timeStamp, nomeStazione);
		var valoriStazionePrecedente = await GetValoreStazioneAsync(_timeStampService.GetBefore(_timeStampFinale), nomeStazione);

		return new ValoriStazione
		{
			IdStazione = valoriStazioneAttuale.IdStazione,
			Lat = valoriStazioneAttuale.Lat,
			Lon = valoriStazioneAttuale.Lon,
			NomeStaz = valoriStazioneAttuale.NomeStaz,
			Ordinamento = valoriStazioneAttuale.Ordinamento,
			SogliaArancione = valoriStazioneAttuale.Soglia2,
			SogliaGialla = valoriStazioneAttuale.Soglia1,
			SogliaRossa = valoriStazioneAttuale.Soglia3,
			ValoreAttuale = valoriStazioneAttuale.Value!.Value,
			ValorePrecedente = valoriStazionePrecedente?.Value ?? 0
		};
	}

	private async Task<IdroAlertHttpResult> GetValoreStazioneAsync((long, DateTime) timeStamp, string nomeStazione)
	{
		var valoriStazioni = await GetValoriStazioniAsync(timeStamp, nomeStazione);
		return valoriStazioni.First(vsa => vsa.NomeStaz == nomeStazione);
	}

	private async Task<List<IdroAlertHttpResult>> GetValoriStazioniAsync((long timeStamp, DateTime dateTime) timeStamp, string nomeStazione)
	{
		_timeStampFinale = timeStamp;
		var valoriStazioni = await _livelloIdrometricoHttpService.GetAsync(_timeStampFinale.timeStamp);

		if (valoriStazioni != null && !valoriStazioni.Any(x => x.NomeStaz == nomeStazione))
		{
			throw new Exception($"Stazione {nomeStazione} non trovata");
		}
		else
		{
			if (valoriStazioni == null || (valoriStazioni != null && !valoriStazioni.First(vs => vs.NomeStaz == nomeStazione).Value.HasValue))
			{
				if (timeStamp.timeStamp < _timeStampService.GetBeforeOneHour())
				{
					throw new Exception($"Valori stazione {nomeStazione} non trovati");
				}
				else
				{
					return await GetValoriStazioniAsync(_timeStampService.GetBefore(_timeStampFinale), nomeStazione);
				}
			}
			else
			{
				return valoriStazioni!;
			}
		}
	}

	private double? GetStoricoValore(string nomeStazione, double valore)
	{
		double? valoreAttuale = null;

		if (File.Exists(_xmlNomeFile))
		{
			var xmlStorico = XDocument.Load(_xmlNomeFile);
			var valoreTrovato = false;

			if (xmlStorico != null)
			{
				var storico = xmlStorico.Descendants(_xmlElemento);

				foreach (var stazione in storico)
				{
					var storicoNomeStazione = stazione.Element(_xmlNomeStazione)?.Value;
					var storicoValoreAttuale = stazione.Element(_xmlValoreAttuale)?.Value;

					if (!String.IsNullOrEmpty(storicoNomeStazione) && storicoNomeStazione == nomeStazione)
					{
						valoreTrovato = true;

						if (!String.IsNullOrEmpty(storicoValoreAttuale))
						{
							valoreAttuale = Convert.ToDouble(storicoValoreAttuale.Replace('.', ','));
						}

						stazione.Element(_xmlValoreAttuale)!.Value = valore.ToString();
					}
				}

				if (!valoreTrovato)
				{
					xmlStorico.Element(_xmlRadice)!.Add(new XElement(_xmlElemento, new XElement(_xmlNomeStazione, nomeStazione), new XElement(_xmlValoreAttuale, valore)));
				}

			}

			xmlStorico!.Save(_xmlNomeFile);
		}
		else
		{
			var xmlStorico = new XDocument(new XElement(_xmlRadice, new XElement(_xmlElemento,
											new XElement(_xmlNomeStazione, nomeStazione), new XElement(_xmlValoreAttuale, valore))));

			xmlStorico.Save(_xmlNomeFile);
		}

		return valoreAttuale;
	}

	private string GeneraMessaggio(ValoriStazione valoriStazione)
	{
		var differenzaLivello = Math.Round((decimal)(valoriStazione.ValoreAttuale > valoriStazione.ValorePrecedente
										? valoriStazione.ValoreAttuale - valoriStazione.ValorePrecedente : valoriStazione.ValoreAttuale - valoriStazione.ValorePrecedente), 2);
		var messaggio = new StringBuilder($"Il livello della stazione {valoriStazione.NomeStaz} è ");

		if ((valoriStazione.ValoreAttuale >= valoriStazione.SogliaRossa && valoriStazione.ValorePrecedente < valoriStazione.SogliaRossa)
			|| (valoriStazione.ValoreAttuale >= valoriStazione.SogliaArancione && valoriStazione.ValorePrecedente < valoriStazione.SogliaArancione)
			|| (valoriStazione.ValoreAttuale >= valoriStazione.SogliaGialla && valoriStazione.ValorePrecedente < valoriStazione.SogliaGialla))
		{
			messaggio.Append("PASSATO");
		}
		else
		{
			if ((valoriStazione.ValorePrecedente >= valoriStazione.SogliaRossa && valoriStazione.ValoreAttuale < valoriStazione.SogliaRossa)
			|| (valoriStazione.ValorePrecedente >= valoriStazione.SogliaArancione && valoriStazione.ValoreAttuale < valoriStazione.SogliaArancione)
			|| (valoriStazione.ValorePrecedente >= valoriStazione.SogliaGialla && valoriStazione.ValoreAttuale < valoriStazione.SogliaGialla))
			{
				//messaggio.Append("tornato");
				return string.Empty;
			}
			else
			{
				//messaggio.Append("ancora");
				return string.Empty;
			}
		}

		messaggio.Append(" in soglia ");

		if (valoriStazione.SogliaRossa > 0 && valoriStazione.ValoreAttuale >= valoriStazione.SogliaRossa)
		{
			messaggio.Append("ROSSA");
		}
		else
		{
			if (valoriStazione.SogliaArancione > 0 && valoriStazione.ValoreAttuale >= valoriStazione.SogliaArancione)
			{
				messaggio.Append("ARANCIONE");
			}
			else
			{
				if (valoriStazione.SogliaGialla > 0 && valoriStazione.ValoreAttuale >= valoriStazione.SogliaGialla)
				{
					messaggio.Append("GIALLA");
				}
			}
		}

		messaggio.Append(" ed è ");
		messaggio.Append($"{(valoriStazione.ValoreAttuale >= valoriStazione.ValorePrecedente ? "aumentato" : "diminuito")}");
		messaggio.Append($" di {differenzaLivello} metri");
		messaggio.AppendLine("");
		messaggio.AppendLine("");
		messaggio.AppendLine($"https://allertameteo.regione.emilia-romagna.it/web/guest/grafico-sensori?p_p_id=AllertaGraficoPortlet&p_p_lifecycle=0&_AllertaGraficoPortlet_mvcRenderCommandName=%2Fallerta%2Fanimazione%2Fgrafico&r={valoriStazione.IdStazione}/254,0,0/1,-,-,-/B13215/{DateTime.UtcNow.ToLocalTime().AddDays(-2).ToString("yyyy-MM-dd")}/{DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd")}&stazione={valoriStazione.IdStazione}&variabile=254,0,0/1,-,-,-/B13215");
		messaggio.AppendLine("");
		messaggio.AppendLine($"Posizione:");
		messaggio.AppendLine($"https://www.google.com/maps/place/{_geocalizzazioneService.ConvertiLatitudine(valoriStazione)}+{_geocalizzazioneService.ConvertiLongitudine(valoriStazione)}");

		return messaggio.ToString();
	}

	private async Task InviaNotificheDaSogliaGiallaAsync(ValoriStazione valoriStazione, string messaggio)
	{
		if (InviaNotificaSecondoSoglia(valoriStazione, Soglia.Gialla))
		{
			await InviaNotificaAsync(messaggio);
		}

		await InviaNotificheDaSogliaArancioneAsync(valoriStazione, messaggio);
	}

	private async Task InviaNotificheDaSogliaArancioneAsync(ValoriStazione valoriStazione, string messaggio)
	{
		if (InviaNotificaSecondoSoglia(valoriStazione, Soglia.Arancione))
		{
			await InviaNotificaAsync(messaggio);
		}

		await InviaNotificheDaSogliaRossaAsync(valoriStazione, messaggio);
	}

	private async Task InviaNotificheDaSogliaRossaAsync(ValoriStazione valoriStazione, string messaggio)
	{
		if (InviaNotificaSecondoSoglia(valoriStazione, Soglia.Rossa))
		{
			await InviaNotificaAsync(messaggio);
		}
	}

	private static bool InviaNotificaSecondoSoglia(ValoriStazione valoriStazione, string soglia)
	{
		double? valoreSoglia = soglia switch
		{
			Soglia.Arancione => valoriStazione.SogliaArancione.HasValue && valoriStazione.SogliaArancione > 0 ? valoriStazione.SogliaArancione : valoriStazione.SogliaGialla,
			Soglia.Rossa => valoriStazione.SogliaRossa.HasValue && valoriStazione.SogliaRossa > 0 ? valoriStazione.SogliaRossa
								: valoriStazione.SogliaArancione.HasValue && valoriStazione.SogliaArancione > 0 ? valoriStazione.SogliaArancione : valoriStazione.SogliaGialla,
			_ => valoriStazione.SogliaGialla
		};

		return valoreSoglia > 0
				&& ((valoriStazione.ValoreAttuale >= valoreSoglia && valoriStazione.ValorePrecedente < valoreSoglia)
					|| (valoriStazione.ValoreAttuale < valoreSoglia && valoriStazione.ValorePrecedente >= valoreSoglia));
	}

	private async Task InviaNotificaAsync(string messaggio)
	{
		_logger.LogDebug($"Invio messaggio: {messaggio}");
		await _telegramHttpService.SendAsync(messaggio);
	}
}
