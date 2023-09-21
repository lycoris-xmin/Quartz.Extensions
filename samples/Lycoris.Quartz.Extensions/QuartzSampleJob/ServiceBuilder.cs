using Lycoris.Quartz.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace QuartzSampleJob
{
    public static class ServiceBuilder
    {
        public static void AddQuartzJobBuilder(this IServiceCollection services)
        {
            //services.AddQuartzSchedulerCenter();
            //services.AddQuartzJobAssembly(MethodBase.GetCurrentMethod().ReflectedType.Assembly).QuartzJobBuild();
            //services.AddQuartzJob(typeof(TestJob), typeof(TestJob2)).QuartzJobBuild();
            //services.AddQuartzJob<TestJob>()
            //        .AddQuartzJob<TestJob2>()
            //        .QuartzJobBuild();

            services.AddQuartzSchedulerCenter(buider =>
            {
                buider.AddJob<TestJob>();
                buider.AddJob<TestJob2>();
            });
        }
    }
}
