# Quartz.Extensions 代码审查与改进设计

**日期**: 2026-05-18
**范围**: Bug 修复、功能扩展、代码优化

---

## 一、Bug 修复

### B1. `IsAssignableFrom` 方向颠倒

- **文件**: `src/QuartzJobHelper.cs:20, :44, :80`
- **原因**: `x.IsAssignableFrom(typeof(IJob))` 判断"x 是否可被 IJob 赋值"，应为 `typeof(IJob).IsAssignableFrom(x)` 判断"IJob 是否可被 x 赋值"
- **修复**: 三处全部修正

### B2. `GetJobs` 验证逻辑反转

- **文件**: `src/QuartzJobHelper.cs:39`
- **原因**: `!x.IsClass || x.IsPublic || x.IsAbstract` 逻辑与语义相反，合法 public class 被过滤
- **修复**: 改为 `!x.IsClass || !x.IsPublic || x.IsAbstract`

### B3. `DefaultQuartzJobHostedService` 空引用

- **文件**: `src/DefaultQuartzJobHostedService.cs:38`
- **原因**: `||` 应在 `_options` 为 null 时短路但不生效，`.Count()` 抛出 NRE
- **修复**: 改为 `_options != null && _options.Any()`

### B4. `CronRunOnProceed` 配置无效

- **文件**: `src/Services/QuartzSchedulerCenter.cs:525-528`
- **原因**: if/else 两个分支代码相同
- **修复**: else 分支改用 `WithMisfireHandlingInstructionDoNothing`

### B5. `AddOnceJobAsync<T>()` 无参版本缺少清理标记

- **文件**: `src/Services/QuartzSchedulerCenter.cs:198-206`
- **原因**: 未设置 `JsonMap = QuartzConstant.ONCE_JOB`，`JobListener` 不触发删除
- **修复**: 补上 `JsonMap = QuartzConstant.ONCE_JOB`

### B6. `StartJobAsync` 错误消息

- **文件**: `src/Services/QuartzSchedulerCenter.cs:322`
- **修复**: `"already exists"` → `"does not exist"`

### B7. 拼写修正与死代码清理

- `RemoveobAsync` → `RemoveJobAsync`（保留旧方法加 `[Obsolete]`，新增正确命名）
- `QuartzExtention` → `QuartzExtension`（类名，保留下划线转发的 `[Obsolete]` 兼容）
- `src/Services/QuartzSchedulerCenter.cs:96-99`: 删除无效 `catch { throw; }`

---

## 二、功能扩展

### E1. Quartz 原生配置属性暴露

- **文件**: `src/QuartzBuilder.cs`
- **内容**: 增加 `NameValueCollection` 字典 + 强类型属性
- **属性**: `InstanceName`, `TablePrefix`, `JobStoreType`, `DataSource`
- **影响**: 用户在 `AddQuartzSchedulerCenter(opt => { ... })` 中即可配置数据库 JobStore

### E2. Listener 支持多实例

- **文件**: `src/QuartzBuilder.cs`, `src/Services/QuartzSchedulerCenter.cs`
- **内容**: `AddSchedulerListener<T>` 等改为往内部集合注册，启动时遍历集合依次 Add
- **影响**: 用户可注册多个不同 Listener

### E3. `DisallowConcurrentExecution` 配置化

- **文件**: `src/Options/QuartzSchedulerOption.cs`, `src/Services/QuartzSchedulerCenter.cs`
- **内容**: `QuartzSchedulerOption` 增加 `bool DisallowConcurrentExecution`，`CreateSimpleTrigger` / `CreateCronTrigger` 中调用 `jobBuilder.DisallowConcurrentExecution()`

### E4. 程序集扫描公开 API

- **文件**: `src/QuartzBuilderExtensions.cs`
- **内容**: 新增 `AddQuartzSchedulerJobsFromAssembly(Assembly)` 公开扩展方法，封装 `QuartzJobHelper.GetJobsByAssembly`

### E5. Trigger 优先级

- **文件**: `src/Options/QuartzSchedulerOption.cs`, `src/Services/QuartzSchedulerCenter.cs`
- **内容**: `QuartzSchedulerOption` 增加 `int Priority = 5`，trigger 创建时设置

---

## 三、代码优化

### O1. 消除 `QuartzSchedulerOption` 构造重复

- **文件**: `src/QuartzBuilderExtensions.cs`
- **内容**: 提取 `QuartzJobType → QuartzSchedulerOption` 映射为独立方法，4 个重载统一调用

### O2. `QuartzSchedulerCenter` 职责拆分

- **文件**: `src/Services/QuartzSchedulerCenter.cs` → 新增 `src/Services/QuartzTriggerFactory.cs`
- **内容**: 将 `CreateCronTrigger`, `CreateSimpleTrigger` 移到独立静态工厂类

### O3. `QuartzJobHelper` 验证逻辑统一

- **文件**: `src/QuartzJobHelper.cs`
- **内容**: 提取 `ValidateJobType(Type)` 方法，`GetJob` 和 `GetJobs` 共用

---

## 实施顺序

1. **B1-B7** — Bug 修复（7 项）
2. **O2-O3-O1** — 优化先行（降低扩展改动的冲突概率）
3. **E1 → E4 → E2 → E3 → E5** — 扩展按依赖顺序

共约 18 项改动，涉及约 12 个文件。
