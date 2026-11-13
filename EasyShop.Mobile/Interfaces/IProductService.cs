using EasyShop.Mobile.Models;

namespace EasyShop.Mobile.Interfaces;

/// <summary>
/// Contrato da camada de negócio do catálogo de produtos.
/// </summary>
public interface IProductService
{
    /// <summary>Lista os produtos do catálogo.</summary>
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>Valida e persiste um novo produto.</summary>
    Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
}
