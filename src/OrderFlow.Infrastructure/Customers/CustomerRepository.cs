using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Customers;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<CustomerResponse?> GetResponseByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await (from c in _dbContext.Customers.AsNoTracking()
                      join u in _dbContext.Users.AsNoTracking() on c.UserId equals u.Id
                      where c.Id == id
                      select new CustomerResponse(
                          c.Id,
                          c.UserId,
                          u.FullName,
                          u.Email!,
                          c.PhoneNumber,
                          c.Address,
                          c.IsActive,
                          c.CreatedAtUtc,
                          c.UpdatedAtUtc))
                     .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CustomerResponse?> GetResponseByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await (from c in _dbContext.Customers.AsNoTracking()
                      join u in _dbContext.Users.AsNoTracking() on c.UserId equals u.Id
                      where c.UserId == userId
                      select new CustomerResponse(
                          c.Id,
                          c.UserId,
                          u.FullName,
                          u.Email!,
                          c.PhoneNumber,
                          c.Address,
                          c.IsActive,
                          c.CreatedAtUtc,
                          c.UpdatedAtUtc))
                     .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<CustomerResponse>> GetPagedListAsync(CustomerQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = from c in _dbContext.Customers.AsNoTracking()
                    join u in _dbContext.Users.AsNoTracking() on c.UserId equals u.Id
                    select new
                    {
                        Customer = c,
                        User = u
                    };

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var term = queryParams.SearchTerm.Trim();
            var pattern = $"%{term}%";

            query = query.Where(x =>
                EF.Functions.Like(x.User.FullName, pattern) ||
                (x.User.Email != null && EF.Functions.Like(x.User.Email, pattern)) ||
                EF.Functions.Like(x.Customer.PhoneNumber, pattern) ||
                EF.Functions.Like(x.Customer.Address, pattern));
        }

        if (queryParams.IsActive.HasValue)
        {
            query = query.Where(x => x.Customer.IsActive == queryParams.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = queryParams.PageNumber < 1 ? 1 : queryParams.PageNumber;
        var pageSize = queryParams.PageSize < 1 ? 10 : queryParams.PageSize;

        var items = await query
            .OrderByDescending(x => x.Customer.CreatedAtUtc)
            .ThenByDescending(x => x.Customer.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CustomerResponse(
                x.Customer.Id,
                x.Customer.UserId,
                x.User.FullName,
                x.User.Email!,
                x.Customer.PhoneNumber,
                x.Customer.Address,
                x.Customer.IsActive,
                x.Customer.CreatedAtUtc,
                x.Customer.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedList<CustomerResponse>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AnyAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
    }
}
