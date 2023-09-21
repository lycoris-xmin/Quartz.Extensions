using Lycoris.Quartz.Extensions.Job;
using Quartz;

namespace QuartzSample
{
    [QuartzJob("测试任务", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 5)]
    public class TestJob : BaseQuartzJob
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Task DoWorkAsync()
        {
            Console.WriteLine("123");
            return Task.CompletedTask;
        }
    }
}
