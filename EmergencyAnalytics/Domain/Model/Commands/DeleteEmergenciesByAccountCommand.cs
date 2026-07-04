namespace foll_backend.EmergencyAnalytics.Domain.Model.Commands;

public record DeleteEmergenciesByAccountCommand(IEnumerable<long> PatientIds);
