public class TripleDictionary<T1, T2, T3>
{
    private Dictionary<(T1, T2), T3> innerDict = new();

    public void Add(T1 key1, T2 key2, T3 value)
    {
        innerDict.Add((key1, key2), value);
    }

    public bool TryGetValue(T1 key1, T2 key2, out T3 value)
    {
        return innerDict.TryGetValue((key1, key2), out value);
    }

    public T3 this[T1 key1, T2 key2]
    {
        get => innerDict[(key1, key2)];
        set => innerDict[(key1, key2)] = value;
    }
}
