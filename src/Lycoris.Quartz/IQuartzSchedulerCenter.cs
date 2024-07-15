using Lycoris.Quartz.Options;
using Quartz;
using System.Threading.Tasks;

namespace Lycoris.Quartz
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
        /// <param name="sche"></param>
        /// <returns></returns>
        Task AddJobAsync(QuartzSchedulerOption sche);

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task AddOnceJobAsync<T>() where T : IJob;

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="args">任务启动参数</param>
        /// <returns></returns>
        Task AddOnceJobAsync<T, TArgs>(TArgs args) where T : IJob where TArgs : class;

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="args"></param>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        Task AddOnceJobAsync<T, TArgs>(TArgs args, string jobKey) where T : IJob where TArgs : class;

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TArgs"></typeparam>
        /// <param name="args"></param>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task AddOnceJobAsync<T, TArgs>(TArgs args, string jobKey, string jobGroup) where T : IJob where TArgs : class;

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">任务启动参数</param>
        /// <returns></returns>
        Task AddOnceJobAsync<T>(string args) where T : IJob;

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        Task AddOnceJobAsync<T>(string args, string jobKey) where T : IJob;

        /// <summary>
        /// 添加单次执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="jobKey"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task AddOnceJobAsync<T>(string args, string jobKey, string jobGroup) where T : IJob;

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
        /// 手动启动所有非待机任务
        /// </summary>
        /// <returns></returns>
        Task ManualRunNonStandbyJobsAsync();

        /// <summary>
        /// 手动启动所有任务
        /// </summary>
        /// <returns></returns>
        Task ManualRunAllJobsAsync();
    }
}
