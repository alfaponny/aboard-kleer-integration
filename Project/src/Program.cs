using AboardKleerIntegration.Services;
using AboardKleerIntegration.Sync;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AboardKleerIntegration.Config;
using AboardKleerIntegration.Services.Slack;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<IKleerService, KleerService>();
builder.Services.AddHttpClient<IAboardService, AboardService>();
builder.Services.AddTransient<UserSyncService>();
builder.Services.Configure<KleerOptions>(
    builder.Configuration.GetSection("Kleer"));
builder.Services.Configure<AboardOptions>(
    builder.Configuration.GetSection("Aboard"));
builder.Services.AddHttpClient<ISlackService, SlackService>();
builder.Services.Configure<SlackOptions>(
    builder.Configuration.GetSection("Slack"));

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("UserSyncJob");
    q.AddJob<UserSyncJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("UserSyncJob-trigger")
        .WithCronSchedule("0 0 0 ? * SUN")); // Every sunday at midnight
        //.StartNow()); //(comment out WithIdentity and WithCronSchedule and uncomment StartNow to run immediately for testing)
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();
await app.RunAsync();