public class TripleDictionary<T1, T2, T3>
{
    private Dictionary<(T1, T2, T3), bool> innerDict = new();

    public void Add(T1 key1, T2 key2, T3 key3)
    {
        innerDict[(key1, key2, key3)] = true;
    }

    // Prüfen, ob ein T1 existiert
    public bool Contains1(T1 key1)
    {
        return innerDict.Keys.Any(k => EqualityComparer<T1>.Default.Equals(k.Item1, key1));
    }

    // Prüfen, ob ein T2 existiert
    public bool Contains2(T2 key2)
    {
        return innerDict.Keys.Any(k => EqualityComparer<T2>.Default.Equals(k.Item2, key2));
    }

    // Prüfen, ob ein T3 existiert
    public bool Contains3(T3 key3)
    {
        return innerDict.Keys.Any(k => EqualityComparer<T3>.Default.Equals(k.Item3, key3));
    }

    // Gib alle T1 zurück, die mit key2 passen
    public IEnumerable<T1> Get1By2(T2 key2)
    {
        return innerDict.Keys
            .Where(k => EqualityComparer<T2>.Default.Equals(k.Item2, key2))
            .Select(k => k.Item1);
    }

    // Gib alle T1 zurück, die mit key3 passen
    public IEnumerable<T1> Get1By3(T3 key3)
    {
        return innerDict.Keys
            .Where(k => EqualityComparer<T3>.Default.Equals(k.Item3, key3))
            .Select(k => k.Item1);
    }

    // Gib alle T2 zurück, die mit key1 passen
    public IEnumerable<T2> Get2By1(T1 key1)
    {
        return innerDict.Keys
            .Where(k => EqualityComparer<T1>.Default.Equals(k.Item1, key1))
            .Select(k => k.Item2);
    }

    // Gib alle T2 zurück, die mit key3 passen
    public IEnumerable<T2> Get2By3(T3 key3)
    {
        return innerDict.Keys
            .Where(k => EqualityComparer<T3>.Default.Equals(k.Item3, key3))
            .Select(k => k.Item2);
    }

    // Gib alle T3 zurück, die mit key1 passen
    public IEnumerable<T3> Get3By1(T1 key1)
    {
        return innerDict.Keys
            .Where(k => EqualityComparer<T1>.Default.Equals(k.Item1, key1))
            .Select(k => k.Item3);
    }

    // Gib alle T3 zurück, die mit key2 passen
    public IEnumerable<T3> Get3By2(T2 key2)
    {
        return innerDict.Keys
            .Where(k => EqualityComparer<T2>.Default.Equals(k.Item2, key2))
            .Select(k => k.Item3);
    }
}