using Lycoris.Quartz.Extensions.Listener;
using Lycoris.Quartz.Extensions.Options;
using Lycoris.Quartz.Extensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace Lycoris.Quartz.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class QuartzBuilder
    {
        private readonly IServiceCollection services;

        /// <summary>
        /// 线程池个数（默认为：10个）
        /// </summary>
        public int ThreadCount { get; set; } = 10;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="services"></param>
        public QuartzBuilder(IServiceCollection services) => this.services = services;

        /// <summary>
        /// 添加调度任务集合
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public QuartzBuilder AddJob(params Type[] types)
        {
            QuartzSchedulderStore.AddJobTypes(types);
            return this;
        }

        /// <summary>
        /// 添加调度任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuartzBuilder AddJob<T>() where T : IJob
        {
            QuartzSchedulderStore.AddJobTypes(typeof(T));
            return this;
        }

        /// <summary>
        /// 添加调度器监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuartzBuilder AddSchedulerListener<T>() where T : SchedulerListener
        {
            services.AddSingleton<ISchedulerListener, T>();
            return this;
        }

        /// <summary>
        /// 添加调度任务监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuartzBuilder AddJobListener<T>() where T : JobListener
        {
            services.AddSingleton<IJobListener, T>();
            return this;
        }

        /// <summary>
        /// 添加调度触发器监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuartzBuilder AddTriggerListener<T>() where T : TriggerListener
        {
            services.AddSingleton<ITriggerListener, T>();
            return this;
        }

        /// <summary>
        /// 禁用自动执行待机列表任务
        /// 需要手动启动待机列表任务请执行 <see cref="IQuartzSchedulerCenter.ManualRunHostedJobsAsync"/>
        /// </summary>
        public void DisabledRunHostedJob() => QuartzSchedulderStore.DisabledRunHostedJob = true;

        /// <summary>
        /// 配置构建
        /// </summary>
        internal void Build()
        {
            // 注册ISchedulerFactory的实例
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>(x =>
            {
                var options = new NameValueCollection
                {
                    { "quartz.threadPool.ThreadCount", ThreadCount.ToString() }
                };

                return new StdSchedulerFactory(options);
            });

            // 注册Job服务
            var jobTypes = QuartzSchedulderStore.GetAllJobTypes();
            if (jobTypes != null && jobTypes.Any())
            {
                foreach (var item in jobTypes)
                {
                    if (item.JobType == null)
                        continue;

                    services.TryAddScoped(item.JobType);
                }
            }

            //
            services.AddBaseQuartzSchedulerCenter().AddHostedService<DefaultQuartzJobHostedService>();
        }
    }
}
