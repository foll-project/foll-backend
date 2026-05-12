namespace foll_backend.DeviceManagment.Infrastructure.Configuration;

public class OutboxOptions
{
    public int PollIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
}
