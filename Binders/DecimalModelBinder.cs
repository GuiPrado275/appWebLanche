using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace SaoJudasLanches.Web.Binders;

/// <summary>
/// Faz o servidor entender preço com vírgula (29,90) ou ponto (29.90)
/// independente da cultura configurada no servidor.
/// </summary>
public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
            return Task.CompletedTask;

        var rawValue = valueResult.FirstValue?.Trim();
        if (string.IsNullOrEmpty(rawValue))
            return Task.CompletedTask;

        // Normaliza: troca vírgula por ponto para o parse
        var normalizado = rawValue.Replace(',', '.');

        if (decimal.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultado))
        {
            bindingContext.Result = ModelBindingResult.Success(resultado);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Preço inválido.");
        }

        return Task.CompletedTask;
    }
}

public class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
            return new DecimalModelBinder();
        return null;
    }
}
