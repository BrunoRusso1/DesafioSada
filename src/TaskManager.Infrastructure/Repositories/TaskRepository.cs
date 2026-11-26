using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskItemStatus? status, DateTime? dueDate, string? title)
    {
        var query = _context.Tasks.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(task => task.Status == status.Value);
        }

        if (dueDate.HasValue)
        {
            var start = dueDate.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(task => task.DueDate >= start && task.DueDate < end);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(task => task.Title.ToLower().Contains(title.ToLower()));
        }

        return await query.OrderBy(task => task.DueDate).ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks.FirstOrDefaultAsync(task => task.Id == id);
    }

    public async Task AddAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await SaveAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await SaveAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            throw new DataAccessException("Could not save changes to the database.", exception);
        }
    }
}