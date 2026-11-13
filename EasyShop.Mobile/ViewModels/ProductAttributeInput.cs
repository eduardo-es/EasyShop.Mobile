using CommunityToolkit.Mvvm.ComponentModel;

namespace EasyShop.Mobile.ViewModels;

/// <summary>
/// Linha temporária do formulário usada para compor um atributo dinâmico
/// antes de persistir o produto.
/// </summary>
public partial class ProductAttributeInput : ObservableObject
{
    public ProductAttributeInput()
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    [ObservableProperty]
    public partial string Key { get; set; }

    [ObservableProperty]
    public partial string Value { get; set; }
}
