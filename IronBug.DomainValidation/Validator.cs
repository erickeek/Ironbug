using System.Collections.Generic;

namespace IronBug.DomainValidation
{
    public abstract class Validator
    {
        private readonly HashSet<Rule> _rules = new HashSet<Rule>();

        public ValidationResult ValidateStopWhenError() => Validate(true);

        public ValidationResult Validate(bool stopWhenError = false)
        {
            var validationResult = new ValidationResult();
            foreach (var rule in _rules)
            {
                var specification = rule.Specification;

                if (!specification.IsSatisfiedBy())
                    validationResult.Add(new ValidationError(rule.ErrorMessage ?? specification.ErrorMessage()));

                if (!validationResult.IsValid && stopWhenError) break;
            }
            return validationResult;
        }

        protected void Add(ISpecification specification, string overrideError = null)
        {
            _rules.Add(new Rule(specification, overrideError));
        }
    }
}