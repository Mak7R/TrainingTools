namespace Contracts.Services;

public interface ISessionContainer<TKey, TValue>
{
    TKey AddAuthorization(TValue valueId);
    void RemoveAuthorization(TKey sessionId);
    bool FindAuthorization(TKey sessionId, out TValue valueId);
}