namespace EasyShop.Mobile.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de negócio do catálogo de produtos é violada.
/// </summary>
public sealed class ProductValidationException : Exception
{
    public ProductValidationException(string message)
        : base(message)
    {
    }

    public ProductValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
