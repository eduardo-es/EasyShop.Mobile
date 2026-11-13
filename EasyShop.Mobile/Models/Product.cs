using LiteDB;

namespace EasyShop.Mobile.Models;

/// <summary>
/// Documento NoSQL que representa um produto do catálogo.
/// A propriedade <see cref="Attributes"/> permite armazenar qualquer combinação
/// de atributos (cor, tamanho, voltagem etc.) sem alterar o esquema do banco.
/// </summary>
public sealed class Product
{
    /// <summary>Identificador gerado automaticamente pelo LiteDB.</summary>
    [BsonId]
    public int Id { get; set; }

    /// <summary>Nome comercial do produto.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Preço unitário do produto.</summary>
    public decimal Price { get; set; }

    /// <summary>Atributos livres do produto, persistidos como documento BSON.</summary>
    public Dictionary<string, object> Attributes { get; set; } = new();

    /// <summary>
    /// Projeção somente leitura usada pela interface para exibir os atributos
    /// dinâmicos. Não é persistida pelo LiteDB.
    /// </summary>
    [BsonIgnore]
    public IReadOnlyList<ProductAttributeItem> AttributeItems
    {
        get
        {
            if (Attributes is null || Attributes.Count == 0)
            {
                return Array.Empty<ProductAttributeItem>();
            }

            return Attributes
                .Select(attribute => new ProductAttributeItem(
                    attribute.Key,
                    attribute.Value?.ToString() ?? string.Empty))
                .ToArray();
        }
    }
}

/// <summary>Item de exibição de um atributo dinâmico do produto.</summary>
public sealed record ProductAttributeItem(string Key, string Value);
