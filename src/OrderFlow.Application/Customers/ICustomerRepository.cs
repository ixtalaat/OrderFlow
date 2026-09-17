using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> GetResponseByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> GetResponseByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<PagedList<CustomerResponse>> GetPagedListAsync(CustomerQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
}
