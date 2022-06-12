using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data.Models;
using TournamentApp.Data;
using TournamentApp.Data.Repos;
using TournamentApp.Data.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TournamentAppDbContextConnection");
builder.Services.AddDbContext<TournamentAppDbContext>(options => 
    options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<TournamentAppDbContext>();
builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddTransient<ITournamentService, TournamentService>();
builder.Services.AddTransient<ITeamService, TeamService>();
builder.Services.AddTransient<IMatchService, MatchService>();
builder.Services.AddTransient<IMailService, MailService>();
//builder.Services.AddTransient(typeof(IRepo<,>), typeof(Repo<,>));
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
////builder.Services.AddScoped<ITournamentServiceManager, TourmantServiceManager>();
builder.Services.AddRazorPages().AddMvcOptions(options => options.Filters.Add(new AuthorizeFilter()));


// Mail Sender settings
builder.Services.AddSingleton(builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.Configure<FormOptions>(options => {
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1; 
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
