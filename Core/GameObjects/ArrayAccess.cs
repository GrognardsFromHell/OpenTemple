using System.Collections.Generic;



namespace OpenTemple.Core.GameObjects;

/// <summary>
/// Provides readonly access to a backing sparse array storage in a game object,
/// and transparently works if the array was not present (because it was empty, for example).
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct ArrayAccess<T> where T : struct
{
    private readonly GameObject _gameObject;

    private readonly IReadOnlyList<T>? _arrayRef;

    public ArrayAccess(GameObject gameObject, IReadOnlyList<T>? arrayRef)
    {
        _gameObject = gameObject;
        _arrayRef = arrayRef;
    }

    public T this[int index] => _arrayRef?[index] ?? default;

    public int Count => _arrayRef?.Count ?? 0;
}