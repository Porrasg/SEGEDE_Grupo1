namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Cola de notificaciones → tblNotificationQueue (§9.22).
/// </summary>
public class NotificationQueue : BaseDTO
{
    public int UserId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsCritical { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public DateTime? NextAttempt { get; set; }
    public DateTime? SentDate { get; set; }
}
