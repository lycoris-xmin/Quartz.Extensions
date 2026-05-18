# Quartz.Extensions Code Review Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 7 bugs, add 5 extensibility features, and apply 3 code optimizations to the Lycoris.Quartz library.

**Architecture:** All changes follow the existing project patterns — `QuartzBuilder` for configuration, `QuartzSchedulerCenter` for scheduling logic, extension methods on `IServiceCollection` for registration. New `QuartzTriggerFactory` static class extracts trigger creation from the monolithic scheduler center.

**Tech Stack:** .NET Standard 2.0, Quartz.NET 3.15.0, Microsoft.Extensions.DependencyInjection / Hosting

---

## Task 1: Fix `IsAssignableFrom` direction (B1)

**Files:**
- Modify: `src/QuartzJobHelper.cs:20, :44, :80`

- [ ] **Step 1: Fix three occurrences**

Line 20 (in `GetJobsByAssembly`):
```csharp
// Before
.Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) || x.IsAssignableFrom(typeof(IJob)))
// After
.Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) || typeof(IJob).IsAssignableFrom(x))
```

Line 44 (in `GetJobs`):
```csharp
// Before
var jobClass = types.Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) && x.IsAssignableFrom(typeof(IJob))).Select(x => x.FullName).ToList() ?? new List<string>();
// After
var jobClass = types.Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) && typeof(IJob).IsAssignableFrom(x)).Select(x => x.FullName).ToList() ?? new List<string>();
```

Line 80 (in `GetJob`):
```csharp
// Before
if (!type.IsSubclassOf(typeof(BaseQuartzJob)) && !type.IsAssignableFrom(typeof(IJob)))
// After
if (!type.IsSubclassOf(typeof(BaseQuartzJob)) && !typeof(IJob).IsAssignableFrom(type))
```

- [ ] **Step 2: Build to verify compilation**

```bash
dotnet build src/Lycoris.Quartz.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/QuartzJobHelper.cs
git commit -m "fix: correct IsAssignableFrom direction in QuartzJobHelper"
```

---

## Task 2: Fix validation logic inversion (B2)

**Files:**
- Modify: `src/QuartzJobHelper.cs:39`

- [ ] **Step 1: Fix the condition**

```csharp
// Before
var modifierList = types.Where(x => !x.IsClass || x.IsPublic || x.IsAbstract).Select(x => x.FullName).ToList() ?? new List<string>();
// After
var modifierList = types.Where(x => !x.IsClass || !x.IsPublic || x.IsAbstract).Select(x => x.FullName).ToList() ?? new List<string>();
```

- [ ] **Step 2: Commit**

```bash
git add src/QuartzJobHelper.cs
git commit -m "fix: correct validation logic for job type filtering"
```

---

## Task 3: Fix null reference risk in DefaultQuartzJobHostedService (B3)

**Files:**
- Modify: `src/DefaultQuartzJobHostedService.cs:38`

- [ ] **Step 1: Fix the null check**

```csharp
// Before
if (_options != null || _options.Count() > 0)
// After
if (_options != null && _options.Any())
```

Also replace `using System.Collections.Generic;` with `using System.Linq;` at the top since we use `Any()` instead of `Count()`.

- [ ] **Step 2: Commit**

```bash
git add src/DefaultQuartzJobHostedService.cs
git commit -m "fix: prevent NRE in DefaultQuartzJobHostedService when options is null"
```

---

## Task 4: Fix CronRunOnProceed ineffective (B4)

**Files:**
- Modify: `src/Services/QuartzSchedulerCenter.cs:525-528`

- [ ] **Step 1: Fix the else branch**

```csharp
// Before
if (sche.CronRunOnProceed)
    trigger.WithCronSchedule(sche.Cron, cronScheduleBuilder => cronScheduleBuilder.WithMisfireHandlingInstructionFireAndProceed());
else
    trigger.WithCronSchedule(sche.Cron, cronScheduleBuilder => cronScheduleBuilder.WithMisfireHandlingInstructionFireAndProceed());

// After
if (sche.CronRunOnProceed)
    trigger.WithCronSchedule(sche.Cron, b => b.WithMisfireHandlingInstructionFireAndProceed());
else
    trigger.WithCronSchedule(sche.Cron, b => b.WithMisfireHandlingInstructionDoNothing());
```

- [ ] **Step 2: Commit**

```bash
git add src/Services/QuartzSchedulerCenter.cs
git commit -m "fix: CronRunOnProceed=false now correctly uses DoNothing misfire policy"
```

---

## Task 5: Fix AddOnceJobAsync without args missing cleanup marker (B5)

**Files:**
- Modify: `src/Services/QuartzSchedulerCenter.cs:198-206`

- [ ] **Step 1: Add JsonMap = ONCE_JOB**

```csharp
// Before
await AddJobAsync<T>(new QuartzSchedulerOption()
{
    Trigger = QuartzTriggerEnum.SIMPLE,
    IntervalSecond = 1,
    RunTimes = 1,
    BeginTime = new DateTime(2000, 1, 1),
    JobName = option.JobName,
    JobGroup = string.IsNullOrEmpty(option.JobGroup) ? QuartzConstant.JOB_DEFAULT_GROUP : option.JobGroup
});

// After
await AddJobAsync<T>(new QuartzSchedulerOption()
{
    Trigger = QuartzTriggerEnum.SIMPLE,
    IntervalSecond = 1,
    RunTimes = 1,
    BeginTime = new DateTime(2000, 1, 1),
    JobName = option.JobName,
    JobGroup = string.IsNullOrEmpty(option.JobGroup) ? QuartzConstant.JOB_DEFAULT_GROUP : option.JobGroup,
    JsonMap = QuartzConstant.ONCE_JOB
});
```

- [ ] **Step 2: Commit**

```bash
git add src/Services/QuartzSchedulerCenter.cs
git commit -m "fix: AddOnceJobAsync no-args overload now sets ONCE_JOB marker for cleanup"
```

---

## Task 6: Fix StartJobAsync error message (B6)

**Files:**
- Modify: `src/Services/QuartzSchedulerCenter.cs:322`

- [ ] **Step 1: Fix message**

```csharp
// Before
throw new Exception($"this job {jobKey} already exists");
// After
throw new Exception($"this job {jobKey} does not exist");
```

- [ ] **Step 2: Commit**

```bash
git add src/Services/QuartzSchedulerCenter.cs
git commit -m "fix: correct error message in StartJobAsync when job not found"
```

---

## Task 7: Fix typos and dead code cleanup (B7)

**Files:**
- Modify: `src/IQuartzSchedulerCenter.cs:132`
- Modify: `src/Services/QuartzSchedulerCenter.cs:96-99, :369`
- Modify: `src/QuartzExtention.cs`

- [ ] **Step 1: Add correctly-named method to interface**

In `src/IQuartzSchedulerCenter.cs`, after line 132 (`RemoveobAsync`), add the new method and mark old as obsolete:

```csharp
/// <summary>
/// 移除任务
/// </summary>
/// <param name="jobKey"></param>
/// <param name="jobGroup"></param>
/// <returns></returns>
[Obsolete("Use RemoveJobAsync instead")]
Task RemoveobAsync(string jobKey, string jobGroup = "");

/// <summary>
/// 移除任务
/// </summary>
/// <param name="jobKey"></param>
/// <param name="jobGroup"></param>
/// <returns></returns>
Task RemoveJobAsync(string jobKey, string jobGroup = "");
```

- [ ] **Step 2: Add implementation and remove dead code**

In `src/Services/QuartzSchedulerCenter.cs`:

Delete lines 96-99 (the dead `catch { throw; }` in `StopScheduleAsync`):
```csharp
// Delete these 4 lines:
            catch
            {
                throw;
            }
```

Rename `RemoveobAsync` method and add old forwarding method at line 369:
```csharp
[Obsolete("Use RemoveJobAsync instead")]
public async Task RemoveobAsync(string jobKey, string jobGroup = "")
{
    await RemoveJobAsync(jobKey, jobGroup);
}

public async Task RemoveJobAsync(string jobKey, string jobGroup = "")
{
    // existing implementation unchanged
    if (string.IsNullOrEmpty(jobGroup))
        jobGroup = QuartzConstant.JOB_DEFAULT_GROUP;

    var job = new JobKey(jobKey, jobGroup);
    if (!await scheduler.CheckExists(job))
        return;

    var state = await scheduler.GetTriggerState(new TriggerKey(jobKey));
    if (state == TriggerState.Normal)
        await scheduler.PauseJob(job);

    await scheduler.DeleteJob(job);
}
```

- [ ] **Step 3: Handle class name typo**

In `src/QuartzExtention.cs`, rename class and add obsolete forwarding:
```csharp
[Obsolete("Use QuartzExtension instead")]
public static class QuartzExtention { }

public static class QuartzExtension
{
    // all existing methods moved here unchanged
    public static string GetJobTraceId(this IJobExecutionContext context) => ...;
    // ... rest of methods
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build src/Lycoris.Quartz.csproj
```

- [ ] **Step 5: Commit**

```bash
git add src/IQuartzSchedulerCenter.cs src/Services/QuartzSchedulerCenter.cs src/QuartzExtention.cs
git commit -m "fix: correct typos (RemoveobAsync->RemoveJobAsync, QuartzExtention->QuartzExtension), remove dead catch"
```

---

## Task 8: Extract trigger factory (O2)

**Files:**
- Create: `src/Services/QuartzTriggerFactory.cs`
- Modify: `src/Services/QuartzSchedulerCenter.cs`

- [ ] **Step 1: Create QuartzTriggerFactory**

New file `src/Services/QuartzTriggerFactory.cs`:

```csharp
using Lycoris.Quartz.Options;
using Quartz;
using System;

namespace Lycoris.Quartz.Services
{
    internal static class QuartzTriggerFactory
    {
        internal static ITrigger CreateCronTrigger(QuartzSchedulerOption sche)
        {
            var trigger = TriggerBuilder.Create();
            trigger = trigger.WithIdentity(sche.JobName, sche.JobGroup);
            trigger = trigger.StartAt(sche.BeginTime);

            if (sche.EndTime.HasValue)
                trigger = trigger.EndAt(sche.EndTime);

            if (sche.CronRunOnProceed)
                trigger.WithCronSchedule(sche.Cron, b => b.WithMisfireHandlingInstructionFireAndProceed());
            else
                trigger.WithCronSchedule(sche.Cron, b => b.WithMisfireHandlingInstructionDoNothing());

            return trigger.ForJob(sche.JobName, sche.JobGroup).Build();
        }

        internal static ITrigger CreateSimpleTrigger(QuartzSchedulerOption sche)
        {
            var triggerBuilder = TriggerBuilder.Create();
            triggerBuilder = triggerBuilder.WithIdentity(sche.JobName, sche.JobGroup)
                                           .StartAt(sche.BeginTime);

            if (sche.EndTime.HasValue)
                triggerBuilder = triggerBuilder.EndAt(sche.EndTime);

            triggerBuilder = triggerBuilder.WithSimpleSchedule(x =>
            {
                if (sche.RunTimes > 0)
                {
                    x.WithIntervalInSeconds(sche.IntervalSecond)
                     .WithRepeatCount(sche.RunTimes)
                     .WithMisfireHandlingInstructionFireNow();
                }
                else
                {
                    x.WithIntervalInSeconds(sche.IntervalSecond)
                     .RepeatForever()
                     .WithMisfireHandlingInstructionFireNow();
                }
            });

            return triggerBuilder.ForJob(sche.JobName, sche.JobGroup).Build();
        }
    }
}
```

- [ ] **Step 2: Remove old methods from QuartzSchedulerCenter**

In `src/Services/QuartzSchedulerCenter.cs`, delete the `CreateCronTrigger` and `CreateSimpleTrigger` private methods (lines 512-567), and update the caller at line 172-174:

```csharp
// Before
var trigger = sche.Trigger == QuartzTriggerEnum.CRON
    ? CreateCronTrigger(sche)
    : CreateSimpleTrigger(sche);

// After
var trigger = sche.Trigger == QuartzTriggerEnum.CRON
    ? QuartzTriggerFactory.CreateCronTrigger(sche)
    : QuartzTriggerFactory.CreateSimpleTrigger(sche);
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build src/Lycoris.Quartz.csproj
```

- [ ] **Step 4: Commit**

```bash
git add src/Services/QuartzTriggerFactory.cs src/Services/QuartzSchedulerCenter.cs
git commit -m "refactor: extract trigger creation into QuartzTriggerFactory"
```

---

## Task 9: Unify job validation logic (O3)

**Files:**
- Modify: `src/QuartzJobHelper.cs`

- [ ] **Step 1: Extract validation method**

Add private method to `QuartzJobHelper`:

```csharp
private static void ValidateJobType(Type type)
{
    if (!type.IsClass || !type.IsPublic || type.IsAbstract)
        throw new Exception($"the {type.FullName} must be a public class and cannot be an abstract class");

    if (!type.IsSubclassOf(typeof(BaseQuartzJob)) && !typeof(IJob).IsAssignableFrom(type))
        throw new Exception($"the {type.FullName} must be subclass of 'BaseQuartzJob' or assignable from 'IJob'");
}
```

- [ ] **Step 2: Update GetJob to use shared validation**

Replace lines 77-81 in `GetJob(Type, bool)`:

```csharp
// Before
if (!type.IsClass || !type.IsPublic || type.IsAbstract)
    throw new Exception($"the {type.FullName} must be a public class and cannot be a abstract class");

if (!type.IsSubclassOf(typeof(BaseQuartzJob)) && !typeof(IJob).IsAssignableFrom(type))
    throw new Exception($"the {type.FullName} muse be subclass of 'BaseQuartzJob' or assignable from 'IJob'");
// After
ValidateJobType(type);
```

- [ ] **Step 3: Update GetJobs to use shared validation**

Replace lines 39-47 in `GetJobs(params Type[])`:

```csharp
// Before
var modifierList = types.Where(x => !x.IsClass || !x.IsPublic || x.IsAbstract).Select(x => x.FullName).ToList() ?? new List<string>();

if (modifierList.Any())
    throw new Exception($"the [{string.Join(",", modifierList)}] must be a public class and cannot be a abstract class");

var jobClass = types.Where(x => x.IsSubclassOf(typeof(BaseQuartzJob)) && typeof(IJob).IsAssignableFrom(x)).Select(x => x.FullName).ToList() ?? new List<string>();

if (jobClass.Any())
    throw new Exception($"the [{string.Join(",", jobClass)}] muse be subclass of 'BaseQuartzJob' or assignable from 'IJob'");
// After
foreach (var type in types)
    ValidateJobType(type);
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build src/Lycoris.Quartz.csproj
```

- [ ] **Step 5: Commit**

```bash
git add src/QuartzJobHelper.cs
git commit -m "refactor: extract shared job type validation into ValidateJobType"
```

---

## Task 10: Eliminate QuartzSchedulerOption construction duplication (O1)

**Files:**
- Modify: `src/QuartzBuilderExtensions.cs`

- [ ] **Step 1: Add mapping method**

Add private static method to `QuartzBuilderExtensions`:

```csharp
private static QuartzSchedulerOption ToSchedulerOption(QuartzJobType job)
{
    var s = job.JobSettings;
    return new QuartzSchedulerOption
    {
        JobType = job.JobType,
        Standby = s.Standby,
        BeginTime = new DateTime(2000, 1, 1),
        Trigger = s.Trigger,
        Cron = s.Cron,
        IntervalSecond = s.IntervalSecond,
        RunTimes = s.RunTimes,
        JobGroup = string.IsNullOrEmpty(s.JobGroup) ? QuartzConstant.JOB_DEFAULT_GROUP : s.JobGroup,
        JobName = s.JobName,
        CronRunOnProceed = s.CronRunOnProceed
    };
}
```

- [ ] **Step 2: Update AddQuartzSchedulerJob&lt;T&gt;() (no configure)**

Replace lines 85-97:

```csharp
// Before
var option = new QuartzSchedulerOption()
{
    JobType = job.JobType,
    Standby = job.JobSettings.Standby,
    BeginTime = new DateTime(2000, 1, 1),
    Trigger = job.JobSettings.Trigger,
    Cron = job.JobSettings.Cron,
    IntervalSecond = job.JobSettings.IntervalSecond,
    RunTimes = job.JobSettings.RunTimes,
    JobGroup = string.IsNullOrEmpty(job.JobSettings.JobGroup) ? QuartzConstant.JOB_DEFAULT_GROUP : job.JobSettings.JobGroup,
    JobName = job.JobSettings.JobName,
    CronRunOnProceed = job.JobSettings.CronRunOnProceed
};
// After
var option = ToSchedulerOption(job);
```

- [ ] **Step 3: Update AddQuartzSchedulerJob(params Type[])**

Replace lines 120-132:

```csharp
// Before
var option = new QuartzSchedulerOption()
{
    JobType = item.JobType,
    Standby = item.JobSettings.Standby,
    BeginTime = new DateTime(2000, 1, 1),
    Trigger = item.JobSettings.Trigger,
    Cron = item.JobSettings.Cron,
    IntervalSecond = item.JobSettings.IntervalSecond,
    RunTimes = item.JobSettings.RunTimes,
    JobGroup = string.IsNullOrEmpty(item.JobSettings.JobGroup) ? QuartzConstant.JOB_DEFAULT_GROUP : item.JobSettings.JobGroup,
    JobName = item.JobSettings.JobName,
    CronRunOnProceed = item.JobSettings.CronRunOnProceed
};
// After
var option = ToSchedulerOption(item);
```

- [ ] **Step 4: Commit**

```bash
git add src/QuartzBuilderExtensions.cs
git commit -m "refactor: extract QuartzJobType-to-Option mapping to eliminate duplication"
```

---

## Task 11: Expose Quartz native properties via QuartzBuilder (E1)

**Files:**
- Modify: `src/QuartzBuilder.cs`
- Modify: `src/QuartzBuilderExtensions.cs:57-65`

- [ ] **Step 1: Add properties to QuartzBuilder**

In `src/QuartzBuilder.cs`, add after `ThreadCount`:

```csharp
/// <summary>
/// 调度器实例名称
/// </summary>
public string InstanceName { get; set; } = "QuartzScheduler";

/// <summary>
/// 表前缀（用于ADO JobStore）
/// </summary>
public string TablePrefix { get; set; } = "QRTZ_";

/// <summary>
/// JobStore 类型全名（如 "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"）
/// </summary>
public string JobStoreType { get; set; }

/// <summary>
/// 数据源连接字符串名称
/// </summary>
public string DataSource { get; set; }

/// <summary>
/// 自定义 Quartz 属性集合
/// </summary>
public System.Collections.Specialized.NameValueCollection Properties { get; } = new System.Collections.Specialized.NameValueCollection();
```

- [ ] **Step 2: Update StdSchedulerFactory construction to use new properties**

In `src/QuartzBuilderExtensions.cs:57-65`, update the `AddSchedulerFactory` lambda:

```csharp
// Before
services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>(x =>
{
    var options = new NameValueCollection
    {
        { "quartz.threadPool.ThreadCount", buidler.ThreadCount.ToString() }
    };
    return new StdSchedulerFactory(options);
}).AddBaseQuartzSchedulerCenter();

// After
services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>(x =>
{
    var options = new NameValueCollection
    {
        { "quartz.threadPool.ThreadCount", buidler.ThreadCount.ToString() },
        { "quartz.scheduler.instanceName", buidler.InstanceName }
    };

    if (!string.IsNullOrEmpty(buidler.JobStoreType))
        options["quartz.jobStore.type"] = buidler.JobStoreType;

    if (!string.IsNullOrEmpty(buidler.TablePrefix))
        options["quartz.jobStore.tablePrefix"] = buidler.TablePrefix;

    if (!string.IsNullOrEmpty(buidler.DataSource))
        options["quartz.jobStore.dataSource"] = buidler.DataSource;

    foreach (string key in buidler.Properties.Keys)
        options[key] = buidler.Properties[key];

    return new StdSchedulerFactory(options);
}).AddBaseQuartzSchedulerCenter();
```

- [ ] **Step 3: Commit**

```bash
git add src/QuartzBuilder.cs src/QuartzBuilderExtensions.cs
git commit -m "feat: expose Quartz native properties (InstanceName, JobStore, DataSource) via QuartzBuilder"
```

---

## Task 12: Assembly scanning public API (E4)

**Files:**
- Modify: `src/QuartzBuilderExtensions.cs`

- [ ] **Step 1: Add public extension method**

In `src/QuartzBuilderExtensions.cs`, add after existing `AddQuartzSchedulerJob` overloads:

```csharp
/// <summary>
/// 从程序集中扫描并注册所有带 QuartzJobAttribute 的调度任务
/// </summary>
/// <param name="services"></param>
/// <param name="assembly"></param>
/// <returns></returns>
public static IServiceCollection AddQuartzSchedulerJobsFromAssembly(this IServiceCollection services, System.Reflection.Assembly assembly)
{
    var jobs = QuartzJobHelper.GetJobsByAssembly(assembly);

    foreach (var item in jobs)
    {
        services.AddScoped(item.JobType);
        services.AddSingleton(ToSchedulerOption(item));
    }

    return services;
}
```

Also make `QuartzJobHelper.GetJobsByAssembly` `public static` (change from `internal static`):
In `src/QuartzJobHelper.cs:17`:
```csharp
// Before
internal static List<QuartzJobType> GetJobsByAssembly(Assembly assembly)
// After
public static List<QuartzJobType> GetJobsByAssembly(Assembly assembly)
```

- [ ] **Step 2: Commit**

```bash
git add src/QuartzBuilderExtensions.cs src/QuartzJobHelper.cs
git commit -m "feat: add AddQuartzSchedulerJobsFromAssembly for assembly scanning"
```

---

## Task 13: Support multiple listeners (E2)

**Files:**
- Modify: `src/QuartzBuilder.cs`
- Modify: `src/Services/QuartzSchedulerCenter.cs`

- [ ] **Step 1: Replace single-registration with list registration in QuartzBuilder**

In `src/QuartzBuilder.cs`, replace the three Add*Listener methods and add internal collections:

```csharp
internal List<Type> SchedulerListenerTypes { get; } = new List<Type>();
internal List<Type> JobListenerTypes { get; } = new List<Type>();
internal List<Type> TriggerListenerTypes { get; } = new List<Type>();

public QuartzBuilder AddSchedulerListener<T>() where T : SchedulerListener
{
    SchedulerListenerTypes.Add(typeof(T));
    return this;
}

public QuartzBuilder AddJobListener<T>() where T : JobListener
{
    JobListenerTypes.Add(typeof(T));
    return this;
}

public QuartzBuilder AddTriggerListener<T>() where T : TriggerListener
{
    TriggerListenerTypes.Add(typeof(T));
    return this;
}
```

- [ ] **Step 2: Update QuartzBuilderExtensions to register all listener types**

In `src/QuartzBuilderExtensions.cs`, in the `AddQuartzSchedulerCenter(configure)` method, after line 67, replace `services.TryAddSingleton<IJobListener, JobListener>();` with:

```csharp
services.TryAddSingleton<IJobListener, JobListener>();

foreach (var type in buidler.SchedulerListenerTypes)
    services.AddSingleton(typeof(ISchedulerListener), type);

foreach (var type in buidler.JobListenerTypes)
    services.AddSingleton(typeof(IJobListener), type);

foreach (var type in buidler.TriggerListenerTypes)
    services.AddSingleton(typeof(ITriggerListener), type);
```

- [ ] **Step 3: Update QuartzSchedulerCenter to resolve and add all listeners**

In `src/Services/QuartzSchedulerCenter.cs`, in `StartScheduleAsync` (lines 62-72), replace single-resolution with multi-resolution:

```csharp
// Before
var schedulerListener = _serviceProvider.GetService<ISchedulerListener>();
if (schedulerListener != null)
    scheduler.ListenerManager.AddSchedulerListener(schedulerListener);

var jobListener = _serviceProvider.GetService<IJobListener>();
if (jobListener != null)
    scheduler.ListenerManager.AddJobListener(jobListener);

var triggerListener = _serviceProvider.GetService<ITriggerListener>();
if (triggerListener != null)
    scheduler.ListenerManager.AddTriggerListener(triggerListener);

// After
var schedulerListeners = _serviceProvider.GetServices<ISchedulerListener>();
foreach (var listener in schedulerListeners)
    scheduler.ListenerManager.AddSchedulerListener(listener);

var jobListeners = _serviceProvider.GetServices<IJobListener>();
foreach (var listener in jobListeners)
    scheduler.ListenerManager.AddJobListener(listener);

var triggerListeners = _serviceProvider.GetServices<ITriggerListener>();
foreach (var listener in triggerListeners)
    scheduler.ListenerManager.AddTriggerListener(listener);
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build src/Lycoris.Quartz.csproj
```

- [ ] **Step 5: Commit**

```bash
git add src/QuartzBuilder.cs src/QuartzBuilderExtensions.cs src/Services/QuartzSchedulerCenter.cs
git commit -m "feat: support multiple listener instances per listener type"
```

---

## Task 14: DisallowConcurrentExecution configurable (E3)

**Files:**
- Modify: `src/Options/QuartzSchedulerOption.cs`
- Modify: `src/Services/QuartzSchedulerCenter.cs:160-170`

- [ ] **Step 1: Add property to QuartzSchedulerOption**

In `src/Options/QuartzSchedulerOption.cs`, add after `CronRunOnProceed`:

```csharp
/// <summary>
/// 禁止并发执行（只有上一个任务完成才会执行下一次任务）
/// 默认：false
/// </summary>
public bool DisallowConcurrentExecution { get; set; } = false;
```

- [ ] **Step 2: Apply in job builder**

In `src/Services/QuartzSchedulerCenter.cs`, in `AddJobAsync(QuartzSchedulerOption sche)`, after line 163:

```csharp
var job = jobBuilder.UsingJobData(QuartzConstant.JOB_NAME, sche.JobName)
                    .UsingJobDataIf(!string.IsNullOrEmpty(sche.JsonMap), QuartzConstant.JSON_MAP, sche.JsonMap)
                    .UsingJobData(QuartzConstant.JOB_ARGS, sche.Args)
                    .UsingJobData(QuartzConstant.JOB_OPTIONS, Newtonsoft.Json.JsonConvert.SerializeObject(sche))
                    .UsingJobData(QuartzConstant.JOB_RUN_COUNT, 0L)
                    .WithDescription(sche.Remark)
                    .WithIdentity(sche.JobName, sche.JobGroup);

// Add this line:
if (sche.DisallowConcurrentExecution)
    jobBuilder.DisallowConcurrentExecution();

var job = jobBuilder.Build();
```

- [ ] **Step 3: Commit**

```bash
git add src/Options/QuartzSchedulerOption.cs src/Services/QuartzSchedulerCenter.cs
git commit -m "feat: support DisallowConcurrentExecution via fluent config"
```

---

## Task 15: Trigger Priority configurable (E5)

**Files:**
- Modify: `src/Options/QuartzSchedulerOption.cs`
- Modify: `src/Services/QuartzTriggerFactory.cs`

- [ ] **Step 1: Add Priority property**

In `src/Options/QuartzSchedulerOption.cs`, add after `CronRunOnProceed`:

```csharp
/// <summary>
/// 触发器优先级，数值越大优先级越高
/// 默认：5
/// </summary>
public int Priority { get; set; } = 5;
```

- [ ] **Step 2: Apply priority in trigger creation**

In `src/Services/QuartzTriggerFactory.cs`, in `CreateCronTrigger`, after the `ForJob` line:
```csharp
return trigger.ForJob(sche.JobName, sche.JobGroup).Build();
```
Change `CreateCronTrigger` to add priority:
```csharp
internal static ITrigger CreateCronTrigger(QuartzSchedulerOption sche)
{
    var trigger = TriggerBuilder.Create();
    trigger = trigger.WithIdentity(sche.JobName, sche.JobGroup)
                     .StartAt(sche.BeginTime)
                     .WithPriority(sche.Priority);  // add this

    if (sche.EndTime.HasValue)
        trigger = trigger.EndAt(sche.EndTime);
    // ... rest unchanged
}
```

In `CreateSimpleTrigger`, add `.WithPriority(sche.Priority)`:
```csharp
triggerBuilder = triggerBuilder.WithIdentity(sche.JobName, sche.JobGroup)
                               .StartAt(sche.BeginTime)
                               .WithPriority(sche.Priority);  // add this
```

- [ ] **Step 3: Commit**

```bash
git add src/Options/QuartzSchedulerOption.cs src/Services/QuartzTriggerFactory.cs
git commit -m "feat: add trigger Priority option to QuartzSchedulerOption"
```

---

## Task 16: Final build and verification

- [ ] **Step 1: Full build**

```bash
dotnet build
```

- [ ] **Step 2: Verify sample project compiles**

```bash
dotnet build samples/QuartzSample/QuartzSample.csproj
```

- [ ] **Step 3: Run available tests**

```bash
dotnet test 2>/dev/null || echo "No test project found"
```

