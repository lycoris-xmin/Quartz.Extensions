
**基于`Quartz`做了一层简单封装，支持Scoped生命周期直接在构造函数注入,简化了使用成本。**

### 安装方式
```shell
// net cli
dotnet add package Lycoris.Quartz.Extensions
// package manager
Install-Package Lycoris.Quartz.Extensions
```

### **一、创建调度任务**

**创建调度任务的两种方式**


**基类会根据设置的任务运行截至事件自动停止任务，并且基类中也做了异常捕捉，除了你业务中必要的业务捕捉要，其他未知异常基类都能帮你及时记录**

**1. 继承扩展的基类 `BaseQuartzJob`**

#### 基类中包含以下三个可读属性
- **`Context`：当前执行的任务上下文**
- **`JobTraceId`：当前执行的任务唯一表示Id**
- **`JobName`：当前执行的任务名称**

```csharp
public class TestJob : BaseQuartzJob
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override Task DoWorkAsync()
    {
        // 记录某些值
        this.Context.AddJobDataMap("map", "something");
        return Task.CompletedTask;
    }
}
```

**创建好调度任务后，还需要设置任务的配置，设置很简单，在任务类上加上特性`[QuartzJob("测试任务", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 5)]`**

**`QuartzJobAttribute`使用指南:**
- **构造函数入参为定时任务名称**
- **`Trigger`为触发器类型,分为普通定时器 `SIMPLE` 和 Cron定时器 `CRON`**
- **`IntervalSecond`：定时秒数，该配置仅对普通定时器有效**
- **`Cron`：如果是Corn定时器，需要配置Cron表达式**
- **`RunTimes`：执行次数，默认是无限循环**

```csharp
[QuartzJob("测试任务", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 5)]
public class TestJob : BaseQuartzJob
```

---

- **2. 继承来自`Quartz`的`IJob`接口**
```csharp
public class TestJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.CompletedTask;
    }
}
```


### **二、注册Quartz调度中心**

#### 注册方式一
```csharp
// 注册扩展
builder.Services.AddQuartzSchedulerCenter();

// 调度任务注册方式二选一
// 注册调度任务 方式一
services.AddQuartzJob<TestJob>().AddQuartzJob<TestJob2>().QuartzJobBuild();
// 注册调度任务 方式二
services.AddQuartzJob(typeof(TestJob), typeof(TestJob2)).QuartzJobBuild();
```

#### 注册方式二
```csharp
 builder.Services.AddQuartzSchedulerCenter(buider =>
 {
     // 线程池个数（默认为：10个）
     builder.ThreadCount = 10;
     // 禁用自动执行待机列表任务
     // 禁用后，需要手动启动待机列表任务请执行 IQuartzSchedulerCenter.ManualRunHostedJobsAsync
     builder.DisabledRunHostedJob();
     buider.AddJob<TestJob>();
 });
```

### **三、调度任务特性(quartz自带的特性)**
- **`PersistJobDataAfterExecutionAttribute`: 这一次的结果作为值传给下一次**
```csharp
[PersistJobDataAfterExecution]
public class TestJob : BaseQuartzJob
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override Task DoWork(IJobExecutionContext context)
    {
        // 取出的是上一次 AddJobDataMap 保存的值
        var val = context.GetJobDataMap("Key");

        // todo

        // 保存这一次的值
        context.AddJobDataMap("Key", "这一次的传值");

        return Task.CompletedTask;
    }
}
```

- **`DisallowConcurrentExecutionAttribute`: 只有上一个任务完成才会执行下一次任务**
  ```csharp
  [DisallowConcurrentExecution]
  public class TestJob : BaseQuartzJob
  ```


### **四、数据库支持**
**扩展不做数据库支持，但是Quartz原生带有各种监听服务，需要使用到数据库做数据持久化的，请自行开发，可实现的接口如下：**
- **`ISchedulerListener`:调度器执行监听**
- **`ITriggerListener`:调度器执行监听**
- **`IJobListener`:调度任务执行监听**

**由于原生的接口有些有很多方法需要实现，如果想偷懒的小伙伴可以继承我处理好的基类：**
- **`SchedulerListener`**
- **`TriggerListener`**
- **`JobListener`**

**仅需重写你需要使用到的监听方法即可**

**注册监听**
```csharp
 builder.Services.AddQuartzSchedulerCenter(buider =>
 {
     buider.AddSchedulerListener<CustomeSchedulerListener>();
     buider.AddTriggerListener<CustomeTriggerListener>();
     buider.AddJobListener<CustomeJobListener>();
 });
```