namespace Contracts.ModelContracts;

public interface IFilter
{
    public string? FilterBy { get; }
    public string? FilterValue { get; }
}