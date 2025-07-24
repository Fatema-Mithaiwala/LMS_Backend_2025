using Microsoft.EntityFrameworkCore;
using library_management_system_backend.Application.Interfaces;
using library_management_system_backend.Domain.Entities;
using library_management_system_backend.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using library_management_system_backend.Application.Interfaces.ReturnRequestTransaction;

namespace library_management_system_backend.Infrastructure.Repositories
{
    public class ReturnRequestRepository : IReturnRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ReturnRequestRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ReturnRequest>> GetAllAsync(string? status = null)
        {
            var query = _context.ReturnRequests
                .Include(rr => rr.User).ThenInclude(u => u.Role)
                .Include(rr => rr.Book)
                .Include(rr => rr.BorrowTransaction)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(rr => rr.Status == status);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ReturnRequest>> GetAllPendingAsync()
        {
            return await _context.ReturnRequests
                .Include(rr => rr.User).ThenInclude(u => u.Role)
                .Include(rr => rr.Book)
                .Include(rr => rr.BorrowTransaction)
                .Where(rr => rr.Status == "Pending")
                .ToListAsync();
        }

        public async Task<ReturnRequest?> GetByIdAsync(int id)
        {
            return await _context.ReturnRequests
                .Include(rr => rr.User).ThenInclude(u => u.Role)
                .Include(rr => rr.Book)
                .Include(rr => rr.BorrowTransaction)
                .FirstOrDefaultAsync(rr => rr.ReturnRequestId == id);
        }

        public async Task AddAsync(ReturnRequest returnRequest)
        {
            await _context.ReturnRequests.AddAsync(returnRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ReturnRequest returnRequest)
        {
            _context.ReturnRequests.Update(returnRequest);
            await _context.SaveChangesAsync();
        }
    }
}