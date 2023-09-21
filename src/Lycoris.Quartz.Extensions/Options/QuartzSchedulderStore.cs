using Lycoris.Quartz.Extensions.Job;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lycoris.Quartz.Extensions.Options
{
    internal static class QuartzSchedulderStore
    {
        /// <summary>
        /// 
        /// </summary>
        internal static bool DisabledRunHostedJob { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        internal static Dictionary<Type, QuartzSchedulerOption> HostedJobOptions { get; set; } = new Dictionary<Type, QuartzSchedulerOption>();

        /// <summary>
        /// 
        /// </summary>
        static readonly List<QuartzJobTypes> JobTypes = new List<QuartzJobTypes>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        internal static void AddJobAssembly(Assembly assembly) => AddJobTypes(assembly.GetTypes());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="types"></param>
        internal static void AddJobTypes(params Type[] types)
        {
            var jobjTypes = types.Where(x => x.IsClass && x.IsPublic && !x.IsAbstract)
                                 .Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) || x.IsAssignableFrom(typeof(IJob)))
                                 .Select(x => new QuartzJobTypes()
                                 {
                                     JobType = x,
                                     JobSettings = x.GetCustomAttribute<QuartzJobAttribute>()
                                 })
                                 .Where(x => x.JobSettings != null).ToList();

            JobTypes.AddRange(jobjTypes ?? new List<QuartzJobTypes>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        internal static void AddJobTypes(Type type)
        {
            var settings = type.GetCustomAttribute<QuartzJobAttribute>();
            if (settings != null)
            {
                JobTypes.Add(new QuartzJobTypes()
                {
                    JobType = type,
                    JobSettings = settings
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static List<QuartzJobTypes> GetAllJobTypes() => JobTypes;

        /// <summary>
        /// 
        /// </summary>
        internal static void ClearStore() => JobTypes.Clear();
    }

    internal class QuartzJobTypes
    {
        public Type JobType { get; set; }

        public QuartzJobAttribute JobSettings { get; set; }
    }
}
