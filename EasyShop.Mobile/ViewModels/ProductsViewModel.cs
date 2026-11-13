using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyShop.Mobile.Exceptions;
using EasyShop.Mobile.Interfaces;
using EasyShop.Mobile.Models;
using Microsoft.Extensions.Logging;

namespace EasyShop.Mobile.ViewModels;

/// <summary>
/// ViewModel da tela de catálogo. Concentra o estado da tela e as interações
/// do usuário, delegando validações e persistência para a camada de negócio.
/// </summary>
public partial class ProductsViewModel : ObservableObject
{
    private const int MaxAttributes = 20;

    private readonly IProductService _productService;
    private readonly ILogger<ProductsViewModel> _logger;

    public ProductsViewModel(
        IProductService productService,
        ILogger<ProductsViewModel> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        StatusMessage = "Preencha os dados e salve o primeiro produto.";
        NewProductName = string.Empty;
        NewProductPrice = string.Empty;
        Attributes.Add(new ProductAttributeInput());
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProductCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    public partial string StatusMessage { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string NewProductName { get; set; }

    [ObservableProperty]
    public partial string NewProductPrice { get; set; }

    /// <summary>Produtos carregados do banco NoSQL.</summary>
    public ObservableCollection<Product> Products { get; } = new();

    /// <summary>Atributos temporários que serão gravados junto ao novo produto.</summary>
    public ObservableCollection<ProductAttributeInput> Attributes { get; } = new();

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    [RelayCommand]
    private async Task LoadProductsAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var products = await _productService.GetProductsAsync(cancellationToken);

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            SetStatus(
                Products.Count == 0
                    ? "Nenhum produto cadastrado ainda."
                    : $"{Products.Count} {(Products.Count == 1 ? "produto" : "produtos")} no catálogo.",
                isError: false);
        }
        catch (OperationCanceledException)
        {
            // Cancelamento solicitado pelo ciclo de vida da tela; nenhum aviso é necessário.
        }
        catch (ProductPersistenceException ex)
        {
            _logger.LogError(ex, "Falha de persistência ao carregar o catálogo.");
            SetStatus(ex.Message, isError: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha inesperada ao carregar o catálogo.");
            SetStatus("Não foi possível carregar os produtos. Tente novamente.", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveProduct))]
    private async Task SaveProductAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        if (!TryParsePrice(NewProductPrice, out var price))
        {
            SetStatus("Informe um preço válido (ex.: 199,90).", isError: true);
            return;
        }

        if (price <= 0m)
        {
            SetStatus("O preço do produto deve ser maior que zero.", isError: true);
            return;
        }

        if (!TryBuildAttributes(out var attributes, out var attributeError))
        {
            SetStatus(attributeError!, isError: true);
            return;
        }

        IsBusy = true;
        SetStatus("Salvando produto...", isError: false);

        try
        {
            var product = new Product
            {
                Name = NewProductName,
                Price = price,
                Attributes = attributes
            };

            var createdProduct = await _productService.CreateProductAsync(product, cancellationToken);

            Products.Insert(0, createdProduct);
            ResetForm();
            SetStatus($"Produto \"{createdProduct.Name}\" salvo com sucesso.", isError: false);
        }
        catch (OperationCanceledException)
        {
            SetStatus("Operação cancelada.", isError: false);
        }
        catch (ProductValidationException ex)
        {
            SetStatus(ex.Message, isError: true);
        }
        catch (ProductPersistenceException ex)
        {
            _logger.LogError(ex, "Falha de persistência ao salvar o produto.");
            SetStatus(ex.Message, isError: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha inesperada ao salvar o produto.");
            SetStatus("Não foi possível salvar o produto. Tente novamente.", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddAttribute()
    {
        if (IsBusy)
        {
            return;
        }

        if (Attributes.Count >= MaxAttributes)
        {
            SetStatus($"O produto pode ter no máximo {MaxAttributes} atributos dinâmicos.", isError: true);
            return;
        }

        Attributes.Add(new ProductAttributeInput());
    }

    [RelayCommand]
    private void RemoveAttribute(ProductAttributeInput? attribute)
    {
        if (IsBusy || attribute is null)
        {
            return;
        }

        Attributes.Remove(attribute);

        if (Attributes.Count == 0)
        {
            Attributes.Add(new ProductAttributeInput());
        }
    }

    private bool CanSaveProduct() => !IsBusy;

    private bool TryBuildAttributes(
        out Dictionary<string, object> attributes,
        out string? error)
    {
        attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        error = null;

        foreach (var input in Attributes)
        {
            var key = input.Key?.Trim() ?? string.Empty;
            var value = input.Value?.Trim() ?? string.Empty;

            // Linhas completamente vazias são ignoradas.
            if (key.Length == 0 && value.Length == 0)
            {
                continue;
            }

            if (key.Length == 0)
            {
                error = "Todo atributo dinâmico precisa ter uma chave preenchida.";
                return false;
            }

            if (value.Length == 0)
            {
                error = $"Informe o valor do atributo \"{key}\".";
                return false;
            }

            if (!attributes.TryAdd(key, value))
            {
                error = $"O atributo \"{key}\" foi informado mais de uma vez.";
                return false;
            }
        }

        return true;
    }

    private static bool TryParsePrice(string? rawPrice, out decimal price)
    {
        price = 0m;

        if (string.IsNullOrWhiteSpace(rawPrice))
        {
            return false;
        }

        var normalized = rawPrice
            .Trim()
            .Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(" ", string.Empty)
            .Replace("\u00A0", string.Empty);

        if (normalized.Contains(',') && normalized.Contains('.'))
        {
            normalized = normalized.LastIndexOf(',') > normalized.LastIndexOf('.')
                ? normalized.Replace(".", string.Empty).Replace(',', '.')
                : normalized.Replace(",", string.Empty);
        }
        else if (normalized.Contains(','))
        {
            normalized = normalized.Replace(',', '.');
        }
        else if (normalized.Contains('.'))
        {
            var lastDot = normalized.LastIndexOf('.');
            var decimalDigits = normalized.Length - lastDot - 1;
            var dotCount = normalized.Count(character => character == '.');

            // Em pt-BR, "1.234" e "1.000.000" representam milhares. Já "199.90"
            // é aceito como decimal para tolerar teclados com ponto decimal.
            if (dotCount > 1 || decimalDigits == 3)
            {
                normalized = normalized.Replace(".", string.Empty);
            }
        }

        return decimal.TryParse(
            normalized,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out price);
    }

    private void ResetForm()
    {
        NewProductName = string.Empty;
        NewProductPrice = string.Empty;

        Attributes.Clear();
        Attributes.Add(new ProductAttributeInput());
    }

    private void SetStatus(string message, bool isError)
    {
        HasError = isError;
        StatusMessage = message;
    }
}
