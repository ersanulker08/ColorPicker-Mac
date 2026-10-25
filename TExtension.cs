using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace ColorPickerApp;

// XAML'de {local:T Anahtar} yazınca o anahtarın çevirisini bağlar; dil değişince kendiliğinden güncellenir.
public class TExtension : MarkupExtension
{
    public TExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new ReflectionBinding($"[{Key}]")
        {
            Source = Loc.Instance,
            Mode = BindingMode.OneWay
        };
    }
}
