namespace ProjectScene.Application.Exceptions;

public class DuplicateResourceException : Exception
{
    // Exceção usada quando a regra de unicidade é violada.
    public DuplicateResourceException(string message) : base(message)
    {
    }
}
