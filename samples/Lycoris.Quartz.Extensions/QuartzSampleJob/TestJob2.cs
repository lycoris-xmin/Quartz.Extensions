using Lycoris.Quartz.Extensions.Job;
using Quartz;

namespace QuartzSampleJob
{
    [QuartzJob("测试任务1", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 10)]
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
            return Task.CompletedTask;
        }
    }
}
