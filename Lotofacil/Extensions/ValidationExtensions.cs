using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lotofacil.Web.Extensions
{
    public static class ValidationExtensions
    {
        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

        }
    }
}
/*Esse método percorre a lista de Errors no ValidationResult e adiciona cada erro ao ModelState, 
 * com a propriedade PropertyName como chave e a ErrorMessage como valor.
 O ASP.NET usa o ModelState para armazenar mensagens de erro de validação e exibi-las na view*/