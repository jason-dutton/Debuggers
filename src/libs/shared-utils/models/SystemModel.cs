public class SystemModel
{
    public int systemId { get; set; }

    public string? systemSize { get; set; }

    public int inverterOutput { get; set; }

    public int numberOfPanels { get; set; }

    public int batterySize { get; set; }

    public int numberOfBatteries { get; set; }

    public int solarInput { get; set; }

    public bool? recordState { get; set; }
}
