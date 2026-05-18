using Lycoris.Quartz.Constant;
using Lycoris.Quartz.Listener;
using Lycoris.Quartz.Options;
using Lycoris.Quartz.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Specialized;

namespace Lycoris.Quartz
{
    /// <summary>
    /// 
    /// </summary>
    public static class QuartzBuilderExtensions
    {
        /// <summary>
        /// 添加基础调度任务服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IServiceCollection AddBaseQuartzSchedulerCenter(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<QuartzJobRunner>();
            services.AddSingleton<IQuartzSchedulerCenter, QuartzSchedulerCenter>();
            return services;
        }

        /// <summary>
        /// 添加Quartz调度任务服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerCenter(this IServiceCollection services)
        {
            services.TryAddSingleton<IJobListener, JobListener>();
            return services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>().AddBaseQuartzSchedulerCenter();
        }

        /// <summary>
        /// 添加Quartz调度任务服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerCenter(this IServiceCollection services, Action<QuartzBuilder> configure)
        {
            var buidler = new QuartzBuilder(services);

            configure.Invoke(buidler);

            // 注册ISchedulerFactory的实例
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>(x =>
            {
                var options = new NameValueCollection
                {
                    { "quartz.threadPool.ThreadCount", buidler.ThreadCount.ToString() },
                    { "quartz.scheduler.instanceName", buidler.InstanceName }
                };

                if (!string.IsNullOrEmpty(buidler.JobStoreType))
                    options["quartz.jobStore.type"] = buidler.JobStoreType;

                if (!string.IsNullOrEmpty(buidler.TablePrefix))
                    options["quartz.jobStore.tablePrefix"] = buidler.TablePrefix;

                if (!string.IsNullOrEmpty(buidler.DataSource))
                    options["quartz.jobStore.dataSource"] = buidler.DataSource;

                foreach (string key in buidler.Properties.Keys)
                    options[key] = buidler.Properties[key];

                return new StdSchedulerFactory(options);
            }).AddBaseQuartzSchedulerCenter();

            services.TryAddSingleton<IJobListener, JobListener>();

            if (buidler.EnableRunStandbyJobOnApplicationStart)
                services.AddHostedService<DefaultQuartzJobHostedService>();

            return services;
        }

        /// <summary>
        /// 添加调度任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerJob<T>(this IServiceCollection services) where T : IJob
        {
            var job = QuartzJobHelper.GetJob<T>();

            var option = ToSchedulerOption(job);

            services.AddScoped(job.JobType);
            services.AddSingleton(option);

            return services;
        }

        /// <summary>
        /// 添加调度任务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="jobTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerJob(this IServiceCollection services, params Type[] jobTypes)
        {
            var jobs = QuartzJobHelper.GetJobs(jobTypes);
            if (jobs.Count > 0)
            {
                foreach (var item in jobs)
                {
                    services.AddScoped(item.JobType);

                    var option = ToSchedulerOption(item);

                    services.AddSingleton(option);
                }
            }

            return services;
        }

        /// <summary>
        /// 添加调度任务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="jobType"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerJob(this IServiceCollection services, Type jobType, Action<QuartzSchedulerOption> configure)
        {
            var job = QuartzJobHelper.GetJob(jobType, false);

            var option = new QuartzSchedulerOption
            {
                JobType = job.JobType
            };

            configure.Invoke(option);

            services.AddScoped(job.JobType);
            services.AddSingleton(option);

            return services;
        }

        /// <summary>
        /// 添加调度任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerJob<T>(this IServiceCollection services, Action<QuartzSchedulerOption> configure) where T : IJob
        {
            var job = QuartzJobHelper.GetJob<T>(false);

            var option = new QuartzSchedulerOption
            {
                JobType = job.JobType
            };

            configure.Invoke(option);

            services.AddScoped(job.JobType);
            services.AddSingleton(option);

            return services;
        }

        /// <summary>
        /// 将 QuartzJobType 映射为 QuartzSchedulerOption
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private static QuartzSchedulerOption ToSchedulerOption(QuartzJobType job)
        {
            var s = job.JobSettings;
            return new QuartzSchedulerOption
            {
                JobType = job.JobType,
                Standby = s.Standby,
                BeginTime = new DateTime(2000, 1, 1),
                Trigger = s.Trigger,
                Cron = s.Cron,
                IntervalSecond = s.IntervalSecond,
                RunTimes = s.RunTimes,
                JobGroup = string.IsNullOrEmpty(s.JobGroup) ? QuartzConstant.JOB_DEFAULT_GROUP : s.JobGroup,
                JobName = s.JobName,
                CronRunOnProceed = s.CronRunOnProceed
            };
        }
    }
}
