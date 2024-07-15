using Lycoris.Quartz.Constant;
using Quartz;
using System;

namespace Lycoris.Quartz
{
    /// <summary>
    /// 
    /// </summary>
    public static class QuartzExtention
    {
        /// <summary>
        /// 获取当前任务运行唯一Id
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetJobTraceId(this IJobExecutionContext context) => context.JobDetail.JobDataMap.Get(QuartzConstant.TRACE_ID) as string;

        /// <summary>
        /// 获取调度任务名称
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetJobName(this IJobExecutionContext context) => context.JobDetail.JobDataMap.Get(QuartzConstant.JOB_NAME) as string ?? "";

        /// <summary>
        /// 获取任务结束时间
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DateTime? GetEndTime(this IJobExecutionContext context)
        {
            var val = context.JobDetail.JobDataMap.Get(QuartzConstant.END_TIME) as string;

            if (!DateTime.TryParse(val, out DateTime dt))
                return null;

            return dt;
        }

        /// <summary>
        /// 获取任务启动参数
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetJobArgs(this IJobExecutionContext context) => context.JobDetail.JobDataMap.Get(QuartzConstant.JOB_ARGS) as string ?? "";

        /// <summary>
        /// 获取任务启动参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T GetJobArgs<T>(this IJobExecutionContext context) where T : class, new()
        {
            var value = context.GetJobArgs();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// 添加调度任务错误信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        public static void AddJobException(this IJobExecutionContext context, Exception ex) => context.JobDetail.JobDataMap.Put(QuartzConstant.EXCEPTION, ex);

        /// <summary>
        /// 获取调度任务错误信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Exception GetJobException(this IJobExecutionContext context) => context.JobDetail.JobDataMap.Get(QuartzConstant.EXCEPTION) as Exception;

        /// <summary>
        /// 添加自定义信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="message"></param>
        public static void AddJobDataMap(this IJobExecutionContext context, string key, string message) => context.JobDetail.JobDataMap.Put(key, message);

        /// <summary>
        /// 添加自定义信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="message"></param>
        public static void AddJobDataMap<T>(this IJobExecutionContext context, string key, T message) => context.JobDetail.JobDataMap.Put(key, message);

        /// <summary>
        /// 获取自定义信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static string GetJobDataMap(this IJobExecutionContext context, string key, string defaultvalue = "") => context.JobDetail.JobDataMap.Get(key) as string ?? defaultvalue;

        /// <summary>
        /// 获取自定义信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetJobDataMap<T>(this IJobExecutionContext context, string key) => (T)context.JobDetail.JobDataMap.Get(key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="where"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static JobBuilder UsingJobDataIf(this JobBuilder builder, bool where, string key, string value) => where ? builder.UsingJobData(key, value) : builder;
    }
}
