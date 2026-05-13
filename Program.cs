using foll_backend.Care.Application.ACL;
using System.Text;
using foll_backend.Care.Application.Internal.CommandServices;
using foll_backend.Care.Application.Internal.QueryServices;
using foll_backend.Care.Application.OutboundServices;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Care.Domain.Services;
using foll_backend.Care.Infrastructure.Persistence.EFC.Repositories;
using foll_backend.DeviceManagment.Application.ACL;
using foll_backend.DeviceManagment.Application.Internal.CommandServices;
using foll_backend.DeviceManagment.Application.Internal.QueryServices;
using foll_backend.DeviceManagment.Application.OutboundServices;
using foll_backend.DeviceManagment.Domain.Repositories;
using foll_backend.DeviceManagment.Domain.Services;
using foll_backend.DeviceManagment.Infrastructure.Configuration;
using foll_backend.DeviceManagment.Infrastructure.Messaging;
using foll_backend.DeviceManagment.Infrastructure.Persistence.EFC.Repositories;
using foll_backend.EmergencyAnalytics.Application.ACL;
using foll_backend.EmergencyAnalytics.Application.Internal.CommandServices;
using foll_backend.EmergencyAnalytics.Application.Internal.QueryServices;
using foll_backend.EmergencyAnalytics.Application.OutboundServices;
using foll_backend.EmergencyAnalytics.Domain.Repositories;
using foll_backend.EmergencyAnalytics.Domain.Services;
using foll_backend.EmergencyAnalytics.Infrastructure.Configuration;
using foll_backend.EmergencyAnalytics.Infrastructure.Messaging;
using foll_backend.EmergencyAnalytics.Infrastructure.Persistence.EFC.Repositories;
using foll_backend.IAM.Application.Internal.CommandServices;
using foll_backend.IAM.Application.Internal.QueryServices;
using foll_backend.IAM.Application.OutboundServices;
using foll_backend.IAM.Application.ACL;
using foll_backend.IAM.Domain.Repositories;
using foll_backend.IAM.Domain.Services;
using foll_backend.IAM.Infrastructure.Hashing;
using foll_backend.IAM.Infrastructure.Persistence.EFC.Repositories;
using foll_backend.IAM.Infrastructure.Tokens;
using foll_backend.Shared.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using foll_backend.Shared.Infrastructure.Swagger;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection("Mqtt"));
builder.Services.Configure<DeviceMonitoringOptions>(builder.Configuration.GetSection("DeviceMonitoring"));
builder.Services.Configure<EmergencyAnalyticsMqttOptions>(builder.Configuration.GetSection("EmergencyAnalyticsMqtt"));
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection("Outbox"));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Foll API",
        Version = "v1",
        Description = "API con arquitectura DDD y Bounded Contexts"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue SOLO el JWT, sin escribir Bearer."
    });

    options.OperationFilter<AuthorizeOperationFilter>();
});

var jwtSecret = builder.Configuration["JwtSettings:Secret"];

if (!string.IsNullOrEmpty(jwtSecret))
{
    var key = Encoding.ASCII.GetBytes(jwtSecret);

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });
}

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IHashingService, BcryptHashingService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IUserInfoAcl, UserInfoAcl>();
builder.Services.AddScoped<IPatientDeviceAcl, PatientDeviceAcl>();
builder.Services.AddScoped<IPatientEmergencyAcl, PatientEmergencyAcl>();

builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientInvitationRepository, PatientInvitationRepository>();
builder.Services.AddScoped<IRelationshipTypeRepository, RelationshipTypeRepository>();

builder.Services.AddScoped<IPatientCommandService, PatientCommandService>();
builder.Services.AddScoped<IPatientQueryService, PatientQueryService>();
builder.Services.AddScoped<IRelationshipTypeQueryService, RelationshipTypeQueryService>();
builder.Services.AddScoped<IUserInfoService, UserInfoService>();

builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceEventRepository, DeviceEventRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

builder.Services.AddScoped<IPatientAccessService, PatientAccessService>();
builder.Services.AddScoped<IDeviceAssignmentAcl, DeviceAssignmentAcl>();
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();
builder.Services.AddScoped<IDeviceQueryService, DeviceQueryService>();

builder.Services.AddScoped<IEmergencyIncidentRepository, EmergencyIncidentRepository>();
builder.Services.AddScoped<IFallTypeRepository, FallTypeRepository>();
builder.Services.AddScoped<IEmergencyOutboxMessageRepository, EmergencyOutboxMessageRepository>();
builder.Services.AddScoped<IDeviceIncidentAssignmentService, DeviceIncidentAssignmentService>();
builder.Services.AddScoped<IPatientIncidentAccessService, PatientIncidentAccessService>();
builder.Services.AddScoped<IEmergencyIncidentCommandService, EmergencyIncidentCommandService>();
builder.Services.AddScoped<IEmergencyIncidentQueryService, EmergencyIncidentQueryService>();

builder.Services.AddHostedService<MqttHeartbeatSubscriberBackgroundService>();
builder.Services.AddHostedService<DeviceConnectivityMonitorBackgroundService>();
builder.Services.AddHostedService<EmergencyAnalyticsMqttSubscriberBackgroundService>();
builder.Services.AddHostedService<OutboxPublisherBackgroundService>();
builder.Services.AddHostedService<EmergencyOutboxPublisherBackgroundService>();

var app = builder.Build();

var applyMigrations = builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup", true);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    if (applyMigrations)
    {
        context.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else if (builder.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }