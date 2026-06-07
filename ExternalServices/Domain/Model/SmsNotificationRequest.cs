namespace foll_backend.ExternalServices.Domain.Model;

public record SmsNotificationRequest(string PhoneNumber, string Message);
