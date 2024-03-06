namespace Contracts.Services;

public interface ISessionContainer<TKey, TValue>
{
    TKey AddAuthentication(TValue valueId);
    void RemoveAuthentication(TKey sessionId);
    bool GetAuthentication(TKey sessionId, out TValue valueId);
}