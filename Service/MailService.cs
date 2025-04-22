using System.Net.Mail;
using System.Net;

namespace HTTL_May_Xay_Dung.Service
{
    public class MailService
    {
        public void SendEmailAsync(string email, string subject, string message)
        {
            var mail = "lebach09122002@gmail.com";
            var pas = "rdbaqfqorhetptru";

            MailMessage mailMessage = new MailMessage();
            mailMessage.Subject = subject;
            mailMessage.From = new MailAddress(mail);
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(mail, pas),
                EnableSsl = true
            };
            smtpClient.Send(mailMessage);
        }
    }
}
