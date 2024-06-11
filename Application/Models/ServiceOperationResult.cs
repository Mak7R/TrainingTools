using Domain.Models;

namespace Application.Models;

public class ServiceOperationResult : OperationResult
{
    public override bool IsSuccessful { get; }
    public override object? ResultObject { get; }
    public override IEnumerable<string> Errors { get; }
    
    
}