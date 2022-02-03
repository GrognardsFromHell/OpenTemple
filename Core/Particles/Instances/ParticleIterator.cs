namespace OpenTemple.Core.Particles.Instances;

public struct ParticleIterator {

    private int _index;
    private readonly int _end;
    private readonly int _size;

    public ParticleIterator(int start, int end, int size)
    {
        _index = start;
        _end = end;
        _size = size;
    }

    public bool HasNext() { return _index != _end; }

    public int Next() {
        var result = _index++;
        if (_index == _size) {
            _index = 0;
        }
        return result;
    }

}