using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using EmFren.Models;

namespace EmFren.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendPasswordResetEmail(
            string recipientEmail,
            string recipientName,
            string resetLink)
        {
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _settings.FromName,
                    _settings.From));

            message.To.Add(
                new MailboxAddress(
                    recipientName,
                    recipientEmail));

            message.Subject = "Recuperar contraseña - EmFren";

            message.Body = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>

                        <h2>EmFren</h2>

                        <p>Hola {recipientName},</p>

                        <p>
                            Recibimos una solicitud para restablecer
                            la contraseña de tu cuenta.
                        </p>

                        <p>
                            Haz clic en el siguiente botón para
                            establecer una nueva contraseña:
                        </p>

                        <p>
                            <a href='{resetLink}'
                               style='
                               display:inline-block;
                               padding:12px 20px;
                               background:#2563eb;
                               color:white;
                               text-decoration:none;
                               border-radius:6px;'>
                                Restablecer contraseña
                            </a>
                        </p>

                        <p>
                            Este enlace será válido durante 60 minutos.
                        </p>

                        <p>
                            Si tú no solicitaste este cambio,
                            puedes ignorar este correo.
                        </p>

                        <p>
                            Saludos,<br>
                            Equipo EmFren
                        </p>

                    </body>
                    </html>"
            }.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _settings.Username,
                _settings.Password);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);
        }
    }
}