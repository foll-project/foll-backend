namespace foll_backend.Care.Domain.Model.Entities;

public class PatientAnnotation
{
    public long PatientAnnotationId { get; private set; }
    public long PatientId { get; private set; }
    public long AuthorUserId { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected PatientAnnotation()
    {
        Content = string.Empty;
    }

    public PatientAnnotation(long patientId, long authorUserId, string content)
    {
        if (patientId <= 0) throw new ArgumentOutOfRangeException(nameof(patientId));
        if (authorUserId <= 0) throw new ArgumentOutOfRangeException(nameof(authorUserId));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("El contenido de la anotación es obligatorio.", nameof(content));

        PatientId = patientId;
        AuthorUserId = authorUserId;
        Content = content.Trim();
        CreatedAt = DateTime.UtcNow; 
    }
}