using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TournamentApp.Data.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }
    [PersonalData]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
    [PersonalData]
    [Display(Name = "About")]
    public string? About { get; set; }

    public List<Participant>? UserTournaments { get; set; }
}

