using Microsoft.AspNetCore.Identity;
using MimeKit;
using TournamentApp.Data.Models;

namespace TournamentApp.Data.Services
{
    public interface IMailService
    {
        Task SendEmailOnMatchCreationAsync(IList<Team> teams, string type);
        Task SendEmailOnAddedToTournamentAsync(string emailAddress);
        void ClearCalendarEvents();
    }

    public class MailService : IMailService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;


        public MailService(UserManager<ApplicationUser> userManager, IEmailSender emailSender)

        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task SendEmailOnMatchCreationAsync(IList<Team> teams, string scheduleType)
        {
            /* Send email once a schedule is created */

            IList<Match> matches = new List<Match>();

            foreach (var team in teams)
            {
                foreach (var match in team.Matches)
                {
                    if (match != null)
                        matches.Add(match);
                }

            }

            // for every match of the team
            foreach (var team in teams)
            {
                if(team != null)
                {
                    var matchEvents = new FormFileCollection();

                    foreach (var match in matches)
                    {
                        if (match != null)
                        {
                            if (team.TeamName == match.TeamA.TeamName
                            || team.TeamName == match.TeamB.TeamName)
                            {
                                string foeName = string.Empty;

                                if (team.TeamName == match.TeamA.TeamName)
                                {
                                    foeName = match.TeamB.TeamName;
                                }
                                else if (team.TeamName == match.TeamB.TeamName)
                                {
                                    foeName = match.TeamA.TeamName;
                                }

                                matchEvents.Add(await _emailSender.CreateIcsAsync(team.TeamName, foeName, match.MatchDate, scheduleType));
                            }
                        }
                    }

                    var emails = new List<MailboxAddress>();

                    // for all member of the team
                    foreach (var member in team.Participants)
                    {
                        if (member != null)
                        {
                            emails.Add(new MailboxAddress(null, (await _userManager.FindByIdAsync(member.UserId)).Email));
                        }

                    }

                    await _emailSender.SendEmailAsync(
                        new Message(
                            emails,
                            "New Match",
                            " You have been scheduled to a match!",
                            matchEvents)
                    );
                }
                
            }
            ClearCalendarEvents();
        }

            public async Task SendEmailOnAddedToTournamentAsync(string emailAddress)
        {
            /* Send email once a participant added to a team */
            var emails = new List<MailboxAddress>();
            emails.Add(new MailboxAddress("Tournament App", emailAddress));

            await _emailSender.SendEmailAsync(
                new Message(
                    emails,
                    "New Tournament",
                    " You have been added to a tournament!",
                    null)
            );
        }

        public void ClearCalendarEvents()
        {
            string[] files = System.IO.Directory.GetFiles("./", "*.ics");

            if (files != null)
            {
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }

        }
    }
}
