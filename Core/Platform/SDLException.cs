using System;
using SDL2;

namespace OpenTemple.Core.Platform;

public class SDLException : Exception
{
    public SDLException() : this(SDL.SDL_GetError())
    {
    }

    public SDLException(string? message) : base(message + ": " + SDL.SDL_GetError())
    {
    }

    public SDLException(string? message, Exception? innerException) : base(message + ": " + SDL.SDL_GetError(), innerException)
    {
    }
}