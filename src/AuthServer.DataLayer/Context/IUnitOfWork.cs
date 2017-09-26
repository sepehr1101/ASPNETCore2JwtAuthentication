using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AuthServer.DataLayer.Context
{
    public interface IUnitOfWork : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        int SaveChanges(bool acceptAllChangesOnSuccess);
        int SaveChanges();
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken());
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());

        /// <summary>
        /// to execute raw sql query , this is temporary until the next relase of .netCore
        /// </summary>
        /// <param name="query">raw query</param>
        /// <returns>List of T</returns>
        List<T> ExecSQL<T>(string query);

    }
}