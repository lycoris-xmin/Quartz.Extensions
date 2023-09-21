using Quartz;

namespace QuartzSample
{
    public class TestJob3 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
