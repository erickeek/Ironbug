using System.Collections.Generic;
using System.Linq;

namespace IronBug.DomainValidation
{
    public class ValidationResult
    {
        public readonly List<ValidationError> Errors = new List<ValidationError>();

        public string Message => Errors.FirstOrDefault()?.Message;

        public bool IsValid => Errors.Count == 0;

        public void Add(ValidationError error)
        {
            Errors.Add(error);
        }
    }
}