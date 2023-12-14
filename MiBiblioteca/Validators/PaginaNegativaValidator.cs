using System.ComponentModel.DataAnnotations;

namespace MiLibreria.Validators
{
    public class PaginaNegativaValidator: ValidationAttribute
    {
        public PaginaNegativaValidator() { }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return new ValidationResult($"El numero de paginas no puede ser nullo");
            }

            int? pagina = value as int?;

            if (pagina < 0)
            {
                return new ValidationResult($"La pagina no puede ser negativa");
            }

            return ValidationResult.Success;
        }
    }
}
