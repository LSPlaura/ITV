public interface ICache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void Agregar(TKey key, TValue value);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    TValue? Obtener(TKey key);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Borrar(TKey key);
    
    /// <summary>
    /// 
    /// </summary>
    void MostrarCache();
}