using System;
using System.Collections.Generic;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres.Entities;

public partial class Policy
{
    public Guid Id { get; set; }

    public string PolicyNumber { get; set; } = null!;

    public string PolicyHolderName { get; set; } = null!;

    public string PolicyHolderEmail { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateOnly EffectiveDate { get; set; }

    public DateOnly ExpirationDate { get; set; }

    public decimal Premium { get; set; }

    public decimal CoverageAmount { get; set; }

    public decimal Deductible { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
