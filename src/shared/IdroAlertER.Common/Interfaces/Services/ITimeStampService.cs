namespace IdroAlertER.Common.Interfaces.Services;
public interface ITimeStampService
{
	long Get();
	long GetBefore(long timeStamp);
	long GetBeforeOneHour();
	long Convert(string date, string time);
}
