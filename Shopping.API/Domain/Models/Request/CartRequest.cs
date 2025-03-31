using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shopping.API.Domain.Models.Request
{
    public record CartRequest
    {
        [Required(ErrorMessage = "The PayerDocument is required.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "The PayerDocument must be exactly 11 digits.")]
        [CPFValidation(ErrorMessage = "The PayerDocument must be a valid CPF.")]
        public string PayerDocument { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CartRequest(string payerDocument)
        {
            PayerDocument = payerDocument;
            CreatedAt = DateTime.UtcNow;
        }
    }

    public class CPFValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is not string cpf || string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = Regex.Replace(cpf, "[^0-9]", "");

            if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
                return false;

            return ValidateCPFCheckDigits(cpf);
        }

        private bool ValidateCPFCheckDigits(string cpf)
        {
            int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int sum = tempCpf.Select((t, i) => (t - '0') * multiplier1[i]).Sum();
            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            tempCpf += digit1;
            sum = tempCpf.Select((t, i) => (t - '0') * multiplier2[i]).Sum();
            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            return cpf.EndsWith(digit1.ToString() + digit2.ToString());
        }
    }
}
