using System.Globalization;
using EasyShop.Mobile.Exceptions;
using EasyShop.Mobile.Interfaces;
using EasyShop.Mobile.Models;

namespace EasyShop.Mobile.Services;

/// <summary>
/// Regras de negócio do catálogo. É o único ponto autorizado a validar
/// e orquestrar a persistência de produtos.
/// </summary>
public sealed class ProductService : IProductService
{
    private const int MaxNameLength = 120;
    private const int MaxAttributes = 20;
    private const int MaxAttributeKeyLength = 50;
    private const int MaxAttributeValueLength = 250;
    private const decimal MaxPrice = 999_999_999.99m;

    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAll(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        var validatedProduct = Validate(product);
        return _repository.Insert(validatedProduct, cancellationToken);
    }

    private static Product Validate(Product? product)
    {
        if (product is null)
        {
            throw new ProductValidationException("Os dados do produto não foram informados.");
        }

        var name = product.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            throw new ProductValidationException("O nome do produto é obrigatório.");
        }

        if (name.Length > MaxNameLength)
        {
            throw new ProductValidationException(
                $"O nome do produto deve ter no máximo {MaxNameLength} caracteres.");
        }

        if (product.Price <= 0m)
        {
            throw new ProductValidationException("O preço do produto deve ser maior que zero.");
        }

        if (product.Price > MaxPrice)
        {
            throw new ProductValidationException(
                $"O preço do produto deve ser menor ou igual a {MaxPrice.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"))}.");
        }

        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (product.Attributes is not null)
        {
            if (product.Attributes.Count > MaxAttributes)
            {
                throw new ProductValidationException(
                    $"O produto pode ter no máximo {MaxAttributes} atributos dinâmicos.");
            }

            foreach (var (rawKey, rawValue) in product.Attributes)
            {
                var key = rawKey?.Trim() ?? string.Empty;

                if (key.Length == 0)
                {
                    throw new ProductValidationException(
                        "Todo atributo dinâmico precisa ter uma chave preenchida.");
                }

                if (key.Length > MaxAttributeKeyLength)
                {
                    throw new ProductValidationException(
                        $"A chave do atributo \"{key}\" deve ter no máximo {MaxAttributeKeyLength} caracteres.");
                }

                var value = rawValue is null
                    ? string.Empty
                    : Convert.ToString(rawValue, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;

                if (value.Length == 0)
                {
                    throw new ProductValidationException(
                        $"O atributo \"{key}\" precisa ter um valor preenchido.");
                }

                if (value.Length > MaxAttributeValueLength)
                {
                    throw new ProductValidationException(
                        $"O valor do atributo \"{key}\" deve ter no máximo {MaxAttributeValueLength} caracteres.");
                }

                if (!attributes.TryAdd(key, value))
                {
                    throw new ProductValidationException(
                        $"O atributo \"{key}\" foi informado mais de uma vez.");
                }
            }
        }

        var normalizedPrice = Math.Round(product.Price, 2, MidpointRounding.AwayFromZero);

        if (normalizedPrice <= 0m)
        {
            throw new ProductValidationException(
                "O preço do produto deve ser de no mínimo R$ 0,01.");
        }

        return new Product
        {
            Name = name,
            Price = normalizedPrice,
            Attributes = attributes
        };
    }
}
