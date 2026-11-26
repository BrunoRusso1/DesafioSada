using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Dtos;

public class CreateTaskDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [EnumDataType(typeof(TaskItemStatus))]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
}
