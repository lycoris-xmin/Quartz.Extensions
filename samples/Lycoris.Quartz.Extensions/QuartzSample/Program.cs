using Lycoris.Quartz.Extensions;
using Lycoris.Quartz.Extensions.Services;
using Microsoft.AspNetCore.Mvc;
using QuartzSample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQuartzSchedulerCenter(builder =>
{
    builder.AddJob<TestJob>();
    builder.AddJob<TestJob2>();

    builder.DisabledRunHostedJob();
});

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", ([FromServices] IQuartzSchedulerCenter center) =>
{
    center.ManualRunHostedJobsAsync();

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}