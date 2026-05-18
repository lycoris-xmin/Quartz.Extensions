using Lycoris.Quartz.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lycoris.Quartz
{
    internal class QuartzJobHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static List<QuartzJobType> GetJobsByAssembly(Assembly assembly)
        {
            var jobjTypes = assembly.GetTypes().Where(x => x.IsClass && x.IsPublic && !x.IsAbstract)
                            .Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) || typeof(IJob).IsAssignableFrom(x))
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
        /// <param name="type"></param>
        private static void ValidateJobType(Type type)
        {
            if (!type.IsClass || !type.IsPublic || type.IsAbstract)
                throw new Exception($"the {type.FullName} must be a public class and cannot be an abstract class");

            if (!type.IsSubclassOf(typeof(BaseQuartzJob)) && !typeof(IJob).IsAssignableFrom(type))
                throw new Exception($"the {type.FullName} must be subclass of 'BaseQuartzJob' or assignable from 'IJob'");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static List<QuartzJobType> GetJobs(params Type[] types)
        {
            foreach (var type in types)
                ValidateJobType(type);

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
            ValidateJobType(type);

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
