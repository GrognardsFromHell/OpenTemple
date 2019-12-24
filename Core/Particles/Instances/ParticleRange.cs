namespace OpenTemple.Core.Particles.Instances
{
  public struct ParticleRange
{
  private readonly int _start;
  private readonly int _end; // Exclusive

  public ParticleRange(int start, int end)
  {
    _start = start;
    _end = end;
  }

  public int GetStart()
  {
    return _start;
  }

  public int GetEnd()
  {
    return _end;
  }
}
}