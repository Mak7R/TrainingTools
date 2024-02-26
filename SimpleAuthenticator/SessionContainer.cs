using Contracts;
using Contracts.Services;

namespace SimpleAuthorizer;

public class SessionContainer : ISessionContainer<Guid, Guid>
{
    protected List<SessionRecord> Authorizations { get; } = new ();

    public Guid AddAuthorization(Guid valueId)
    {
        var sessionRecord = new SessionRecord(valueId);
        Authorizations.Add(sessionRecord);
        return sessionRecord.SessionId;
    }

    public void RemoveAuthorization(Guid sessionId)
    {
        Authorizations.RemoveAll(s => s.SessionId == sessionId);
    }

    public bool FindAuthorization(Guid sessionId, out Guid valueId)
    {
        var id = Authorizations.FirstOrDefault(s => s.SessionId == sessionId)?.ValueId;
        
        if (id.HasValue)
        {
            valueId = id.Value;
            return true;
        }
        
        valueId = default;
        return false;
    }
}