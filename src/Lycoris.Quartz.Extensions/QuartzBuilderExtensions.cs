using Lycoris.Quartz.Extensions.Options;
using Lycoris.Quartz.Extensions.Services;
using Lycoris.Quartz.Extensions.Services.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Linq;
using System.Reflection;

namespace Lycoris.Quartz.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class QuartzBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IServiceCollection AddBaseQuartzSchedulerCenter(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<QuartzJobRunner>();
            services.AddSingleton<IQuartzSchedulerCenter, QuartzSchedulerCenter>();
            services.AddHostedService<DefaultQuartzJobHostedService>();
            return services;
        }

        /// <summary>
        /// 添加Quartz调度任务服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerCenter(this IServiceCollection services) => services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>().AddBaseQuartzSchedulerCenter();

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
            buidler.Build();
            return services;
        }

        /// <summary>
        /// 添加调度任务程序集
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzJobAssembly(this IServiceCollection services, Assembly assembly)
        {
            QuartzSchedulderStore.AddJobAssembly(assembly);
            return services;
        }

        /// <summary>
        /// 添加调度任务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="jobTypes"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzJob(this IServiceCollection services, params Type[] jobTypes)
        {
            QuartzSchedulderStore.AddJobTypes(jobTypes);
            return services;
        }

        /// <summary>
        /// 添加调度任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzJob<T>(this IServiceCollection services) where T : IJob
        {
            QuartzSchedulderStore.AddJobTypes(typeof(T));
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection QuartzJobBuild(this IServiceCollection services)
        {
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

            return services;
        }
    }
}
