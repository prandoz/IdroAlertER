using IdroAlertER.Common.Entities.Results;
using IdroAlertER.Common.Interfaces.Repositories;
using Newtonsoft.Json;

namespace IdroAlertER.Infrastructure.Repositories;
internal class LivelloIdrometricoHttpService : ILivelloIdrometricoHttpService
{
	public async Task<List<IdroAlertHttpResult>?> GetAsync(long timestamp)
	{
		using HttpClient client = new();
		try
		{
			string urlLivelloIdrometricoEmiliaRomagna = "https://allertameteo.regione.emilia-romagna.it/o/api/allerta/get-sensor-values?variabile=254,0,0/1,-,-,-/B13215&time=";
			var response = await client.GetStringAsync(string.Concat(urlLivelloIdrometricoEmiliaRomagna, timestamp));

			return JsonConvert.DeserializeObject<List<IdroAlertHttpResult>>(response);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Errore durante la chiamata HTTP: " + ex.Message);
			return null;
		}
	}
}
