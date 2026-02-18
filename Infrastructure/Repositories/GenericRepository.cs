using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GenericRepository<T>(StoreContext context) : IGenericRepository<T> where T : class
{
    public async Task<T?> GetByIdAsync(int id) =>
        await context.Set<T>().FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync() =>
        await context.Set<T>().ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await context.Set<T>().AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(T entity)
    {
        context.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await context.Set<T>().FindAsync(id) is not null;

    public async Task<int> SaveChangesAsync() =>
        await context.SaveChangesAsync();
}