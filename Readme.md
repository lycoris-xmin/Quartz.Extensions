# Lycoris.Quartz

基于 [Quartz.NET](https://github.com/quartznet/quartznet) 的简易封装库，支持 Scoped 生命周期直接通过构造函数注入服务，大幅降低使用成本。

---

## 安装

```shell
# .NET CLI
dotnet add package Lycoris.Quartz

# Package Manager
Install-Package Lycoris.Quartz
```

## 快速开始

```csharp
using Lycoris.Quartz;

var builder = WebApplication.CreateBuilder(args);

// 注册调度中心 + 启动时自动运行非待机任务
builder.Services.AddQuartzSchedulerCenter(opt =>
    opt.EnableRunStandbyJobOnApplicationStart = true
);

// 注册一个简单的任务
builder.Services.AddQuartzSchedulerJob<HelloJob>();

var app = builder.Build();
app.Run();

// --- 任务定义 ---

[QuartzJob("Hello", Trigger = QuartzTriggerEnum.SIMPLE, IntervalSecond = 10)]
public class HelloJob : BaseQuartzJob
{
    protected override Task DoWorkAsync()
    {
        Console.WriteLine($"[{DateTime.Now}] Hello from Quartz!");
        return Task.CompletedTask;
    }
}
```

以上代码会在程序启动后，每隔 10 秒输出一次 "Hello from Quartz!"。

---

## 创建任务

支持两种方式创建调度任务。

### 方式一：继承 BaseQuartzJob（推荐）

```csharp
public class ReportJob : BaseQuartzJob
{
    private readonly IReportService _reportService;

    // 支持注入 Scoped 服务
    public ReportJob(IReportService reportService)
    {
        _reportService = reportService;
    }

    protected override async Task DoWorkAsync()
    {
        var traceId = this.JobTraceId;  // 当前执行唯一标识
        var args = this.Context.GetJobArgs<string>();  // 启动参数

        await _reportService.GenerateAsync();
    }
}
```

基类提供的可读属性：

| 属性 | 说明 |
|------|------|
| `Context` | 当前任务执行上下文 |
| `JobKey` | 任务唯一标识符 |
| `JobName` | 任务名称 |
| `JobTraceId` | 单次执行唯一 TraceId（默认 Guid） |

可重写方法：

| 方法 | 说明 |
|------|------|
| `SetJobTraceId(IJobExecutionContext)` | 自定义单次执行标识码 |
| `DoWorkAsync()` | 任务执行体 |

### 方式二：实现 IJob 接口

```csharp
public class SimpleJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("执行了一次");
        return Task.CompletedTask;
    }
}
```

此方式不包含基类提供的 `JobTraceId`、`JobName` 等属性和上下文扩展方法。

---

## 配置任务

### 通过特性配置

```csharp
[QuartzJob(
    "报表生成",                          // 任务名称（必填）
    JobGroup = "报表分组",               // 任务分组，默认 "默认分组"
    Trigger = QuartzTriggerEnum.CRON,     // 触发器类型，默认 SIMPLE
    Cron = "0 0 8 * * ?",                // Cron 表达式
    IntervalSecond = 5,                   // 间隔秒数（SIMPLE 触发时有效）
    RunTimes = 0,                         // 执行次数，0 = 无限循环
    Standby = false,                      // true = 待机，不会自动启动
    CronRunOnProceed = true              // 错过调度时是否补偿执行
)]
public class ReportJob : BaseQuartzJob
{
    protected override Task DoWorkAsync()
    {
        // ...
    }
}
```

### 通过 Fluent API 配置

```csharp
builder.Services.AddQuartzSchedulerJob<ReportJob>(opt =>
{
    opt.JobName = "报表生成";
    opt.JobGroup = "报表分组";
    opt.Trigger = QuartzTriggerEnum.CRON;
    opt.Cron = "0 0 8 * * ?";
    opt.DisallowConcurrentExecution = true;   // 禁止并发执行
    opt.Priority = 10;                        // 触发器优先级
    opt.Remark = "每天早上8点生成报表";
    opt.EndTime = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);  // 任务到期时间
});
```

不使用特性时，Fluent API 是唯一的配置方式。

---

## 注册任务

### 单个注册

```csharp
// 通过特性读取配置
builder.Services.AddQuartzSchedulerJob<ReportJob>();

// 通过 Fluent API 配置
builder.Services.AddQuartzSchedulerJob<ReportJob>(opt => { ... });

// 通过 Type 手动配置
builder.Services.AddQuartzSchedulerJob(typeof(ReportJob), opt => { ... });
```

### 批量注册

```csharp
// 按类型逐个注册
builder.Services.AddQuartzSchedulerJob(typeof(Job1), typeof(Job2), typeof(Job3));

// 程序集扫描 — 自动发现所有带 [QuartzJob] 特性的任务
builder.Services.AddQuartzSchedulerJobsFromAssembly(typeof(Program).Assembly);
```

---

## 注册调度中心

### 基础注册

```csharp
// 不会自动启动，需要手动调用 StartScheduleAsync()
builder.Services.AddQuartzSchedulerCenter();

// 注册 + 启动时自动运行所有非待机任务
builder.Services.AddQuartzSchedulerCenter(opt =>
    opt.EnableRunStandbyJobOnApplicationStart = true
);
```

### 完整配置

```csharp
builder.Services.AddQuartzSchedulerCenter(opt =>
{
    // 基础设置
    opt.EnableRunStandbyJobOnApplicationStart = true;
    opt.ThreadCount = 20;
    opt.InstanceName = "MyScheduler";

    // 数据库持久化（ADO.NET JobStore）
    opt.JobStoreType = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
    opt.TablePrefix = "QRTZ_";
    opt.DataSource = "default";
    opt.Properties["quartz.jobStore.driverDelegateType"] =
        "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz";
    opt.Properties["quartz.dataSource.default.connectionString"] =
        "Server=.;Database=Quartz;Trusted_Connection=true;";
    opt.Properties["quartz.dataSource.default.provider"] = "SqlServer";

    // 注册监听器（支持多个）
    opt.AddSchedulerListener<CustomeSchedulerListener>();
    opt.AddJobListener<CustomeJobListener>();
    opt.AddTriggerListener<CustomeTriggerListener>();
});
```

---

## 调度中心 API

通过注入 `IQuartzSchedulerCenter` 管理任务生命周期：

```csharp
// 启动 / 停止调度器
await center.StartScheduleAsync();
await center.StopScheduleAsync();

// 动态添加任务
await center.AddJobAsync<ReportJob>();
await center.AddJobAsync<ReportJob>(option);

// 单次执行任务（执行后自动删除）
await center.AddOnceJobAsync<EmailJob>();                          // 无参数
await center.AddOnceJobAsync<EmailJob, EmailArgs>(new EmailArgs()); // 强类型参数
await center.AddOnceJobAsync<EmailJob>(jsonArgs);                   // JSON 字符串参数

// 控制任务
await center.StartJobAsync("jobName", "jobGroup");    // 启动暂停中的任务
await center.StopJobAsync("jobName");                 // 暂停任务
await center.RemoveJobAsync("jobName");               // 删除任务
await center.RunJobAsync("jobName", "jobGroup");      // 立即触发执行一次

// 批量启动
await center.ManualRunNonStandbyJobsAsync();  // 启动所有非待机任务
await center.ManualRunAllJobsAsync();         // 启动所有任务（含待机）

// 查询
var allJobs = await center.GetAllJobDetailsAsync();                  // 获取所有任务
var jobDetail = await center.GetJobDetailsAsync("jobName");          // 获取单个任务详情
```

---

## 上下文扩展

Quartz 的 `IJobExecutionContext` 通过扩展方法提供了便捷的数据存取：

```csharp
protected override async Task DoWorkAsync()
{
    // 写入自定义数据（持久化，跨执行保留）
    this.Context.AddJobDataMap("key", "string value");
    this.Context.AddJobDataMap("complex", new { Name = "test" });

    // 读取自定义数据
    var value = this.Context.GetJobDataMap("key");         // "string value"
    var obj = this.Context.GetJobDataMap<MyType>("complex");

    // 获取启动参数
    var args = this.Context.GetJobArgs();                  // string
    var typedArgs = this.Context.GetJobArgs<MyArgs>();     // 反序列化

    // 获取任务元信息
    var traceId = this.Context.GetJobTraceId();            // 单次执行 TraceId
    var endTime = this.Context.GetEndTime();               // 任务到期时间
    var exception = this.Context.GetJobException();        // 上次执行异常
}
```

---

## 监听器

通过监听器可以在任务生命周期的关键节点插入自定义逻辑（如日志记录、告警通知、数据持久化）。

### 内置基类

| 基类 | 对应接口 | 说明 |
|------|----------|------|
| `SchedulerListener` | `ISchedulerListener` | 调度器生命周期事件 |
| `JobListener` | `IJobListener` | 任务执行前后事件 |
| `TriggerListener` | `ITriggerListener` | 触发器触发事件 |

### 示例

```csharp
// 自定义 Job 监听器 — 记录执行耗时
public class TimingJobListener : JobListener
{
    public override string Name => "TimingListener";

    public override async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException jobException,
        CancellationToken ct = default)
    {
        var elapsed = context.JobRunTime;
        var jobName = context.GetJobName();

        if (jobException != null)
            Console.WriteLine($"[ERROR] {jobName} 执行失败，耗时 {elapsed}ms");
        else
            Console.WriteLine($"[OK] {jobName} 执行完成，耗时 {elapsed}ms");
    }
}

// 注册（支持多个）
builder.Services.AddQuartzSchedulerCenter(opt =>
{
    opt.AddJobListener<TimingJobListener>();
    opt.AddJobListener<AnotherJobListener>();    // 第二个 JobListener 也会被调用
});
```

也可以直接实现 `ISchedulerListener` / `IJobListener` / `ITriggerListener` 原生接口，无需继承基类。

---

## 数据库持久化

```csharp
builder.Services.AddQuartzSchedulerCenter(opt =>
{
    // SQL Server
    opt.JobStoreType = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
    opt.TablePrefix = "QRTZ_";
    opt.DataSource = "default";
    opt.Properties["quartz.jobStore.driverDelegateType"] =
        "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz";
    opt.Properties["quartz.dataSource.default.provider"] = "SqlServer";
    opt.Properties["quartz.dataSource.default.connectionString"] =
        "Server=.;Database=Quartz;Trusted_Connection=true;";
});
```

MySQL / PostgreSQL / Oracle 改对应的 `driverDelegateType` 和 `provider` 即可。

建表脚本：[Quartz.NET Database Scripts](https://github.com/quartznet/quartznet/tree/main/database/tables)

---

## 配置参数速查

### QuartzJobAttribute

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `JobName` | string | 必填 | 任务名称 |
| `JobGroup` | string | `"默认分组"` | 任务分组 |
| `Trigger` | enum | `SIMPLE` | 触发器类型（`SIMPLE` / `CRON`） |
| `IntervalSecond` | int | `1` | 间隔秒数，SIMPLE 触发时有效 |
| `RunTimes` | int | `0` | 执行次数，`0` = 无限循环 |
| `Cron` | string | `""` | Cron 表达式，CRON 触发时有效 |
| `Standby` | bool | `false` | 待机任务，需手动触发 |
| `CronRunOnProceed` | bool | `false` | 错过调度时是否补偿执行 |

### QuartzSchedulerOption（Fluent API）

除上述所有参数外，额外支持：

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `BeginTime` | DateTimeOffset | `DateTime.Now` | 任务开始时间 |
| `EndTime` | DateTimeOffset? | null | 任务到期时间 |
| `DisallowConcurrentExecution` | bool | `false` | 禁止并发执行 |
| `Priority` | int | `5` | 触发器优先级 |
| `Remark` | string | `""` | 任务备注 |
| `Args` | string | null | 启动参数 |

### QuartzBuilder（调度中心配置）

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ThreadCount` | int | `10` | 线程池大小 |
| `InstanceName` | string | `"QuartzScheduler"` | 调度器实例名 |
| `EnableRunStandbyJobOnApplicationStart` | bool | `false` | 启动时自动运行非待机任务 |
| `JobStoreType` | string | null | JobStore 全限定类型名 |
| `TablePrefix` | string | `"QRTZ_"` | 数据表前缀 |
| `DataSource` | string | null | 数据源名称 |
| `Properties` | NameValueCollection | 空 | 自定义 Quartz 原生属性 |

---

## 注意事项

- **`BaseQuartzJob` 默认带有 `[PersistJobDataAfterExecution]`**，每次执行后 JobDataMap 会持久化传给下一次，无需手动设置。
- **单次任务**（`AddOnceJobAsync`）执行后自动从调度器删除，适合发邮件、推送通知等一次性操作。
- **待机任务**（`Standby = true`）不会随调度中心启动而执行，适用于通过 API 动态触发的场景。
- **任务名 + 分组必须唯一**，同一组内不允许重名。
- **`DisallowConcurrentExecution`** 可通过特性或 Fluent API 设置，效果相同。
- **`CronRunOnProceed`**：`true` = 错过调度后补偿执行；`false` = 跳过，等待下一次。
- **程序停止时调度任务不会自动迁移**：不使用数据库持久化时，所有调度状态仅存于内存。
- **API 兼容**：`RemoveobAsync`（拼写错误）已标记 `[Obsolete]`，请使用 `RemoveJobAsync`。`QuartzExtention` 已重命名为 `QuartzExtension`。
- **目标框架** `netstandard2.0`，兼容 .NET Framework 4.6.1+ 和 .NET Core 2.0+。

---

## 完整示例

```csharp
using Lycoris.Quartz;

var builder = WebApplication.CreateBuilder(args);

// ====== 1. 注册调度中心 ======
builder.Services.AddQuartzSchedulerCenter(opt =>
{
    opt.EnableRunStandbyJobOnApplicationStart = true;
    opt.ThreadCount = 10;
    opt.InstanceName = "AppScheduler";

    // 可选：数据库持久化
    // opt.JobStoreType = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
    // opt.TablePrefix = "QRTZ_";
    // opt.DataSource = "default";

    // 注册监听器
    // opt.AddJobListener<MyJobListener>();
});

// ====== 2. 注册任务 ======

// A. 程序集扫描批量注册
builder.Services.AddQuartzSchedulerJobsFromAssembly(typeof(Program).Assembly);

// B. 手动注册（覆盖特性配置）
builder.Services.AddQuartzSchedulerJob<DailyReportJob>(opt =>
{
    opt.Cron = "0 0 8 * * ?";
    opt.DisallowConcurrentExecution = true;
    opt.Remark = "每日8点生成报表";
});

var app = builder.Build();

// ====== 3. API 管理 ======

// 查看所有任务
app.MapGet("/api/jobs", async (IQuartzSchedulerCenter center) =>
    Results.Ok(await center.GetAllJobDetailsAsync()));

// 立即触发任务
app.MapPost("/api/jobs/{name}/trigger", async (string name, IQuartzSchedulerCenter center) =>
{
    await center.RunJobAsync(name, "默认分组");
    return Results.NoContent();
});

// 暂停任务
app.MapPost("/api/jobs/{name}/pause", async (string name, IQuartzSchedulerCenter center) =>
{
    await center.StopJobAsync(name);
    return Results.NoContent();
});

// 恢复任务
app.MapPost("/api/jobs/{name}/resume", async (string name, IQuartzSchedulerCenter center) =>
{
    await center.StartJobAsync(name);
    return Results.NoContent();
});

// 单次发送邮件
app.MapPost("/api/send-email", async (SendRequest req, IQuartzSchedulerCenter center) =>
{
    await center.AddOnceJobAsync<SendEmailJob, SendRequest>(req);
    return Results.Accepted();
});

app.Run();

// ====== 任务定义 ======

[QuartzJob("每日报表", Cron = "0 0 8 * * ?", Trigger = QuartzTriggerEnum.CRON)]
public class DailyReportJob : BaseQuartzJob
{
    private readonly IReportService _reportService;

    public DailyReportJob(IReportService reportService)
    {
        _reportService = reportService;
    }

    protected override async Task DoWorkAsync()
    {
        var traceId = this.JobTraceId;
        await _reportService.GenerateDailyReportAsync();
    }
}

[QuartzJob("发送邮件", Standby = true)]
public class SendEmailJob : BaseQuartzJob
{
    protected override async Task DoWorkAsync()
    {
        var args = this.Context.GetJobArgs<SendRequest>();
        Console.WriteLine($"发送邮件至: {args.To}, 主题: {args.Subject}");
    }
}

public record SendRequest(string To, string Subject, string Body);
```
