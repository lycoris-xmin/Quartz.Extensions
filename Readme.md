# Lycoris.Quartz

[![NuGet](https://img.shields.io/nuget/v/Lycoris.Quartz)](https://www.nuget.org/packages/Lycoris.Quartz)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Lycoris.Quartz)](https://www.nuget.org/packages/Lycoris.Quartz)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-netstandard2.0-blue.svg)](https://dotnet.microsoft.com/)

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

> 默认使用内存 RAM JobStore，无需额外配置。仅在需要持久化调度状态（任务跨重启保留、集群部署等）时才启用数据库 JobStore。

```csharp
builder.Services.AddQuartzSchedulerCenter(opt =>
{
    // 启用数据库持久化（设置此属性后，TablePrefix 和 DataSource 才会生效）
    opt.JobStoreType = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";

    // SQL Server
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
| `JobStoreType` | string | null | JobStore 全限定类型名，不设置则使用内存 RAM JobStore |
| `TablePrefix` | string | `"QRTZ_"` | 数据表前缀，仅在 `JobStoreType` 不为空时生效 |
| `DataSource` | string | null | 数据源名称，仅在 `JobStoreType` 不为空时生效 |
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
- **数据库持久化默认关闭**：只有显式设置 `opt.JobStoreType` 后，`TablePrefix` 和 `DataSource` 等相关配置才会写入 Quartz，否则使用内存 RAM JobStore。
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

---

## 依赖项

本项目依赖 [Quartz.NET](https://github.com/quartznet/quartznet)（[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)），以及以下 NuGet 包：

| 包名 | 许可证 |
|------|--------|
| [Quartz](https://www.nuget.org/packages/Quartz) | [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0) |
| [Microsoft.Extensions.Hosting.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.Abstractions) | [MIT](https://opensource.org/licenses/MIT) |
| [Microsoft.Extensions.Options](https://www.nuget.org/packages/Microsoft.Extensions.Options) | [MIT](https://opensource.org/licenses/MIT) |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) | [MIT](https://opensource.org/licenses/MIT) |

---

## 开源协议

本项目基于 [MIT 许可证](LICENSE) 发布。

```
MIT License

Copyright (c) 2023-2026 lycoris

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### Quartz.NET 许可证声明

本项目封装了 [Quartz.NET](https://github.com/quartznet/quartznet)，Quartz.NET 在 [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0) 下发布。

```
Apache License
Version 2.0, January 2004
http://www.apache.org/licenses/

TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION

1. Definitions.

"License" shall mean the terms and conditions for use, reproduction,
and distribution as defined by Sections 1 through 9 of this document.

"Licensor" shall mean the copyright owner or entity authorized by
the copyright owner that is granting the License.

"Legal Entity" shall mean the union of the acting entity and all
other entities that control, are controlled by, or are under common
control with that entity. For the purposes of this definition,
"control" means (i) the power, direct or indirect, to cause the
direction or management of such entity, whether by contract or
otherwise, or (ii) ownership of fifty percent (50%) or more of the
outstanding shares, or (iii) beneficial ownership of such entity.

"You" (or "Your") shall mean an individual or Legal Entity
exercising permissions granted by this License.

"Source" form shall mean the preferred form for making modifications,
including but not limited to software source code, documentation
source, and configuration files.

"Object" form shall mean any form resulting from mechanical
transformation or translation of a Source form, including but
not limited to compiled object code, generated documentation,
and conversions to other media types.

"Work" shall mean the work of authorship, whether in Source or
Object form, made available under the License, as indicated by a
copyright notice that is included in or attached to the work
(an example is provided in the Appendix below).

"Derivative Works" shall mean any work, whether in Source or Object
form, that is based on (or derived from) the Work and for which the
editorial revisions, annotations, elaborations, or other modifications
represent, as a whole, an original work of authorship. For the purposes
of this License, Derivative Works shall not include works that remain
separable from, or merely link (or bind by name) to the interfaces of,
the Work and Derivative Works thereof.

"Contribution" shall mean any work of authorship, including
the original version of the Work and any modifications or additions
to that Work or Derivative Works thereof, that is intentionally
submitted to Licensor for inclusion in the Work by the copyright owner
or by an individual or Legal Entity authorized to submit on behalf of
the copyright owner. For the purposes of this definition, "submitted"
means any form of electronic, verbal, or written communication sent
to the Licensor or its representatives, including but not limited to
communication on electronic mailing lists, source code control systems,
and issue tracking systems that are managed by, or on behalf of, the
Licensor for the purpose of discussing and improving the Work, but
excluding communication that is conspicuously marked or otherwise
designated in writing by the copyright owner as "Not a Contribution."

"Contributor" shall mean Licensor and any individual or Legal Entity
on behalf of whom a Contribution has been received by Licensor and
subsequently incorporated within the Work.

2. Grant of Copyright License. Subject to the terms and conditions of
this License, each Contributor hereby grants to You a perpetual,
worldwide, non-exclusive, no-charge, royalty-free, irrevocable
copyright license to reproduce, prepare Derivative Works of,
publicly display, publicly perform, sublicense, and distribute the
Work and such Derivative Works in Source or Object form.

3. Grant of Patent License. Subject to the terms and conditions of
this License, each Contributor hereby grants to You a perpetual,
worldwide, non-exclusive, no-charge, royalty-free, irrevocable
(except as stated in this section) patent license to make, have made,
use, offer to sell, sell, import, and otherwise transfer the Work,
where such license applies only to those patent claims licensable
by such Contributor that are necessarily infringed by their
Contribution(s) alone or by combination of their Contribution(s)
with the Work to which such Contribution(s) was submitted. If You
institute patent litigation against any entity (including a
cross-claim or counterclaim in a lawsuit) alleging that the Work
or a Contribution incorporated within the Work constitutes direct
or contributory patent infringement, then any patent licenses
granted to You under this License for that Work shall terminate
as of the date such litigation is filed.

4. Redistribution. You may reproduce and distribute copies of the
Work or Derivative Works thereof in any medium, with or without
modifications, and in Source or Object form, provided that You
meet the following conditions:

(a) You must give any other recipients of the Work or
Derivative Works a copy of this License; and

(b) You must cause any modified files to carry prominent notices
stating that You changed the files; and

(c) You must retain, in the Source form of any Derivative Works
that You distribute, all copyright, patent, trademark, and
attribution notices from the Source form of the Work,
excluding those notices that do not pertain to any part of
the Derivative Works; and

(d) If the Work includes a "NOTICE" text file as part of its
distribution, then any Derivative Works that You distribute must
include a readable copy of the attribution notices contained
within such NOTICE file, excluding those notices that do not
pertain to any part of the Derivative Works, in at least one
of the following places: within a NOTICE text file distributed
as part of the Derivative Works; within the Source form or
documentation, if provided along with the Derivative Works; or,
within a display generated by the Derivative Works, if and
wherever such third-party notices normally appear. The contents
of the NOTICE file are for informational purposes only and
do not modify the License. You may add Your own attribution
notices within Derivative Works that You distribute, alongside
or as an addendum to the NOTICE text from the Work, provided
that such additional attribution notices cannot be construed
as modifying the License.

You may add Your own copyright statement to Your modifications and
may provide additional or different license terms and conditions
for use, reproduction, or distribution of Your modifications, or
for any such Derivative Works as a whole, provided Your use,
reproduction, and distribution of the Work otherwise complies with
the conditions stated in this License.

5. Submission of Contributions. Unless You explicitly state otherwise,
any Contribution intentionally submitted for inclusion in the Work
by You to the Licensor shall be under the terms and conditions of
this License, without any additional terms or conditions.
Notwithstanding the above, nothing herein shall supersede or modify
the terms of any separate license agreement you may have executed
with Licensor regarding such Contributions.

6. Trademarks. This License does not grant permission to use the trade
names, trademarks, service marks, or product names of the Licensor,
except as required for reasonable and customary use in describing the
origin of the Work and reproducing the content of the NOTICE file.

7. Disclaimer of Warranty. Unless required by applicable law or
agreed to in writing, Licensor provides the Work (and each
Contributor provides its Contributions) on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied, including, without limitation, any warranties or conditions
of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A
PARTICULAR PURPOSE. You are solely responsible for determining the
appropriateness of using or redistributing the Work and assume any
risks associated with Your exercise of permissions under this License.

8. Limitation of Liability. In no event and under no legal theory,
whether in tort (including negligence), contract, or otherwise,
unless required by applicable law (such as deliberate and grossly
negligent acts) or agreed to in writing, shall any Contributor be
liable to You for damages, including any direct, indirect, special,
incidental, or consequential damages of any character arising as a
result of this License or out of the use or inability to use the
Work (including but not limited to damages for loss of goodwill,
work stoppage, computer failure or malfunction, or any and all
other commercial damages or losses), even if such Contributor
has been advised of the possibility of such damages.

9. Accepting Warranty or Additional Liability. While redistributing
the Work or Derivative Works thereof, You may choose to offer,
and charge a fee for, acceptance of support, warranty, indemnity,
or other liability obligations and/or rights consistent with this
License. However, in accepting such obligations, You may act only
on Your own behalf and on Your sole responsibility, not on behalf
of any other Contributor, and only if You agree to indemnify,
defend, and hold each Contributor harmless for any liability
incurred by, or claims asserted against, such Contributor by reason
of your accepting any such warranty or additional liability.

END OF TERMS AND CONDITIONS

APPENDIX: How to apply the Apache License to your work.

To apply the Apache License to your work, attach the following
boilerplate notice, with the fields enclosed by brackets "[]"
replaced with your own identifying information. (Don't include
the brackets!)  The text should be enclosed in the appropriate
comment syntax for the file format. We also recommend that a
file or class name and description of purpose be included on the
same "printed page" as the copyright notice for easier
identification within third-party archives.

Copyright [yyyy] [name of copyright owner]

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```

Quartz.NET 的原始版权归属于 [Quartz.NET 项目](https://github.com/quartznet/quartznet) 及其贡献者。本封装库（Lycoris.Quartz）为独立项目，与 Quartz.NET 官方无关联。
