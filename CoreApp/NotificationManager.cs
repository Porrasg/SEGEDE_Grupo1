using System.Net;
using System.Net.Mail;
using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.CoreApp;

// Manager de Notificaciones (§14.11). Instanciación directa con new sin IoC.
// Gestiona la cola asíncrona de notificaciones (ACID, RNF-009, RF-070), envíos SMTP con reintentos y backoff exponencial, y consulta paginada por usuario.
public class NotificationManager
{
    private readonly NotificationQueueCrudFactory _queueFactory = new();

    // RF-070: Encola una notificación para envío posterior por el job en segundo plano. NextAttempt=NowCR().
    public void Enqueue(int userId, string email, string type, string subject, string body, bool isCritical)
    {
        var notif = new NotificationQueue
        {
            UserId = userId,
            RecipientEmail = email,
            NotificationType = type,
            Subject = subject,
            Body = body,
            IsCritical = isCritical,
            Status = NotificationStates.Pending,
            Attempts = 0,
            NextAttempt = TimeHelper.NowCR(),
            SentDate = null,
            Created = TimeHelper.NowCR()
        };

        _queueFactory.Create(notif);
    }

    // RF-070 / RNF-009: Procesa la cola de notificaciones pendientes (ejecutado en job de 60s).
    // Intenta el envío por SMTP; si falla, aplica backoff hasta un máximo de 3 reintentos antes de marcar Failed.
    public void ProcessQueue()
    {
        var pending = _queueFactory.RetrievePending();
        var now = TimeHelper.NowCR();

        string host = Environment.GetEnvironmentVariable("Smtp:Host") ?? "";
        int port = int.TryParse(Environment.GetEnvironmentVariable("Smtp:Port"), out int p) ? p : 587;
        string user = Environment.GetEnvironmentVariable("Smtp:User") ?? "";
        string pass = Environment.GetEnvironmentVariable("Smtp:Password") ?? "";
        string from = Environment.GetEnvironmentVariable("Smtp:FromAddress") ?? "no-reply@sgde.cr";
        bool enableSsl = !string.Equals(Environment.GetEnvironmentVariable("Smtp:EnableSsl"), "false", StringComparison.OrdinalIgnoreCase);

        foreach (var notif in pending)
        {
            if (notif.NextAttempt.HasValue && notif.NextAttempt.Value > now)
            {
                continue;
            }

            bool sentSuccess = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(host))
                {
                    using var client = new SmtpClient(host, port)
                    {
                        Credentials = new NetworkCredential(user, pass),
                        EnableSsl = enableSsl
                    };
                    using var mail = new MailMessage(from, notif.RecipientEmail, notif.Subject, notif.Body);
                    client.Send(mail);
                    sentSuccess = true;
                }
                else
                {
                    // Host vacío: solo se simula éxito en desarrollo. En producción es una mala
                    // configuración y NO debe marcarse como enviada (se reintenta / marca Failed).
                    bool isDev = string.Equals(
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                        "Development", StringComparison.OrdinalIgnoreCase);
                    sentSuccess = isDev;
                }
            }
            catch
            {
                sentSuccess = false;
            }

            notif.Updated = now;

            if (sentSuccess)
            {
                notif.Status = NotificationStates.Sent;
                notif.SentDate = now;
                notif.NextAttempt = null;
            }
            else
            {
                notif.Attempts += 1;
                if (notif.Attempts >= 3)
                {
                    notif.Status = NotificationStates.Failed;
                    notif.NextAttempt = null;
                }
                else
                {
                    // Backoff exponencial: 5 min, 15 min...
                    notif.NextAttempt = now.AddMinutes(Math.Pow(3, notif.Attempts) * 5);
                }
            }

            _queueFactory.Update(notif);
        }
    }

    // RF-070: Retorna las notificaciones del usuario.
    public List<NotificationQueue> RetrieveByUser(int userId)
    {
        return _queueFactory.RetrieveByUser(userId, 1, 1000);
    }
}
