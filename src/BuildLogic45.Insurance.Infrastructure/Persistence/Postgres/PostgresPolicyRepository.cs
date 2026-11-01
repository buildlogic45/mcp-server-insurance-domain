using BuildLogic45.Insurance.Core.Common;
using BuildLogic45.Insurance.Core.Policies;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PolicyRow = BuildLogic45.Insurance.Infrastructure.Persistence.Postgres.Entities.Policy;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres;

/// <summary>
/// PostgreSQL-backed policy store over the scaffolded <see cref="InsuranceDbContext"/>. The schema is
/// owned by the scripts in <c>database/postgres</c>; this class never creates or alters it.
/// </summary>
public sealed class PostgresPolicyRepository(InsuranceDbContext db) : IPolicyRepository
{
    private const string UniqueViolation = "23505";

    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return row?.ToDomain();
    }

    public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        var pattern = EscapeLikePattern(policyNumber);
        var row = await db.Policies.AsNoTracking()
            .FirstOrDefaultAsync(p => EF.Functions.ILike(p.PolicyNumber, pattern), cancellationToken);
        return row?.ToDomain();
    }

    public async Task<PagedResult<Policy>> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        IQueryable<PolicyRow> query = db.Policies.AsNoTracking();

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
            var value = type.ToString();
            query = query.Where(p => p.Type == value);
        }

        if (criteria.Status is { } status)
        {
            var value = status.ToString();
            query = query.Where(p => p.Status == value);
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
        var rows = await query
            .OrderBy(p => p.PolicyNumber)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Policy>(rows.Select(r => r.ToDomain()).ToList(), criteria.Page, criteria.PageSize, totalCount);
    }

    public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        db.Policies.Add(policy.ToRow());
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
        db.Policies.Update(policy.ToRow());
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
