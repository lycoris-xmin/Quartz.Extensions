using Lycoris.Quartz;

namespace QuartzSample
{
    [QuartzJob("测试任务3", Trigger = QuartzTriggerEnum.SIMPLE, Standby = true)]
    public class TestJob3 : BaseQuartzJob
    {
        protected override Task DoWorkAsync()
        {
            Console.WriteLine(this.Context.GetJobName() + this.Context.GetJobArgs());
            return Task.CompletedTask;
        }
    }
}
