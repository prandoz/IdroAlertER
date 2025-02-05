namespace IdroAlertER.Common.Entities;
public class ValoriStazione
{
	public string? IdStazione { get; set; }
	public int? Ordinamento { get; set; }
	public string? NomeStaz { get; set; }
	public string? Lon { get; set; }
	public string? Lat { get; set; }
	public double ValoreAttuale { get; set; }
	public double ValorePrecedente { get; set; }
	public double? SogliaGialla { get; set; }
	public double? SogliaArancione { get; set; }
	public double? SogliaRossa { get; set; }
	public long? TimeStamp { get; set; }
}
