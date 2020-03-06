namespace IronBug.DomainValidation
{
    public class Rule
    {
        public ISpecification Specification { get; }
        public string ErrorMessage { get; }

        public Rule(ISpecification specification, string errorMessage)
        {
            Specification = specification;
            ErrorMessage = errorMessage;
        }
    }
}
