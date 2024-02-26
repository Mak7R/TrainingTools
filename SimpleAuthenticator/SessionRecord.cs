
namespace SimpleAuthorizer;

public class SessionRecord
{
    public SessionRecord(Guid valueId)
    {
        SessionId = Guid.NewGuid();
        ValueId = valueId;
        AuthDateTime = DateTime.Now;
    }
    public Guid SessionId { get; }
    public Guid ValueId { get; }
    public DateTime AuthDateTime { get; }
}