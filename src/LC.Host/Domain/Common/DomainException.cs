namespace LC.Host.Domain.Common;

/// <summary>
/// Нарушение бизнес-правила (инварианта) домена.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
        {
            throw new DomainException(message);
        }
    }
}
