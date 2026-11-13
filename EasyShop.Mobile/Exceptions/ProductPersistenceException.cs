namespace EasyShop.Mobile.Exceptions;

/// <summary>
/// Exceção lançada quando o banco NoSQL local não consegue concluir uma operação.
/// </summary>
public sealed class ProductPersistenceException : Exception
{
    public ProductPersistenceException(string message)
        : base(message)
    {
    }

    public ProductPersistenceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
