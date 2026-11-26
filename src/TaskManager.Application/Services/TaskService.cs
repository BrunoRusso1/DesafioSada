using Microsoft.Extensions.Logging;
using TaskManager.Application.Dtos;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository repository, ILogger<TaskService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskResponseDto>> GetAllAsync(TaskItemStatus? status, DateTime? dueDate, string? title)
    {
        var tasks = await _repository.GetAllAsync(status, dueDate, title);
        return tasks.Select(ToResponse).ToList();
    }

    public async Task<TaskResponseDto> GetByIdAsync(Guid id)
    {
        var task = await FindOrThrowAsync(id);
        return ToResponse(task);
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Description = dto.Description,
            DueDate = dto.DueDate,
            Status = dto.Status
        };

        await _repository.AddAsync(task);
        _logger.LogInformation("Task {TaskId} created", task.Id);

        return ToResponse(task);
    }

    public async Task<TaskResponseDto> UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var task = await FindOrThrowAsync(id);

        task.Title = dto.Title.Trim();
        task.Description = dto.Description;
        task.DueDate = dto.DueDate;
        task.Status = dto.Status;

        await _repository.UpdateAsync(task);
        _logger.LogInformation("Task {TaskId} updated", task.Id);

        return ToResponse(task);
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await FindOrThrowAsync(id);

        await _repository.DeleteAsync(task);
        _logger.LogInformation("Task {TaskId} deleted", id);
    }

    private async Task<TaskItem> FindOrThrowAsync(Guid id)
    {
        var task = await _repository.GetByIdAsync(id);
        if (task is null)
        {
            _logger.LogWarning("Task {TaskId} not found", id);
            throw new NotFoundException($"Task {id} not found.");
        }

        return task;
    }

    private static TaskResponseDto ToResponse(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        DueDate = task.DueDate,
        Status = task.Status
    };
}
