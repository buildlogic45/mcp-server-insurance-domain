using BuildLogic45.Insurance.Core.Common;
using BuildLogic45.Insurance.Core.Policies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres;

/// <summary>
/// PostgreSQL-backed policy store. Reads are untracked, so the immutable <see cref="Policy"/>
/// records returned here can be passed straight back to <see cref="UpdateAsync"/>.
/// </summary>
public sealed class PostgresPolicyRepository(InsuranceDbContext db) : IPolicyRepository
{
    private const string UniqueViolation = "23505";

    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default) =>
        db.Policies.AsNoTracking().FirstOrDefaultAsync(
            p => EF.Functions.ILike(p.PolicyNumber, EscapeLikePattern(policyNumber)),
            cancellationToken);

    public async Task<PagedResult<Policy>> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        IQueryable<Policy> query = db.Policies.AsNoTracking();

        if (criteria.Query is { } text)
        {
            var pattern = $"%{EscapeLikePattern(text)}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.PolicyNumber, pattern) ||
                EF.Functions.ILike(p.PolicyHolderName, pattern) ||
                EF.Functions.ILike(p.PolicyHolderEmail, pattern));
        }

        if (criteria.Type is { } type)
        {
            query = query.Where(p => p.Type == type);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(p => p.Status == status);
        }

        if (criteria.EffectiveFrom is { } from)
        {
            query = query.Where(p => p.EffectiveDate >= from);
        }

        if (criteria.EffectiveTo is { } to)
        {
            query = query.Where(p => p.EffectiveDate <= to);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var page = await query
            .OrderBy(p => p.PolicyNumber)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Policy>(page, criteria.Page, criteria.PageSize, totalCount);
    }

    public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        db.Policies.Add(policy);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolation })
        {
            throw new InvalidOperationException(
                $"A policy with id {policy.Id} or number {policy.PolicyNumber} already exists.", ex);
        }
        finally
        {
            db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        db.Policies.Update(policy);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // No row matched the id: the policy was deleted after it was read.
            return false;
        }
        finally
        {
            db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Policies.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken) > 0;

    /// <summary>Escapes LIKE wildcards so user text is matched literally (backslash is the default escape).</summary>
    private static string EscapeLikePattern(string value) =>
        value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}
