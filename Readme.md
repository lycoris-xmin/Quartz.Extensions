
**基于`Quartz`做了一层简单封装，支持Scoped生命周期直接在构造函数注入,简化了使用成本。**

### 安装方式
```shell
// net cli
dotnet add package Lycoris.Quartz
// package manager
Install-Package Lycoris.Quartz
```

### **一、注册Quartz调度中心**

#### 注册方式一
```csharp
// 默认注册扩展
builder.Services.AddQuartzSchedulerCenter();

// 启用程序启动时自动启动非待机定时任务
builder.Services.AddQuartzSchedulerCenter(opt => opt.EnableRunStandbyJobOnApplicationStart = true);
```

**未设置 `EnableRunStandbyJobOnApplicationStart` 为 `true` 的情况下需要手动启动调度中心及调度任务**
**以下以最小化api的方式作为示例**
```csharp
app.MapGet("/weatherforecast", async ([FromServices] IQuartzSchedulerCenter center) =>
{
    // 启动调度中心
    await center.StartScheduleAsync();

    // 启动非待机定时任务
    await center.ManualRunNonStandbyJobsAsync();

    // 启动所有任务
    //await center.ManualRunAllJobsAsync();

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});
```

### **二、创建调度任务**

**创建调度任务的两种方式**


**基类会根据设置的任务运行截至时间自动停止任务**

#### **1. 继承扩展的基类 `BaseQuartzJob`**

#### 基类中包含以下三个可读属性
- **`Context`：当前执行的任务上下文**
- **`JobTraceId`：当前执行的任务唯一表示Id**
- **`JobName`：当前执行的任务名称**

#### 基类中包含以下可重写方法
- **`public virtual string SetJobTraceId(IJobExecutionContext JobContext)`：用于设置当前回合执行任务唯一标识码，基类默认为`Guid`，可自行修改**

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

    private void Test()
    {
        // 获取记录的值
        var val = this.Context.GetJobDataMap("map");
    }
}
```


#### **2. 继承来自`Quartz`的`IJob`接口**
**继承来自`Quartz`的`IJob`接口的实现方式，但不包含上述基类提及的所有属性及方法**

```csharp
public class TestJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.CompletedTask;
    }
}
```

### **三、配置调度任务**

#### **使用特性进行配置**

**在创建好的任务类上加上特性 `QuartzJobAttribute("JobName")` 或 `QuartzJobAttribute("JobName", "JobGroup")`**

**`QuartzJobAttribute`其他属性如下所示：**

- **`Trigger`为触发器类型,分为普通定时器 `SIMPLE` 和 Cron定时器 `CRON`；默认：`SIMPLE`**
- **`IntervalSecond`：定时秒数，该配置仅对普通定时器有效；默认：1**
- **`Cron`：如果是Corn定时器，需要配置Cron表达式**
- **`RunTimes`：执行次数，默认：0 (无限循环)**
- **`Standby`：待机任务，默认：`false` (待机任务指程序运行时不会启动的任务，需要手动执行启动，详见手动启动任务)**

```csharp
// 不设定任务分组的配置
[QuartzJob("job", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 5)]
public class TestJob : BaseQuartzJob
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override Task DoWorkAsync()
    {
        // do something
        return Task.CompletedTask;
    }
}

// 设定任务分组的配置
[QuartzJob("job","jobGroup", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 5)]
public class TestJob : BaseQuartzJob
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override Task DoWorkAsync()
    {
        // do something
        return Task.CompletedTask;
    }
}
```

#### **手动配置注册的方式详见 `四、注册调度任务` 的注册介绍**

### **四、注册调度任务**

**使用`QuartzJobAttribute`进行配置的调度任务注册方式**
```csharp
builder.Services.AddQuartzSchedulerJob<TestJob>();
```

**未使用`QuartzJobAttribute`进行配置的调度任务注册方式**
```csharp
builder.Services.AddQuartzSchedulerJob<TestJob>(opt =>
{
    opt.JobName = "测试任务";
    opt.Trigger = QuartzTriggerEnum.SIMPLE;
    opt.IntervalSecond = 10;
    opt.Standby = true;
});
```

### **五、调度任务特性(quartz自带的特性)**
- **`PersistJobDataAfterExecutionAttribute`: 这一次的结果作为值传给下一次定时任务**
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


### **六、`IQuartzSchedulerCenter` 调度中心方法**

```csharp
public interface IQuartzSchedulerCenter
{
    /// <summary>
    /// 开启调度器
    /// </summary>
    /// <returns></returns>
    Task<bool> StartScheduleAsync();

    /// <summary>
    /// 停止调度器
    /// </summary>
    Task<bool> StopScheduleAsync();

    /// <summary>
    /// 添加工作任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task AddJobAsync<T>() where T : IJob;

    /// <summary>
    /// 添加工作任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sche"></param>
    /// <returns></returns>
    Task AddJobAsync<T>(QuartzSchedulerOption sche) where T : IJob;

    /// <summary>
    /// 添加工作任务
    /// </summary>
    /// <param name="sche"></param>
    /// <returns></returns>
    Task AddJobAsync(QuartzSchedulerOption sche);

    /// <summary>
    /// 添加单次执行任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task AddOnceJobAsync<T>() where T : IJob;

    /// <summary>
    /// 添加单次执行任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TArgs"></typeparam>
    /// <param name="args">任务启动参数</param>
    /// <returns></returns>
    Task AddOnceJobAsync<T, TArgs>(TArgs args) where T : IJob where TArgs : class;

    /// <summary>
    /// 添加单次执行任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args">任务启动参数</param>
    /// <returns></returns>
    Task AddOnceJobAsync<T>(string args) where T : IJob;

    /// <summary>
    /// 启动任务
    /// </summary>
    /// <param name="jobKey"></param>
    /// <param name="jobGroup"></param>
    /// <returns></returns>
    Task StartJobAsync(string jobKey, string jobGroup = "");

    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <param name="jobKey"></param>
    /// <param name="jobGroup"></param>
    /// <returns></returns>
    Task StopJobAsync(string jobKey, string jobGroup = "");

    /// <summary>
    /// 移除任务
    /// </summary>
    /// <param name="jobKey"></param>
    /// <param name="jobGroup"></param>
    /// <returns></returns>
    Task RemoveobAsync(string jobKey, string jobGroup = "");

    /// <summary>
    /// 立即执行一次
    /// </summary>
    /// <param name="jobKey"></param>
    /// <param name="jobGroup"></param>
    /// <returns></returns>
    Task RunJobAsync(string jobKey, string jobGroup);

    /// <summary>
    /// 手动启动所有非待机任务
    /// </summary>
    /// <returns></returns>
    Task ManualRunNonStandbyJobsAsync();

    /// <summary>
    /// 手动启动所有任务
    /// </summary>
    /// <returns></returns>
    Task ManualRunAllJobsAsync();
}
```

### **七、调度任务上下文扩展**
```csharp
public class TestJob : BaseQuartzJob
{
    protected override Task DoWorkAsync()
    {
        // 记录自定义值 （记录的自定义值会一直存在）
        this.Context.AddJobDataMap("key", "something");
        this.Context.AddJobDataMap("key1", new string[]{ "something" });

        // 获取自定义值
        var value = this.Context.GetJobDataMap("key");
        var value1 = this.Context.GetJobDataMap<string[]>("key1");

        // 获取任务的结束时间
        var endTime = this.Context.GetEndTime();

        // 获取任务启动参数
        var args = this.Context.GetJobArgs();
        var agrs1 = this.Context.GetJobArgs<string[]>();

        return Task.CompletedTask;
    }
}
```

### **八、数据库支持**
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
 builder.Services.AddQuartzSchedulerCenter(opt =>
 {
     opt.AddSchedulerListener<CustomeSchedulerListener>();
     opt.AddTriggerListener<CustomeTriggerListener>();
     opt.AddJobListener<CustomeJobListener>();
 });
```