using Lycoris.Quartz.Extensions;

namespace QuartzSample
{
    [QuartzJob("测试任务", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 5, Standby = true)]
    public class TestJob : BaseQuartzJob
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Task DoWorkAsync()
        {
            Console.WriteLine(this.Context.GetJobName());
            return Task.CompletedTask;
        }
    }
}
