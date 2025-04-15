namespace IdroAlertER.Common.Interfaces.Services;
public interface ITimeStampService
{
	(long timeStamp, DateTime dateTime) Get();
	(long timeStamp, DateTime dateTime) GetBefore((long timeStamp, DateTime dateTime) timeStamp);
	long GetBeforeOneHour();
	long Convert(string date, string time);
}
