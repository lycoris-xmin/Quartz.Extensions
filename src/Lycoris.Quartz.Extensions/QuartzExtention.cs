using Lycoris.Quartz.Extensions.Constant;
using Quartz;
using System;
using System.Collections.Generic;

namespace Lycoris.Quartz.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class QuartzExtention
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="where"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JobBuilder UsingJobDataIf(this JobBuilder builder, bool where, string key, string value)
        {
            if (where)
                builder.UsingJobData(key, value);

            return builder;
        }

        /// <summary>
        /// 获取当前任务运行唯一Id
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetJobTraceId(this IJobExecutionContext context)
            => context.GetJobDataMap(QuartzConstant.JobTraceId);

        /// <summary>
        /// 获取调度任务名称
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetJobName(this IJobExecutionContext context)
        {
            var jobName = context.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobName) ? context.JobDetail.JobDataMap.GetString(QuartzConstant.JobName) : "";
            return jobName ?? "";
        }

        /// <summary>
        /// 获取任务结束时间
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DateTime? GetEndTime(this IJobExecutionContext context)
        {
            var val = context.JobDetail.JobDataMap.GetString(QuartzConstant.EndTime);

            if (!DateTime.TryParse(val, out DateTime dt))
                return null;

            return dt;
        }

        /// <summary>
        /// 添加调度任务自定义信息
        /// </summary>
        /// <param name="JobContext"></param>
        /// <param name="message"></param>
        public static void AddJobMessage(this IJobExecutionContext JobContext, string message)
        {
            List<string> list = null;

            if (JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobMessage))
                list = JobContext.JobDetail.JobDataMap[QuartzConstant.JobMessage] as List<string>;

            if (list == null)
                list = new List<string>();

            list.Add(message);

            JobContext.JobDetail.JobDataMap.Add(QuartzConstant.JobMessage, list);
        }

        /// <summary>
        /// 获取调度任务自定义信息
        /// </summary>
        /// <param name="JobContext"></param>
        /// <returns></returns>
        public static List<string> GetJobMessage(this IJobExecutionContext JobContext)
        {
            if (!JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobMessage))
                return null;

            return JobContext.JobDetail.JobDataMap[QuartzConstant.JobMessage] as List<string>;
        }

        /// <summary>
        /// 获取调度任务配置信息
        /// </summary>
        /// <param name="JobContext"></param>
        /// <param name="data"></param>
        public static void AddJobDetail(this IJobExecutionContext JobContext, string data)
        {
            if (JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobDetail))
                JobContext.JobDetail.JobDataMap[QuartzConstant.JobDetail] = data;
            else
                JobContext.JobDetail.JobDataMap.Add(QuartzConstant.JobDetail, data);
        }

        /// <summary>
        /// 获取调度任务配置信息
        /// </summary>
        /// <param name="JobContext"></param>
        /// <returns></returns>
        public static string GetJobDetail(this IJobExecutionContext JobContext)
        {
            if (!JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobDetail))
                return string.Empty;

            return JobContext.JobDetail.JobDataMap[QuartzConstant.JobDetail] as string;
        }

        /// <summary>
        /// 添加调度任务错误信息
        /// </summary>
        /// <param name="JobContext"></param>
        /// <param name="ex"></param>
        public static void AddJobException(this IJobExecutionContext JobContext, Exception ex)
        {
            if (JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobException))
                JobContext.JobDetail.JobDataMap[QuartzConstant.JobException] = ex;
            else
                JobContext.JobDetail.JobDataMap.Add(QuartzConstant.JobException, ex);
        }

        /// <summary>
        /// 获取调度任务错误信息
        /// </summary>
        /// <param name="JobContext"></param>
        /// <returns></returns>
        public static Exception GetJobException(this IJobExecutionContext JobContext)
        {
            if (!JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobException))
                return default;

            return JobContext.JobDetail.JobDataMap[QuartzConstant.JobException] as Exception;
        }

        /// <summary>
        /// 添加自定义信息
        /// </summary>
        /// <param name="JobContext"></param>
        /// <param name="key"></param>
        /// <param name="message"></param>
        public static void AddJobDataMap(this IJobExecutionContext JobContext, string key, string message)
        {
            if (JobContext.JobDetail.JobDataMap.ContainsKey(key))
                JobContext.JobDetail.JobDataMap[key] = message;
            else
                JobContext.JobDetail.JobDataMap.Add(key, message);
        }

        /// <summary>
        /// 获取自定义信息
        /// </summary>
        /// <param name="JobContext"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static string GetJobDataMap(this IJobExecutionContext JobContext, string key, string defaultvalue = "")
            => JobContext.JobDetail.JobDataMap.ContainsKey(key) ? JobContext.JobDetail.JobDataMap[key] as string : defaultvalue;

        /// <summary>
        /// 清楚Map中的参数
        /// </summary>
        /// <param name="JobContext"></param>
        public static void JodDataClear(this IJobExecutionContext JobContext)
        {
            if (JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobMessage))
                JobContext.JobDetail.JobDataMap[QuartzConstant.JobMessage] = "";
            if (JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobDetail))
                JobContext.JobDetail.JobDataMap[QuartzConstant.JobDetail] = "";
            if (JobContext.JobDetail.JobDataMap.ContainsKey(QuartzConstant.JobException))
                JobContext.JobDetail.JobDataMap[QuartzConstant.JobException] = "";
        }
    }
}
