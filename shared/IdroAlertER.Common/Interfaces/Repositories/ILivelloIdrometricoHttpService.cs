using IdroAlertER.Common.Entities.Results;

namespace IdroAlertER.Common.Interfaces.Repositories;
public interface ILivelloIdrometricoHttpService
{
	Task<List<IdroAlertHttpResult>?> GetAsync(long timestamp);
}
