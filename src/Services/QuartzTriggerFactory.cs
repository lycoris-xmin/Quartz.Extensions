using Lycoris.Quartz.Options;
using Quartz;
using System;

namespace Lycoris.Quartz.Services
{
    internal static class QuartzTriggerFactory
    {
        internal static ITrigger CreateCronTrigger(QuartzSchedulerOption sche)
        {
            var trigger = TriggerBuilder.Create();
            trigger = trigger.WithIdentity(sche.JobName, sche.JobGroup);
            trigger = trigger.StartAt(sche.BeginTime);

            if (sche.EndTime.HasValue)
                trigger = trigger.EndAt(sche.EndTime);

            if (sche.CronRunOnProceed)
                trigger.WithCronSchedule(sche.Cron, b => b.WithMisfireHandlingInstructionFireAndProceed());
            else
                trigger.WithCronSchedule(sche.Cron, b => b.WithMisfireHandlingInstructionDoNothing());

            return trigger.ForJob(sche.JobName, sche.JobGroup).Build();
        }

        internal static ITrigger CreateSimpleTrigger(QuartzSchedulerOption sche)
        {
            var triggerBuilder = TriggerBuilder.Create();
            triggerBuilder = triggerBuilder.WithIdentity(sche.JobName, sche.JobGroup)
                                           .StartAt(sche.BeginTime);

            if (sche.EndTime.HasValue)
                triggerBuilder = triggerBuilder.EndAt(sche.EndTime);

            triggerBuilder = triggerBuilder.WithSimpleSchedule(x =>
            {
                if (sche.RunTimes > 0)
                {
                    x.WithIntervalInSeconds(sche.IntervalSecond)
                     .WithRepeatCount(sche.RunTimes)
                     .WithMisfireHandlingInstructionFireNow();
                }
                else
                {
                    x.WithIntervalInSeconds(sche.IntervalSecond)
                     .RepeatForever()
                     .WithMisfireHandlingInstructionFireNow();
                }
            });

            return triggerBuilder.ForJob(sche.JobName, sche.JobGroup).Build();
        }
    }
}
