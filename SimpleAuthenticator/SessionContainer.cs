using Contracts.Services;

namespace SimpleAuthorizer;

public class SessionContainer : ISessionContainer<Guid, Guid>
{
    protected List<SessionRecord> Authentications { get; } = new ();

    public Guid AddAuthentication(Guid valueId)
    {
        var sessionRecord = new SessionRecord(valueId);
        Authentications.Add(sessionRecord);
        return sessionRecord.SessionId;
    }

    public void RemoveAuthentication(Guid sessionId)
    {
        Authentications.RemoveAll(s => s.SessionId == sessionId);
    }

    public bool GetAuthentication(Guid sessionId, out Guid valueId)
    {
        var id = Authentications.FirstOrDefault(s => s.SessionId == sessionId)?.ValueId;
        
        if (id.HasValue)
        {
            valueId = id.Value;
            return true;
        }
        
        valueId = default;
        return false;
    }
}