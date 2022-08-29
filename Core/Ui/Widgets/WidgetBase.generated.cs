#nullable enable
using System;
using System.Collections.Immutable;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.Widgets;

[global::System.CodeDom.Compiler.GeneratedCode("Core.WidgetEventGenerator", "2022-08-29T22:08:34.9484465Z")]
public partial class WidgetBase
{
    public delegate void EventHandler<in T>(T e) where T : UiEvent;

    #region MouseDown

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersMouseDown = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnMouseDown 
    {
        add => AddMouseDownListener(value);
        remove => RemoveMouseDownListener(value);
    }
    
    public void AddMouseDownListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseDown = _listenersMouseDown.Add(new (handler));
    }
    
    public void RemoveMouseDownListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseDown = _listenersMouseDown.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddMouseDownListener.
    /// </summary>
    protected virtual void HandleMouseDown(MouseEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultMouseDownAction(MouseEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnMouseDown
    /// </summary>
    internal void DispatchMouseDown(MouseEvent e)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersMouseDown;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersMouseDown = target._listenersMouseDown.Remove(listener); 
                }
                listener.Listener(e);
                if (e.IsImmediatePropagationStopped)
                {
                    break;
                }
            }
            
            // Call the implicitly registered event listener if propagation wasn't stopped
            if (!e.IsImmediatePropagationStopped)
            {
                target.HandleMouseDown(e);
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultMouseDownAction(e);
        }
    }
    #endregion
    
    #region MouseUp

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersMouseUp = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnMouseUp 
    {
        add => AddMouseUpListener(value);
        remove => RemoveMouseUpListener(value);
    }
    
    public void AddMouseUpListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseUp = _listenersMouseUp.Add(new (handler));
    }
    
    public void RemoveMouseUpListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseUp = _listenersMouseUp.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddMouseUpListener.
    /// </summary>
    protected virtual void HandleMouseUp(MouseEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultMouseUpAction(MouseEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnMouseUp
    /// </summary>
    internal void DispatchMouseUp(MouseEvent e)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersMouseUp;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersMouseUp = target._listenersMouseUp.Remove(listener); 
                }
                listener.Listener(e);
                if (e.IsImmediatePropagationStopped)
                {
                    break;
                }
            }
            
            // Call the implicitly registered event listener if propagation wasn't stopped
            if (!e.IsImmediatePropagationStopped)
            {
                target.HandleMouseUp(e);
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultMouseUpAction(e);
        }
    }
    #endregion
    
    #region MouseEnter

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersMouseEnter = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnMouseEnter 
    {
        add => AddMouseEnterListener(value);
        remove => RemoveMouseEnterListener(value);
    }
    
    public void AddMouseEnterListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseEnter = _listenersMouseEnter.Add(new (handler));
    }
    
    public void RemoveMouseEnterListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseEnter = _listenersMouseEnter.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddMouseEnterListener.
    /// </summary>
    protected virtual void HandleMouseEnter(MouseEvent e)
    {
    }

    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnMouseEnter
    /// </summary>
    internal void DispatchMouseEnter(MouseEvent e)
    {

        // Dispatch to additional registered handlers first
        var listeners = this._listenersMouseEnter;
        foreach (var listener in listeners)
        {
            // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
            // remove it again.
            if (listener.Once)
            {
                this._listenersMouseEnter = this._listenersMouseEnter.Remove(listener); 
            }
            listener.Listener(e);
            if (e.IsImmediatePropagationStopped)
            {
                break;
            }
        }
        
        // Call the implicitly registered event listener if propagation wasn't stopped
        if (!e.IsImmediatePropagationStopped)
        {
            this.HandleMouseEnter(e);
        }
        
    }
    #endregion
    
    #region MouseLeave

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersMouseLeave = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnMouseLeave 
    {
        add => AddMouseLeaveListener(value);
        remove => RemoveMouseLeaveListener(value);
    }
    
    public void AddMouseLeaveListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseLeave = _listenersMouseLeave.Add(new (handler));
    }
    
    public void RemoveMouseLeaveListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseLeave = _listenersMouseLeave.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddMouseLeaveListener.
    /// </summary>
    protected virtual void HandleMouseLeave(MouseEvent e)
    {
    }

    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnMouseLeave
    /// </summary>
    internal void DispatchMouseLeave(MouseEvent e)
    {

        // Dispatch to additional registered handlers first
        var listeners = this._listenersMouseLeave;
        foreach (var listener in listeners)
        {
            // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
            // remove it again.
            if (listener.Once)
            {
                this._listenersMouseLeave = this._listenersMouseLeave.Remove(listener); 
            }
            listener.Listener(e);
            if (e.IsImmediatePropagationStopped)
            {
                break;
            }
        }
        
        // Call the implicitly registered event listener if propagation wasn't stopped
        if (!e.IsImmediatePropagationStopped)
        {
            this.HandleMouseLeave(e);
        }
        
    }
    #endregion
    
    #region MouseMove

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersMouseMove = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnMouseMove 
    {
        add => AddMouseMoveListener(value);
        remove => RemoveMouseMoveListener(value);
    }
    
    public void AddMouseMoveListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseMove = _listenersMouseMove.Add(new (handler));
    }
    
    public void RemoveMouseMoveListener(EventHandler<MouseEvent> handler)
    {
        _listenersMouseMove = _listenersMouseMove.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddMouseMoveListener.
    /// </summary>
    protected virtual void HandleMouseMove(MouseEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultMouseMoveAction(MouseEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnMouseMove
    /// </summary>
    internal void DispatchMouseMove(MouseEvent e)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersMouseMove;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersMouseMove = target._listenersMouseMove.Remove(listener); 
                }
                listener.Listener(e);
                if (e.IsImmediatePropagationStopped)
                {
                    break;
                }
            }
            
            // Call the implicitly registered event listener if propagation wasn't stopped
            if (!e.IsImmediatePropagationStopped)
            {
                target.HandleMouseMove(e);
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultMouseMoveAction(e);
        }
    }
    #endregion
    
    #region MouseWheel

    private ImmutableList<RegisteredListener<WheelEvent>> _listenersMouseWheel = ImmutableList<RegisteredListener<WheelEvent>>.Empty;
    
    public event EventHandler<WheelEvent>? OnMouseWheel 
    {
        add => AddMouseWheelListener(value);
        remove => RemoveMouseWheelListener(value);
    }
    
    public void AddMouseWheelListener(EventHandler<WheelEvent> handler)
    {
        _listenersMouseWheel = _listenersMouseWheel.Add(new (handler));
    }
    
    public void RemoveMouseWheelListener(EventHandler<WheelEvent> handler)
    {
        _listenersMouseWheel = _listenersMouseWheel.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddMouseWheelListener.
    /// </summary>
    protected virtual void HandleMouseWheel(WheelEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultMouseWheelAction(WheelEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnMouseWheel
    /// </summary>
    internal void DispatchMouseWheel(WheelEvent e)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersMouseWheel;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersMouseWheel = target._listenersMouseWheel.Remove(listener); 
                }
                listener.Listener(e);
                if (e.IsImmediatePropagationStopped)
                {
                    break;
                }
            }
            
            // Call the implicitly registered event listener if propagation wasn't stopped
            if (!e.IsImmediatePropagationStopped)
            {
                target.HandleMouseWheel(e);
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultMouseWheelAction(e);
        }
    }
    #endregion
    
    #region Click

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersClick = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnClick 
    {
        add => AddClickListener(value);
        remove => RemoveClickListener(value);
    }
    
    public void AddClickListener(EventHandler<MouseEvent> handler)
    {
        _listenersClick = _listenersClick.Add(new (handler));
    }
    
    public void RemoveClickListener(EventHandler<MouseEvent> handler)
    {
        _listenersClick = _listenersClick.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddClickListener.
    /// </summary>
    protected virtual void HandleClick(MouseEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultClickAction(MouseEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnClick
    /// </summary>
    internal void DispatchClick(MouseEvent e)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersClick;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersClick = target._listenersClick.Remove(listener); 
                }
                listener.Listener(e);
                if (e.IsImmediatePropagationStopped)
                {
                    break;
                }
            }
            
            // Call the implicitly registered event listener if propagation wasn't stopped
            if (!e.IsImmediatePropagationStopped)
            {
                target.HandleClick(e);
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultClickAction(e);
        }
    }
    #endregion
    
    #region TextInput

    private ImmutableList<RegisteredListener<TextInputEvent>> _listenersTextInput = ImmutableList<RegisteredListener<TextInputEvent>>.Empty;
    
    public event EventHandler<TextInputEvent>? OnTextInput 
    {
        add => AddTextInputListener(value);
        remove => RemoveTextInputListener(value);
    }
    
    public void AddTextInputListener(EventHandler<TextInputEvent> handler)
    {
        _listenersTextInput = _listenersTextInput.Add(new (handler));
    }
    
    public void RemoveTextInputListener(EventHandler<TextInputEvent> handler)
    {
        _listenersTextInput = _listenersTextInput.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddTextInputListener.
    /// </summary>
    protected virtual void HandleTextInput(TextInputEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultTextInputAction(TextInputEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnTextInput
    /// </summary>
    internal void DispatchTextInput(TextInputEvent e)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersTextInput;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersTextInput = target._listenersTextInput.Remove(listener); 
                }
                listener.Listener(e);
                if (e.IsImmediatePropagationStopped)
                {
                    break;
                }
            }
            
            // Call the implicitly registered event listener if propagation wasn't stopped
            if (!e.IsImmediatePropagationStopped)
            {
                target.HandleTextInput(e);
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultTextInputAction(e);
        }
    }
    #endregion
    
    #region KeyDown

    private ImmutableList<RegisteredListener<KeyboardEvent>> _listenersKeyDown = ImmutableList<RegisteredListener<KeyboardEvent>>.Empty;
    
    public event EventHandler<KeyboardEvent>? OnKeyDown 
    {
        add => AddKeyDownListener(value);
        remove => RemoveKeyDownListener(value);
    }
    
    public void AddKeyDownListener(EventHandler<KeyboardEvent> handler)
    {
        _listenersKeyDown = _listenersKeyDown.Add(new (handler));
    }
    
    public void RemoveKeyDownListener(EventHandler<KeyboardEvent> handler)
    {
        _listenersKeyDown = _listenersKeyDown.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddKeyDownListener.
    /// </summary>
    protected virtual void HandleKeyDown(KeyboardEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultKeyDownAction(KeyboardEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnKeyDown
    /// </summary>
    internal void DispatchKeyDown(KeyboardEvent e)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersKeyDown;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersKeyDown = target._listenersKeyDown.Remove(listener); 
                }
                listener.Listener(e);
                if (e.IsImmediatePropagationStopped)
                {
                    break;
                }
            }
            
            // Call the implicitly registered event listener if propagation wasn't stopped
            if (!e.IsImmediatePropagationStopped)
            {
                target.HandleKeyDown(e);
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultKeyDownAction(e);
        }
    }
    #endregion
    
    #region KeyUp

    private ImmutableList<RegisteredListener<KeyboardEvent>> _listenersKeyUp = ImmutableList<RegisteredListener<KeyboardEvent>>.Empty;
    
    public event EventHandler<KeyboardEvent>? OnKeyUp 
    {
        add => AddKeyUpListener(value);
        remove => RemoveKeyUpListener(value);
    }
    
    public void AddKeyUpListener(EventHandler<KeyboardEvent> handler)
    {
        _listenersKeyUp = _listenersKeyUp.Add(new (handler));
    }
    
    public void RemoveKeyUpListener(EventHandler<KeyboardEvent> handler)
    {
        _listenersKeyUp = _listenersKeyUp.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddKeyUpListener.
    /// </summary>
    protected virtual void HandleKeyUp(KeyboardEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultKeyUpAction(KeyboardEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnKeyUp
    /// </summary>
    internal void DispatchKeyUp(KeyboardEvent e)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersKeyUp;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersKeyUp = target._listenersKeyUp.Remove(listener); 
                }
                listener.Listener(e);
                if (e.IsImmediatePropagationStopped)
                {
                    break;
                }
            }
            
            // Call the implicitly registered event listener if propagation wasn't stopped
            if (!e.IsImmediatePropagationStopped)
            {
                target.HandleKeyUp(e);
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultKeyUpAction(e);
        }
    }
    #endregion
    
    
    private readonly record struct RegisteredListener<T>(EventHandler<T> Listener, bool Once = false) where T : UiEvent;
}
