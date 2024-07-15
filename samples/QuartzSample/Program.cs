using Lycoris.Quartz;
using Microsoft.AspNetCore.Mvc;
using QuartzSample;

var builder = WebApplication.CreateBuilder(args);

//
builder.Services.AddQuartzSchedulerCenter(opt => opt.EnableRunStandbyJobOnApplicationStart = true)
                .AddQuartzSchedulerJob<TestJob>()
                .AddQuartzSchedulerJob<TestJob2>()
                .AddQuartzSchedulerJob<TestJob3>(opt =>
                {
                    opt.JobName = "测试任务3";
                    opt.Trigger = QuartzTriggerEnum.SIMPLE;
                    opt.IntervalSecond = 10;
                    opt.Standby = true;
                });

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async ([FromServices] IQuartzSchedulerCenter center) =>
{
    for (int i = 0; i < 10; i++)
    {
        await center.AddOnceJobAsync<TestJob3>($"测试启动参数-{i}");
    }

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