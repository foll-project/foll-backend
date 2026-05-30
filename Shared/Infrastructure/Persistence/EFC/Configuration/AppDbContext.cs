using foll_backend.IAM.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.Entities;
using DeviceEntity = foll_backend.DeviceManagment.Domain.Model.Entities.Device;
using foll_backend.DeviceManagment.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.Shared.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<PatientInvitation> PatientInvitations { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    public DbSet<RelationshipType> RelationshipTypes { get; set; }

    public DbSet<DeviceEntity> Devices { get; set; }
    public DbSet<DeviceEvent> DeviceEvents { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }
    public DbSet<UserPushToken> UserPushTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

