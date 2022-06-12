using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace TournamentApp.Data.Models
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public IFormFileCollection Attachments { get; set; }

        public Message(IEnumerable<MailboxAddress> to, string subject, string content, [FromForm]IFormFileCollection attachments)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => x));
            Subject = subject;
            Content = content;
            Attachments = attachments;
        }
    }   


}
