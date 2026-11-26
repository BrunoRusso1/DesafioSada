using TaskManager.Application.Dtos;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponseDto>> GetAllAsync(TaskItemStatus? status, DateTime? dueDate, string? title);
    Task<TaskResponseDto> GetByIdAsync(Guid id);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
    Task<TaskResponseDto> UpdateAsync(Guid id, UpdateTaskDto dto);
    Task DeleteAsync(Guid id);
}
