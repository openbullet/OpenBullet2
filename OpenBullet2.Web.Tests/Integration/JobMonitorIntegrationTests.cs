using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Tests.Extensions;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using Xunit.Abstractions;

namespace OpenBullet2.Web.Tests.Integration;

[Collection("IntegrationTests")]
public class JobMonitorIntegrationTests(ITestOutputHelper testOutputHelper)
    : IntegrationTests(testOutputHelper)
{
    [Fact]
    public async Task GetAllTriggeredActions_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobMonitorService = GetRequiredService<JobMonitorService>();
        var triggeredAction = new TriggeredAction
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            JobId = 1,
            Triggers = [
                new JobStatusTrigger { Status = JobStatus.Running },
                new ProgressTrigger { Comparison = NumComparison.GreaterThan, Amount = 50 },
                new TimeElapsedTrigger { Comparison = NumComparison.GreaterThan, Seconds = 10 }
            ],
            Actions = [
                new StopJobAction { JobId = 1 },
                new WaitAction { Seconds = 10 },
                new StartJobAction { JobId = 1 }
            ]
        };
        jobMonitorService.TriggeredActions.Add(triggeredAction);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<TriggeredActionDto>>(
            client, "api/v1/job-monitor/triggered-action/all");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        
        var dto = result.Value.First();
        
        Assert.Equal(triggeredAction.Id, dto.Id);
        Assert.Equal(triggeredAction.Name, dto.Name);
        Assert.Equal(triggeredAction.JobId, dto.JobId);
        Assert.Equal(triggeredAction.Triggers.Count, dto.Triggers.Count);
        Assert.Equal(triggeredAction.Actions.Count, dto.Actions.Count);
    }
    
    [Fact]
    public async Task GetAllTriggeredActions_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var result = await GetJsonAsync<IEnumerable<TriggeredActionDto>>(
            client, "api/v1/job-monitor/triggered-action/all");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task GetTriggeredAction_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobMonitorService = GetRequiredService<JobMonitorService>();
        var triggeredAction = new TriggeredAction
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            JobId = 1,
            Triggers = [
                new JobStatusTrigger { Status = JobStatus.Running },
                new ProgressTrigger { Comparison = NumComparison.GreaterThan, Amount = 50 },
                new TimeElapsedTrigger { Comparison = NumComparison.GreaterThan, Seconds = 10 }
            ],
            Actions = [
                new StopJobAction { JobId = 1 },
                new WaitAction { Seconds = 10 },
                new StartJobAction { JobId = 1 }
            ]
        };
        jobMonitorService.TriggeredActions.Add(triggeredAction);
        
        // Act
        var queryParams = new { id = triggeredAction.Id };
        var result = await GetJsonAsync<TriggeredActionDto>(
            client, "api/v1/job-monitor/triggered-action".ToUri(queryParams));
        
        // Assert
        Assert.True(result.IsSuccess);
        
        var dto = result.Value;
        
        Assert.Equal(triggeredAction.Id, dto.Id);
        Assert.Equal(triggeredAction.Name, dto.Name);
        Assert.Equal(triggeredAction.JobId, dto.JobId);
        Assert.Equal(triggeredAction.Triggers.Count, dto.Triggers.Count);
        Assert.Equal(triggeredAction.Actions.Count, dto.Actions.Count);
    }
    
    [Fact]
    public async Task GetTriggeredAction_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new { id = 1 };
        var result = await GetJsonAsync<TriggeredActionDto>(
            client, "api/v1/job-monitor/triggered-action".ToUri(queryParams));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task CreateTriggeredAction_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobMonitorService = GetRequiredService<JobMonitorService>();

        var triggers = new List<TriggerDto> {
            new JobStatusTriggerDto { Status = JobStatus.Running },
            new ProgressTriggerDto { Comparison = NumComparison.GreaterThan, Amount = 50 },
            new TimeElapsedTriggerDto { Comparison = NumComparison.GreaterThan, TimeSpan = TimeSpan.FromSeconds(10) }
        };

        var actions = new List<ActionDto> {
            new StopJobActionDto { JobId = 1 },
            new WaitActionDto { TimeSpan = TimeSpan.FromSeconds(10) },
            new StartJobActionDto { JobId = 1 }
        };
        
        var dto = new CreateTriggeredActionDto
        {
            Name = "Test",
            JobId = 1,
            Triggers = triggers.Select(Serialize).ToList(),
            Actions = actions.Select(Serialize).ToList()
        };
        
        // Act
        var result = await PostJsonAsync<TriggeredActionDto>(
            client, "api/v1/job-monitor/triggered-action", dto);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Id);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(dto.JobId, result.Value.JobId);
        Assert.Equal(dto.Triggers.Count, result.Value.Triggers.Count);
        Assert.Equal(dto.Actions.Count, result.Value.Actions.Count);
        
        Assert.Single(jobMonitorService.TriggeredActions);
        
        var triggeredAction = jobMonitorService.TriggeredActions[0];
        Assert.Equal(result.Value.Id, triggeredAction.Id);
        Assert.Equal(dto.Name, triggeredAction.Name);
        Assert.Equal(dto.JobId, triggeredAction.JobId);
        Assert.Equal(dto.Triggers.Count, triggeredAction.Triggers.Count);
        Assert.Equal(dto.Actions.Count, triggeredAction.Actions.Count);
    }
    
    [Fact]
    public async Task CreateTriggeredAction_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        var dto = new CreateTriggeredActionDto
        {
            JobId = 1
        };
        
        // Act
        var result = await PostJsonAsync<TriggeredActionDto>(
            client, "api/v1/job-monitor/triggered-action", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task UpdateTriggeredAction_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobMonitorService = GetRequiredService<JobMonitorService>();
        var triggeredAction = new TriggeredAction {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            JobId = 1,
            Triggers = [
                new JobStatusTrigger { Status = JobStatus.Running },
                new ProgressTrigger { Comparison = NumComparison.GreaterThan, Amount = 50 },
                new TimeElapsedTrigger { Comparison = NumComparison.GreaterThan, Seconds = 10 }
            ],
            Actions = [
                new StopJobAction { JobId = 1 },
                new WaitAction { Seconds = 10 },
                new StartJobAction { JobId = 1 }
            ]
        };
        jobMonitorService.TriggeredActions.Add(triggeredAction);

        var triggers = new List<TriggerDto> {
            new JobStatusTriggerDto { Status = JobStatus.Idle },
            new TimeElapsedTriggerDto { Comparison = NumComparison.LessThan, TimeSpan = TimeSpan.FromSeconds(10) }
        };

        var actions = new List<ActionDto> {
            new StopJobActionDto { JobId = 2 }
        };

        var dto = new UpdateTriggeredActionDto {
            Id = triggeredAction.Id,
            Name = "Test2",
            JobId = 2,
            Triggers = triggers.Select(Serialize).ToList(),
            Actions = actions.Select(Serialize).ToList()
        };

        // Act
        var result = await PutJsonAsync<TriggeredActionDto>(
            client, "api/v1/job-monitor/triggered-action", dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto.Id, result.Value.Id);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(dto.JobId, result.Value.JobId);
        Assert.Equal(dto.Triggers.Count, result.Value.Triggers.Count);
        Assert.Equal(dto.Actions.Count, result.Value.Actions.Count);

        var updatedAction = jobMonitorService.TriggeredActions[0];
        Assert.Equal(dto.Id, updatedAction.Id);
        Assert.Equal(dto.Name, updatedAction.Name);
        Assert.Equal(dto.JobId, updatedAction.JobId);
        Assert.Equal(dto.Triggers.Count, updatedAction.Triggers.Count);
        Assert.Equal(dto.Actions.Count, updatedAction.Actions.Count);
    }
    
    [Fact]
    public async Task UpdateTriggeredAction_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);

        var dto = new UpdateTriggeredActionDto
        {
            JobId = 1
        };
        
        // Act
        var result = await PutJsonAsync<TriggeredActionDto>(
            client, "api/v1/job-monitor/triggered-action", dto);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, result.Error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task ResetTriggeredAction_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobMonitorService = GetRequiredService<JobMonitorService>();
        var triggeredAction = new TriggeredAction
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            JobId = 1,
            IsRepeatable = true,
            Executions = 5
        };
        jobMonitorService.TriggeredActions.Add(triggeredAction);
        
        // Act
        var queryParams = new { id = triggeredAction.Id };
        var error = await PostAsync(
            client, "api/v1/job-monitor/triggered-action/reset".ToUri(queryParams), null);
        
        // Assert
        Assert.Null(error);
        Assert.Equal(0, triggeredAction.Executions);
    }
    
    [Fact]
    public async Task ResetTriggeredAction_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new { id = 1 };
        var error = await PostAsync(
            client, "api/v1/job-monitor/triggered-action/reset".ToUri(queryParams), null);
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.Forbidden, error!.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task SetActive_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobMonitorService = GetRequiredService<JobMonitorService>();
        var triggeredAction = new TriggeredAction
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            JobId = 1,
            IsActive = true
        };
        jobMonitorService.TriggeredActions.Add(triggeredAction);
        
        // Act
        var queryParams = new { id = triggeredAction.Id, active = false };
        var error = await PostAsync(
            client, "api/v1/job-monitor/triggered-action/set-active".ToUri(queryParams), null);
        
        // Assert
        Assert.Null(error);
        Assert.False(triggeredAction.IsActive);
    }
    
    [Fact]
    public async Task SetActive_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new { id = 1, active = false };
        var error = await PostAsync(
            client, "api/v1/job-monitor/triggered-action/set-active".ToUri(queryParams), null);
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.Forbidden, error!.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, error.Content!.ErrorCode);
    }
    
    [Fact]
    public async Task Delete_Admin_Success()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var jobMonitorService = GetRequiredService<JobMonitorService>();
        var triggeredAction = new TriggeredAction
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            JobId = 1
        };
        jobMonitorService.TriggeredActions.Add(triggeredAction);
        
        // Act
        var queryParams = new { id = triggeredAction.Id };
        var error = await DeleteAsync(
            client, "api/v1/job-monitor/triggered-action".ToUri(queryParams));
        
        // Assert
        Assert.Null(error);
        Assert.Empty(jobMonitorService.TriggeredActions);
    }
    
    [Fact]
    public async Task Delete_Guest_Forbidden()
    {
        // Arrange
        using var client = Factory.CreateClient();
        var dbContext = GetRequiredService<ApplicationDbContext>();
        var guest = new GuestEntity { Username = "guest", AccessExpiration = DateTime.MaxValue };
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync();
        
        RequireLogin();
        ImpersonateGuest(client, guest);
        
        // Act
        var queryParams = new { id = 1 };
        var error = await DeleteAsync(
            client, "api/v1/job-monitor/triggered-action".ToUri(queryParams));
        
        // Assert
        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.Forbidden, error!.Response.StatusCode);
        Assert.Equal(ErrorCode.NotAdmin, error.Content!.ErrorCode);
    }
    
    private JsonElement Serialize<T>(T obj) where T : class
    {
        var node = JsonNode.Parse(
            JsonSerializer.Serialize(obj, JsonSerializerOptions))!;

        node["_polyTypeName"] = obj.GetType()
            .GetCustomAttribute<PolyTypeAttribute>()!.PolyType;
        
        return JsonSerializer.SerializeToElement(node, JsonSerializerOptions);
    }
}
