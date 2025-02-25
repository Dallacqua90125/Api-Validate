using System.Net.Mail;
using System.Net;

namespace UserRegistrationAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string userEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            // Configuração do cliente SMTP
            var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
            {
                Port = int.Parse(emailSettings["SmtpPort"]),
                Credentials = new NetworkCredential(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]),
                EnableSsl = true,
            };

            // Criando o e-mail a ser enviado
            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["FromEmail"]), // E-mail de envio
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            // Definindo o e-mail do usuário como destinatário
            mailMessage.To.Add(userEmail);

            // Enviar o e-mail de forma assíncrona
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
