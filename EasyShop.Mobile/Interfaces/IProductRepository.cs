using EasyShop.Mobile.Models;

namespace EasyShop.Mobile.Interfaces;

/// <summary>
/// Contrato da camada de acesso a dados do catálogo (LiteDB).
/// </summary>
public interface IProductRepository
{
    /// <summary>Insere um novo documento de produto e devolve o documento com o Id gerado.</summary>
    Task<Product> Insert(Product product, CancellationToken cancellationToken = default);

    /// <summary>Retorna todos os produtos persistidos, do mais recente para o mais antigo.</summary>
    Task<IReadOnlyList<Product>> GetAll(CancellationToken cancellationToken = default);
}
