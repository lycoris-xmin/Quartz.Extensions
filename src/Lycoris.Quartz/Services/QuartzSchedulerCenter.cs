using Lycoris.Quartz.Extensions.Constant;
using Lycoris.Quartz.Extensions.Exceptions;
using Lycoris.Quartz.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Lycoris.Quartz.Extensions.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class QuartzSchedulerCenter : IQuartzSchedulerCenter
    {
        private const string DefaultJobGroup = "unclassified";
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 调度器
        /// </summary>
        private readonly IScheduler scheduler;
        /// <summary>
        /// 任务工厂
        /// </summary>
        private readonly IJobFactory jobFactory;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public QuartzSchedulerCenter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var schedulerFactory = serviceProvider.GetService<ISchedulerFactory>() ?? throw new ArgumentNullException(nameof(ISchedulerFactory), "unable to resolve 'ISchedulerFactory' service from constructor");

            scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

            jobFactory = serviceProvider.GetService<IJobFactory>();
        }

        /// <summary>
        /// 开启调度器
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartScheduleAsync()
        {
            if (scheduler.InStandbyMode)
            {
                if (jobFactory == null)
                    throw new ArgumentNullException(nameof(IJobFactory), "unable to resolve 'IJobFactory' service from constructor");

                scheduler.JobFactory = jobFactory;

                var schedulerListener = _serviceProvider.GetService<ISchedulerListener>();
                if (schedulerListener != null)
                    scheduler.ListenerManager.AddSchedulerListener(schedulerListener);

                var jobListener = _serviceProvider.GetService<IJobListener>();
                if (jobListener != null)
                    scheduler.ListenerManager.AddJobListener(jobListener);

                var triggerListener = _serviceProvider.GetService<ITriggerListener>();
                if (triggerListener != null)
                    scheduler.ListenerManager.AddTriggerListener(triggerListener);

                await scheduler.Start();
            }

            return scheduler.InStandbyMode;
        }

        /// <summary>
        /// 停止调度器
        /// </summary>
        public async Task<bool> StopScheduleAsync()
        {
            try
            {
                // 判断调度是否已经关闭
                if (!scheduler.InStandbyMode)
                {
                    //TODO  注意：Shutdown后Start会报错,所以这里使用暂停。
                    await scheduler.Standby();
                }

                return !scheduler.InStandbyMode;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 添加工作任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task AddJobAsync<T>() where T : IJob
        {
            var settings = typeof(T).GetCustomAttribute<QuartzJobAttribute>() ?? throw new Exception($"can not find QuartzJobAttribute by current job {typeof(T).Name},unable to add job");

            var option = new QuartzSchedulerOption()
            {
                BeginTime = new DateTime(2000, 1, 1),
                Trigger = settings.Trigger,
                Cron = settings.Cron,
                IntervalSecond = settings.IntervalSecond,
                RunTimes = settings.RunTimes,
                JobGroup = string.IsNullOrEmpty(settings.JobGroup) ? "unclassified" : settings.JobGroup,
                JobName = settings.JobName
            };

            return AddJobAsync<T>(option);
        }

        /// <summary>
        /// 添加工作任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sche"></param>
        /// <returns></returns>
        public Task AddJobAsync<T>(QuartzSchedulerOption sche) where T : IJob
        {
            sche.JobType = typeof(T);
            return AddJobAsync(sche);
        }

        /// <summary>
        /// 添加工作任务
        /// </summary>
        /// <param name="sche"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task AddJobAsync(QuartzSchedulerOption sche)
        {
            if (!IsInterfaceFrom<IJob>(sche.JobType))
                throw new Exception($"{sche.JobType.FullName} must implement 'IJob' or inherit the base class 'BaseQuartzJob'");

            if (string.IsNullOrEmpty(sche.JobGroup))
                sche.JobGroup = DefaultJobGroup;

            if (sche.Trigger == QuartzTriggerEnum.SIMPLE && sche.IntervalSecond <= 0 || sche.Trigger == QuartzTriggerEnum.CRON && string.IsNullOrEmpty(sche.Cron))
                throw new QuartzOptionException(sche.JobType, sche.Trigger);

            // 检查任务是否已存在
            var jobKey = new JobKey(sche.JobKey, sche.JobGroup);

            if (await scheduler.CheckExists(jobKey))
                throw new Exception($"job name {sche.JobName} already exists");

            var jobBuilder = JobBuilder.Create(sche.JobType);

            // 定义这个工作,并将其绑定到我们的IJob实现类                
            var job = jobBuilder.UsingJobData(QuartzConstant.JobKey, sche.JobKey)
                                .UsingJobData(QuartzConstant.JobName, sche.JobName)
                                .UsingJobDataIf(!string.IsNullOrEmpty(sche.JsonMap), QuartzConstant.JobJsonMap, sche.JsonMap)
                                .UsingJobData(QuartzConstant.JobArgs, sche.Args)
                                .WithDescription(sche.Remark)
                                .WithIdentity(sche.JobKey, sche.JobGroup)
                                .Build();

            var trigger = sche.Trigger == QuartzTriggerEnum.CRON
                ? CreateCronTrigger(sche)
                : CreateSimpleTrigger(sche);

            // 安排触发器开始作业
            await scheduler.ScheduleJob(job, trigger);
        }

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task AddOnceJobAsync<T>() where T : IJob
        {
            var options = _serviceProvider.GetServices<QuartzSchedulerOption>();
            var option = options.Where(x => x.JobType == typeof(T)).SingleOrDefault();

            // 检查任务是否已存在
            var job = new JobKey(option.JobKey, option.JobGroup);
            if (await scheduler.CheckExists(job))
            {
                await scheduler.TriggerJob(job);
                return;
            }

            await AddJobAsync<T>(new QuartzSchedulerOption()
            {
                Trigger = QuartzTriggerEnum.SIMPLE,
                IntervalSecond = 1,
                RunTimes = 1,
                BeginTime = new DateTime(2000, 1, 1),
                JobName = option.JobName,
                JobGroup = string.IsNullOrEmpty(option.JobGroup) ? "unclassified" : option.JobGroup,
                JobKey = option.JobKey,
            });
        }

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="args">启动参数</param>
        /// <returns></returns>
        public Task AddOnceJobAsync<T, TArgs>(TArgs args) where T : IJob where TArgs : class => AddOnceJobAsync<T>(Newtonsoft.Json.JsonConvert.SerializeObject(args));

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task AddOnceJobAsync<T>(string args) where T : IJob
        {
            var options = _serviceProvider.GetServices<QuartzSchedulerOption>();
            var option = options.Where(x => x.JobType == typeof(T)).SingleOrDefault();
            option.JobKey = Guid.NewGuid().ToString();

            // 检查任务是否已存在
            var job = new JobKey(option.JobKey, option.JobGroup);
            if (await scheduler.CheckExists(job))
            {
                if (option.Args == null)
                {
                    await scheduler.TriggerJob(job);
                    return;
                }

                var detail = await scheduler.GetJobDetail(job);
                if (detail.JobDataMap.ContainsKey(QuartzConstant.JobArgs) && detail.JobDataMap.GetString(QuartzConstant.JobArgs) == option.Args)
                {
                    await scheduler.TriggerJob(job);
                    return;
                }

                // 如果存在配置参数
                await scheduler.DeleteJob(job);
            }

            await AddJobAsync<T>(new QuartzSchedulerOption()
            {
                Trigger = QuartzTriggerEnum.SIMPLE,
                IntervalSecond = 1,
                RunTimes = 1,
                BeginTime = new DateTime(2000, 1, 1),
                JobName = option.JobName,
                JobGroup = string.IsNullOrEmpty(option.JobGroup) ? "unclassified" : option.JobGroup,
                JobKey = option.JobKey,
                Args = args
            });
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public async Task StartJobAsync(string jobKey, string jobGroup = "")
        {
            if (string.IsNullOrEmpty(jobGroup))
                jobGroup = DefaultJobGroup;

            //检查任务是否存在
            var job = new JobKey(jobKey, jobGroup);
            if (!await scheduler.CheckExists(job))
                throw new Exception($"this job {jobKey} already exists");

            var jobDetail = await scheduler.GetJobDetail(job);
            if (jobDetail != null)
            {
                var endTime = jobDetail.JobDataMap.GetString(QuartzConstant.EndTime);
                if (!string.IsNullOrEmpty(endTime) && DateTime.Parse(endTime) <= DateTime.Now)
                    throw new Exception($"this job {jobKey} has expired");
            }

            var state = await scheduler.GetTriggerState(new TriggerKey(jobKey));
            if (state == TriggerState.Normal)
                return;

            //任务已经存在则暂停任务
            await scheduler.ResumeJob(job);
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public async Task StopJobAsync(string jobKey, string jobGroup = "")
        {
            if (string.IsNullOrEmpty(jobGroup))
                jobGroup = DefaultJobGroup;

            //检查任务是否存在
            var job = new JobKey(jobKey, jobGroup);
            if (!await scheduler.CheckExists(job))
                return;

            var state = await scheduler.GetTriggerState(new TriggerKey(jobKey));
            if (state == TriggerState.Paused)
                return;

            await scheduler.PauseJob(job);
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public async Task RemoveobAsync(string jobKey, string jobGroup = "")
        {
            if (string.IsNullOrEmpty(jobGroup))
                jobGroup = DefaultJobGroup;

            //检查任务是否存在
            var job = new JobKey(jobKey, jobGroup);
            if (!await scheduler.CheckExists(job))
                return;

            var state = await scheduler.GetTriggerState(new TriggerKey(jobKey));
            if (state == TriggerState.Normal)
                await scheduler.PauseJob(job);

            await scheduler.DeleteJob(job);
        }

        /// <summary>
        /// 立即执行一次现有的任务
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public async Task RunJobAsync(string jobKey, string jobGroup)
        {
            if (string.IsNullOrEmpty(jobGroup))
                jobGroup = DefaultJobGroup;

            //检查任务是否存在
            var job = new JobKey(jobKey, jobGroup);
            if (!await scheduler.CheckExists(job))
                throw new Exception($"this job {jobKey} does not exist");

            await scheduler.TriggerJob(job);
        }

        /// <summary>
        /// 手动启动所有非待机任务
        /// </summary>
        /// <returns></returns>
        public async Task ManualRunNonStandbyJobsAsync()
        {
            var options = _serviceProvider.GetServices<QuartzSchedulerOption>();

            if (options.Count() == 0)
                return;

            foreach (var item in options)
            {
                if (!item.Standby)
                    await AddJobAsync(item);
            }
        }

        /// <summary>
        /// 手动启动所有任务
        /// </summary>
        /// <returns></returns>
        public async Task ManualRunAllJobsAsync()
        {
            var options = _serviceProvider.GetServices<QuartzSchedulerOption>();

            if (options.Count() == 0)
                return;

            foreach (var item in options)
                await AddJobAsync(item);
        }

        /// <summary>
        /// 创建类型Cron的触发器
        /// </summary>
        /// <param name="sche"></param>
        /// <returns></returns>
        private static ITrigger CreateCronTrigger(QuartzSchedulerOption sche)
        {
            var trigger = TriggerBuilder.Create();
            trigger = trigger.WithIdentity(sche.JobKey, sche.JobGroup);

            //开始时间
            trigger = trigger.StartAt(sche.BeginTime);

            //结束时间
            if (sche.EndTime.HasValue)
                trigger = trigger.EndAt(sche.EndTime);

            // 作业触发器
            trigger = trigger.WithCronSchedule(sche.Cron, cronScheduleBuilder => cronScheduleBuilder.WithMisfireHandlingInstructionFireAndProceed())
                             .ForJob(sche.JobKey, sche.JobGroup);

            return trigger.Build();
        }

        /// <summary>
        /// 创建类型Simple的触发器
        /// </summary>
        /// <param name="sche"></param>
        /// <returns></returns>
        private static ITrigger CreateSimpleTrigger(QuartzSchedulerOption sche)
        {
            var triggerBulider = TriggerBuilder.Create();

            triggerBulider = triggerBulider.WithIdentity(sche.JobKey, sche.JobGroup)
                                           .StartAt(sche.BeginTime);

            if (sche.EndTime.HasValue)
                triggerBulider = triggerBulider.EndAt(sche.EndTime);

            triggerBulider = triggerBulider.WithSimpleSchedule(x =>
            {
                if (sche.RunTimes > 0)
                {
                    x.WithIntervalInSeconds(sche.IntervalSecond)//执行时间间隔,单位秒
                     .WithRepeatCount(sche.RunTimes)//执行次数、默认从0开始
                     .WithMisfireHandlingInstructionFireNow();
                }
                else
                {
                    //执行时间间隔,单位秒
                    x.WithIntervalInSeconds(sche.IntervalSecond)
                     .RepeatForever()//无限循环
                     .WithMisfireHandlingInstructionFireNow();
                }
            });

            return triggerBulider.ForJob(sche.JobKey, sche.JobGroup).Build();
        }

        /// <summary>
        /// 判断一个类是否实现了某个接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsInterfaceFrom<T>(Type type)
        {
            var @interface = typeof(T);

            var intarfaces = type.GetInterfaces();
            if (intarfaces == null || intarfaces.Length == 0)
                return false;

            if (@interface.IsGenericType)
            {
                foreach (var item in intarfaces)
                {
                    if (item.GetGenericTypeDefinition() == @interface)
                        return true;
                }
            }
            else
                return intarfaces.Any(x => x == @interface);

            return false;
        }
    }
}
