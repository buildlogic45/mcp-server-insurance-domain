namespace BuildLogic45.Insurance.Core.Common;

/// <summary>Raised when a request violates a business rule. The message is safe to show to callers.</summary>
public sealed class DomainValidationException(string message) : Exception(message);
