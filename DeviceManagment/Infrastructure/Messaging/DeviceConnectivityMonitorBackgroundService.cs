using foll_backend.DeviceManagment.Domain.Model.Commands;
using foll_backend.DeviceManagment.Domain.Repositories;
using foll_backend.DeviceManagment.Domain.Services;
using foll_backend.DeviceManagment.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace foll_backend.DeviceManagment.Infrastructure.Messaging;

public class DeviceConnectivityMonitorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DeviceConnectivityMonitorBackgroundService> _logger;
    private readonly DeviceMonitoringOptions _monitoringOptions;

    public DeviceConnectivityMonitorBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<DeviceMonitoringOptions> monitoringOptions,
        ILogger<DeviceConnectivityMonitorBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _monitoringOptions = monitoringOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
                var commandService = scope.ServiceProvider.GetRequiredService<IDeviceCommandService>();

                var deviceIds = await deviceRepository.ListMonitoredActiveDeviceIdsAsync();
                var detectedAtUtc = DateTime.UtcNow;
                var disconnectThreshold = TimeSpan.FromSeconds(
                    _monitoringOptions.ExpectedHeartbeatIntervalSeconds *
                    _monitoringOptions.MissedHeartbeatsBeforeDisconnect);

                foreach (var deviceId in deviceIds)
                {
                    await commandService.Handle(new CheckDeviceConnectivityCommand(
                        deviceId,
                        detectedAtUtc,
                        disconnectThreshold));
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoreando conectividad de dispositivos.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_monitoringOptions.ConnectivityCheckIntervalSeconds), stoppingToken);
        }
    }
}
