using Lycoris.Quartz.Listener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace Lycoris.Quartz
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
        /// 调度器实例名称
        /// </summary>
        public string InstanceName { get; set; } = "QuartzScheduler";

        /// <summary>
        /// 表前缀（用于ADO JobStore）
        /// </summary>
        public string TablePrefix { get; set; } = "QRTZ_";

        /// <summary>
        /// JobStore 类型全名（如 "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"）
        /// </summary>
        public string JobStoreType { get; set; }

        /// <summary>
        /// 数据源连接字符串名称
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 自定义 Quartz 属性集合
        /// </summary>
        public System.Collections.Specialized.NameValueCollection Properties { get; } = new System.Collections.Specialized.NameValueCollection();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="services"></param>
        public QuartzBuilder(IServiceCollection services) => this.services = services;

        /// <summary>
        /// 添加调度器监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuartzBuilder AddSchedulerListener<T>() where T : SchedulerListener
        {
            services.TryAddSingleton<ISchedulerListener, T>();
            return this;
        }

        /// <summary>
        /// 添加调度任务监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuartzBuilder AddJobListener<T>() where T : JobListener
        {
            services.TryAddSingleton<IJobListener, T>();
            return this;
        }

        /// <summary>
        /// 添加调度触发器监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuartzBuilder AddTriggerListener<T>() where T : TriggerListener
        {
            services.TryAddSingleton<ITriggerListener, T>();
            return this;
        }

        /// <summary>
        /// 启用程序启动时自动启动非待机定时任务
        /// 默认：<see langword="false"/> 
        /// 禁用状态下，手动启动调度器 <see cref="IQuartzSchedulerCenter.StartScheduleAsync"/> 并执行 <see cref="IQuartzSchedulerCenter.ManualRunNonStandbyJobsAsync"/> 启动待机任务
        /// </summary>
        public bool EnableRunStandbyJobOnApplicationStart { get; set; } = false;
    }
}
