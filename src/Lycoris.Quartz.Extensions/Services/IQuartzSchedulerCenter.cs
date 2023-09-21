using Lycoris.Quartz.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Lycoris.Quartz.Extensions.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQuartzSchedulerCenter
    {
        /// <summary>
        /// 开启调度器
        /// </summary>
        /// <returns></returns>
        Task<bool> StartScheduleAsync();

        /// <summary>
        /// 停止调度器
        /// </summary>
        Task<bool> StopScheduleAsync();

        /// <summary>
        /// 添加工作任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task AddJobAsync<T>() where T : IJob;

        /// <summary>
        /// 添加工作任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sche"></param>
        /// <returns></returns>
        Task AddJobAsync<T>(QuartzSchedulerOption sche) where T : IJob;

        /// <summary>
        /// 添加工作任务
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="sche"></param>
        /// <returns></returns>
        Task AddJobAsync(Type jobType, QuartzSchedulerOption sche);

        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task StartJobAsync(string jobKey, string jobGroup = "");

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task StopJobAsync(string jobKey, string jobGroup = "");

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task RemoveobAsync(string jobKey, string jobGroup = "");

        /// <summary>
        /// 立即执行一次
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task RunJobAsync(string jobKey, string jobGroup);

        /// <summary>
        /// 手动启动待机列表任务
        /// </summary>
        /// <returns></returns>
        Task ManualRunHostedJobsAsync();
    }
}
