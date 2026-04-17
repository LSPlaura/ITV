namespace ITV.Cache;

public interface ILruCache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    void Actualizar(TKey key);
}