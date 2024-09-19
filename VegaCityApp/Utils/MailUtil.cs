using System.Net;
using System.Net.Mail;

namespace VegaCityApp.API.Utils
{
    public class MailUtil
    {
        private const string _smtpServer = "smtp.gmail.com";
        private const int _port = 587;
        private const string _fromEmail = "khangnhse161460@fpt.edu.vn";
        private const string _password = "ljei leab sygd radq";
        #region email sercure
        //private const string _fromEmail = "hoangthse161468@fpt.edu.vn";
        //private const string _password = "qbsg kyos lceg pxzu";
        #endregion
        public static async Task SendMailAsync(string to, string subject, string body)
        {
            //Send mail
            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(to);
            using (var smtpClient = new SmtpClient(_smtpServer, _port))
            {
                smtpClient.Credentials = new NetworkCredential(_fromEmail, _password);
                smtpClient.EnableSsl = true;

                try
                {
                    await smtpClient.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error sending email: " + ex.Message);
                }
            }
        }
    }
}
