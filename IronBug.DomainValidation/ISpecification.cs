namespace IronBug.DomainValidation
{
    public interface ISpecification
    {
        bool IsSatisfiedBy();
        string ErrorMessage();
    }
}