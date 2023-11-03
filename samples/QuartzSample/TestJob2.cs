using Lycoris.Quartz.Extensions;

namespace QuartzSample
{
    [QuartzJob("测试任务2", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 10)]
    public class TestJob2 : BaseQuartzJob
    {
        public TestJob2()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Task DoWorkAsync()
        {
            Console.WriteLine(this.Context.GetJobName());
            return Task.CompletedTask;
        }
    }
}
