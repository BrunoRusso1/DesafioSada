using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManager.Application.Dtos;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repository = new();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(_repository.Object, NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsTaskWithGeneratedId()
    {
        var dto = new CreateTaskDto { Title = "  Study  ", Status = TaskItemStatus.Pending };

        var result = await _service.CreateAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Study", result.Title);
        _repository.Verify(repository => repository.AddAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_PassesFiltersToRepository()
    {
        var dueDate = new DateTime(2026, 12, 1);
        _repository
            .Setup(repository => repository.GetAllAsync(TaskItemStatus.Completed, dueDate, null))
            .ReturnsAsync(new List<TaskItem> { new() { Id = Guid.NewGuid(), Title = "Done", Status = TaskItemStatus.Completed } });

        var result = await _service.GetAllAsync(TaskItemStatus.Completed, dueDate, null);

        Assert.Single(result);
        Assert.Equal(TaskItemStatus.Completed, result[0].Status);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsNotFoundException()
    {
        _repository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_UpdatesFields()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Old", Status = TaskItemStatus.Pending };
        _repository.Setup(repository => repository.GetByIdAsync(task.Id)).ReturnsAsync(task);
        var dto = new UpdateTaskDto { Title = "New", Description = "Desc", Status = TaskItemStatus.InProgress };

        var result = await _service.UpdateAsync(task.Id, dto);

        Assert.Equal("New", result.Title);
        Assert.Equal(TaskItemStatus.InProgress, result.Status);
        _repository.Verify(repository => repository.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsNotFoundException()
    {
        _repository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), new UpdateTaskDto { Title = "x" }));
    }

    [Fact]
    public async Task DeleteAsync_ExistingTask_DeletesFromRepository()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Delete me" };
        _repository.Setup(repository => repository.GetByIdAsync(task.Id)).ReturnsAsync(task);

        await _service.DeleteAsync(task.Id);

        _repository.Verify(repository => repository.DeleteAsync(task), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ThrowsNotFoundException()
    {
        _repository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid()));
    }
}
