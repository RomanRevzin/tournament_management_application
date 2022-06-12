using TournamentApp.Data.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System.Text;
using System.Diagnostics;
using MailKit.Security;

namespace TournamentApp.Data.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
        Task<FormFile> CreateIcsAsync(string teamName, string foeName, DateTime dateTime, string tournamentType);
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(Message message)
        {
            try
            {
                var emailMessage = CreateEmailMessage(message);
                await SendAsync(emailMessage);
            }catch(Exception e)
            {

            }
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            //if (message.To.Count == 0)
            //    return null;
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Tournament App", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = string.Format("<h2 style='color:red;'>{0}</h2>", message.Content) };

            if (message.Attachments != null && message.Attachments.Any())
            {

                byte[] fileBytes;
                foreach (var attachment in message.Attachments)
                {
                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            using (var stream = File.Open(attachment.FileName, FileMode.Open))
                            {
                                stream.CopyTo(ms);// Use stream
                                fileBytes = ms.ToArray();
                            }
                        }
                        bodyBuilder.Attachments.Add(attachment.FileName, fileBytes, ContentType.Parse(attachment.ContentType));
                    }
                    
                    finally
                    {
                        File.Delete(attachment.FileName);
                    }
                }
            }
            emailMessage.Body = bodyBuilder.ToMessageBody();

            return emailMessage;
        }
        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.SslOnConnect);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                    client.Send(mailMessage);
                }catch(Exception e)
                {
                    
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
        public async Task<FormFile> CreateIcsAsync(string teamName, string foeName, DateTime dateTime, string tournamentType)
        {
            //some variables for demo purposes
            DateTime DateStart = dateTime;
            DateTime DateEnd = dateTime;

            string Summary = "Your match vs " + foeName + "";
            string Location = "Online";
            string Description = "Add the match to your calendar!";

            //create a new stringbuilder instance
            StringBuilder sb = new StringBuilder();

            //start the calendar item
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:TOURNAMENT_APP"); // change to site address after cloud deployment
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");

            //add the event
            sb.AppendLine("BEGIN:VEVENT");

            sb.AppendLine("DTSTART:" + DateStart.ToString("yyyyMMdd") + "T000000");
            sb.AppendLine("DTEND:" + DateEnd.ToString("yyyyMMdd") + "T000000");

            sb.AppendLine("SUMMARY:" + Summary + "");
            sb.AppendLine("LOCATION:" + Location + "");
            sb.AppendLine("DESCRIPTION: " + Description + "");
            sb.AppendLine("PRIORITY:3");
            sb.AppendLine("END:VEVENT");

            //end calendar item
            sb.AppendLine("END:VCALENDAR");

            //create a string from the stringbuilder
            string CalendarItem = sb.ToString();

            string currFile = string.Empty;

            switch (tournamentType)
            {
                case "default":
                    currFile = "matches of " + teamName + " vs " + foeName + ".ics";
                    break;

                case "rr":
                    currFile = "matches of " + teamName + " vs " + foeName + " home.ics";

                    if (File.Exists(currFile))
                        currFile = "matches of " + teamName + " vs " + foeName + " away.ics";
                    break;
            }

            var stream = new FileStream(currFile, FileMode.Create);
            byte[] buf = Encoding.Unicode.GetBytes(CalendarItem);
            stream.Write(buf, 0, buf.Length);
            var formFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/calendar"
            };

            stream.Flush();
            stream.Close();

            return formFile;
        }
    } 
}
