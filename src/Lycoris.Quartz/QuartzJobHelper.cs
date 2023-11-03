using Lycoris.Quartz.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lycoris.Quartz.Extensions
{
    internal class QuartzJobHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        internal static List<QuartzJobType> GetJobsByAssembly(Assembly assembly)
        {
            var jobjTypes = assembly.GetTypes().Where(x => x.IsClass && x.IsPublic && !x.IsAbstract)
                            .Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) || x.IsAssignableFrom(typeof(IJob)))
                            .Select(x => new QuartzJobType()
                            {
                                JobType = x,
                                JobSettings = x.GetCustomAttribute<QuartzJobAttribute>()
                            })
                            .Where(x => x.JobSettings != null).ToList();

            return jobjTypes ?? new List<QuartzJobType>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static List<QuartzJobType> GetJobs(params Type[] types)
        {
            var modifierList = types.Where(x => !x.IsClass || x.IsPublic || x.IsAbstract).Select(x => x.FullName).ToList() ?? new List<string>();

            if (modifierList.Any())
                throw new Exception($"the [{string.Join(",", modifierList)}] must be a public class and cannot be a abstract class");

            var jobClass = types.Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) && x.IsAssignableFrom(typeof(IJob))).Select(x => x.FullName).ToList() ?? new List<string>();

            if (jobClass.Any())
                throw new Exception($"the [{string.Join(",", jobClass)}] muse be subclass of 'BaseQuartzJob' or assignable from 'IJob'");

            var jobjTypes = types.Select(x => new QuartzJobType()
            {
                JobType = x,
                JobSettings = x.GetCustomAttribute<QuartzJobAttribute>()
            }).ToList() ?? new List<QuartzJobType>();

            var settings = jobjTypes.Where(x => x.JobSettings == null).Select(x => x.JobType.FullName).ToList();
            if (settings.Any())
                throw new Exception($"the [{string.Join(",", settings)}] scheduled task configuration not set");

            return jobjTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="checkAttribute"></param>
        /// <returns></returns>
        internal static QuartzJobType GetJob<T>(bool checkAttribute = true) where T : IJob => GetJob(typeof(T), checkAttribute);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="checkAttribute"></param>
        internal static QuartzJobType GetJob(Type type, bool checkAttribute = true)
        {
            if (!type.IsClass || !type.IsPublic || type.IsAbstract)
                throw new Exception($"the {type.FullName} must be a public class and cannot be a abstract class");

            if (!type.IsSubclassOf(typeof(BaseQuartzJob)) && !type.IsAssignableFrom(typeof(IJob)))
                throw new Exception($"the {type.FullName} muse be subclass of 'BaseQuartzJob' or assignable from 'IJob'");

            var settings = type.GetCustomAttribute<QuartzJobAttribute>();
            if (checkAttribute && settings == null)
                throw new Exception($"the {type.FullName} scheduled task configuration not set");

            return new QuartzJobType()
            {
                JobType = type,
                JobSettings = settings
            };
        }
    }
}
