#nullable enable
using System;
using System.Collections.Immutable;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.Widgets;

[global::System.CodeDom.Compiler.GeneratedCode("Core.WidgetEventGenerator", "2022-12-18T23:12:11.8606690Z")]
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
    
    public void AddMouseDownListener(Action handler)
    {
        _listenersMouseDown = _listenersMouseDown.Add(new (e => handler()));
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
    internal void DispatchMouseDown(MouseEvent e, bool localOnly = false)
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
            
            if (localOnly)
            {
                break;
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
    
    public void AddMouseUpListener(Action handler)
    {
        _listenersMouseUp = _listenersMouseUp.Add(new (e => handler()));
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
    internal void DispatchMouseUp(MouseEvent e, bool localOnly = false)
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
            
            if (localOnly)
            {
                break;
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
    
    public void AddMouseEnterListener(Action handler)
    {
        _listenersMouseEnter = _listenersMouseEnter.Add(new (e => handler()));
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
    
    public void AddMouseLeaveListener(Action handler)
    {
        _listenersMouseLeave = _listenersMouseLeave.Add(new (e => handler()));
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
    
    public void AddMouseMoveListener(Action handler)
    {
        _listenersMouseMove = _listenersMouseMove.Add(new (e => handler()));
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
    internal void DispatchMouseMove(MouseEvent e, bool localOnly = false)
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
            
            if (localOnly)
            {
                break;
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
    
    public void AddMouseWheelListener(Action handler)
    {
        _listenersMouseWheel = _listenersMouseWheel.Add(new (e => handler()));
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
    internal void DispatchMouseWheel(WheelEvent e, bool localOnly = false)
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
            
            if (localOnly)
            {
                break;
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
    
    public void AddClickListener(Action handler)
    {
        _listenersClick = _listenersClick.Add(new (e => handler()));
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
    internal void DispatchClick(MouseEvent e, bool localOnly = false)
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
            
            if (localOnly)
            {
                break;
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultClickAction(e);
        }
    }
    #endregion
    
    #region OtherClick

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersOtherClick = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnOtherClick 
    {
        add => AddOtherClickListener(value);
        remove => RemoveOtherClickListener(value);
    }
    
    public void AddOtherClickListener(EventHandler<MouseEvent> handler)
    {
        _listenersOtherClick = _listenersOtherClick.Add(new (handler));
    }
    
    public void AddOtherClickListener(Action handler)
    {
        _listenersOtherClick = _listenersOtherClick.Add(new (e => handler()));
    }
    
    public void RemoveOtherClickListener(EventHandler<MouseEvent> handler)
    {
        _listenersOtherClick = _listenersOtherClick.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddOtherClickListener.
    /// </summary>
    protected virtual void HandleOtherClick(MouseEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultOtherClickAction(MouseEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnOtherClick
    /// </summary>
    internal void DispatchOtherClick(MouseEvent e, bool localOnly = false)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersOtherClick;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersOtherClick = target._listenersOtherClick.Remove(listener); 
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
                target.HandleOtherClick(e);
            }
            
            if (localOnly)
            {
                break;
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultOtherClickAction(e);
        }
    }
    #endregion
    
    #region DoubleClick

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersDoubleClick = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnDoubleClick 
    {
        add => AddDoubleClickListener(value);
        remove => RemoveDoubleClickListener(value);
    }
    
    public void AddDoubleClickListener(EventHandler<MouseEvent> handler)
    {
        _listenersDoubleClick = _listenersDoubleClick.Add(new (handler));
    }
    
    public void AddDoubleClickListener(Action handler)
    {
        _listenersDoubleClick = _listenersDoubleClick.Add(new (e => handler()));
    }
    
    public void RemoveDoubleClickListener(EventHandler<MouseEvent> handler)
    {
        _listenersDoubleClick = _listenersDoubleClick.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddDoubleClickListener.
    /// </summary>
    protected virtual void HandleDoubleClick(MouseEvent e)
    {
    }

    /// <summary>
    /// Allows widgets to implement the default action associated with an event type.
    /// </summary>
    protected virtual void DefaultDoubleClickAction(MouseEvent e)
    {
    }
    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnDoubleClick
    /// </summary>
    internal void DispatchDoubleClick(MouseEvent e, bool localOnly = false)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersDoubleClick;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersDoubleClick = target._listenersDoubleClick.Remove(listener); 
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
                target.HandleDoubleClick(e);
            }
            
            if (localOnly)
            {
                break;
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultDoubleClickAction(e);
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
    
    public void AddTextInputListener(Action handler)
    {
        _listenersTextInput = _listenersTextInput.Add(new (e => handler()));
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
    internal void DispatchTextInput(TextInputEvent e, bool localOnly = false)
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
            
            if (localOnly)
            {
                break;
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
    
    public void AddKeyDownListener(Action handler)
    {
        _listenersKeyDown = _listenersKeyDown.Add(new (e => handler()));
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
    internal void DispatchKeyDown(KeyboardEvent e, bool localOnly = false)
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
            
            if (localOnly)
            {
                break;
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
    
    public void AddKeyUpListener(Action handler)
    {
        _listenersKeyUp = _listenersKeyUp.Add(new (e => handler()));
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
    internal void DispatchKeyUp(KeyboardEvent e, bool localOnly = false)
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
            
            if (localOnly)
            {
                break;
            }
        }
        
        if (!e.IsDefaultPrevented)
        {
            DefaultKeyUpAction(e);
        }
    }
    #endregion
    
    #region GotMouseCapture

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersGotMouseCapture = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnGotMouseCapture 
    {
        add => AddGotMouseCaptureListener(value);
        remove => RemoveGotMouseCaptureListener(value);
    }
    
    public void AddGotMouseCaptureListener(EventHandler<MouseEvent> handler)
    {
        _listenersGotMouseCapture = _listenersGotMouseCapture.Add(new (handler));
    }
    
    public void AddGotMouseCaptureListener(Action handler)
    {
        _listenersGotMouseCapture = _listenersGotMouseCapture.Add(new (e => handler()));
    }
    
    public void RemoveGotMouseCaptureListener(EventHandler<MouseEvent> handler)
    {
        _listenersGotMouseCapture = _listenersGotMouseCapture.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddGotMouseCaptureListener.
    /// </summary>
    protected virtual void HandleGotMouseCapture(MouseEvent e)
    {
    }

    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnGotMouseCapture
    /// </summary>
    internal void DispatchGotMouseCapture(MouseEvent e, bool localOnly = false)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersGotMouseCapture;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersGotMouseCapture = target._listenersGotMouseCapture.Remove(listener); 
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
                target.HandleGotMouseCapture(e);
            }
            
            if (localOnly)
            {
                break;
            }
        }
        
    }
    #endregion
    
    #region LostMouseCapture

    private ImmutableList<RegisteredListener<MouseEvent>> _listenersLostMouseCapture = ImmutableList<RegisteredListener<MouseEvent>>.Empty;
    
    public event EventHandler<MouseEvent>? OnLostMouseCapture 
    {
        add => AddLostMouseCaptureListener(value);
        remove => RemoveLostMouseCaptureListener(value);
    }
    
    public void AddLostMouseCaptureListener(EventHandler<MouseEvent> handler)
    {
        _listenersLostMouseCapture = _listenersLostMouseCapture.Add(new (handler));
    }
    
    public void AddLostMouseCaptureListener(Action handler)
    {
        _listenersLostMouseCapture = _listenersLostMouseCapture.Add(new (e => handler()));
    }
    
    public void RemoveLostMouseCaptureListener(EventHandler<MouseEvent> handler)
    {
        _listenersLostMouseCapture = _listenersLostMouseCapture.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddLostMouseCaptureListener.
    /// </summary>
    protected virtual void HandleLostMouseCapture(MouseEvent e)
    {
    }

    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnLostMouseCapture
    /// </summary>
    internal void DispatchLostMouseCapture(MouseEvent e, bool localOnly = false)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersLostMouseCapture;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersLostMouseCapture = target._listenersLostMouseCapture.Remove(listener); 
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
                target.HandleLostMouseCapture(e);
            }
            
            if (localOnly)
            {
                break;
            }
        }
        
    }
    #endregion
    
    #region Tooltip

    private ImmutableList<RegisteredListener<TooltipEvent>> _listenersTooltip = ImmutableList<RegisteredListener<TooltipEvent>>.Empty;
    
    public event EventHandler<TooltipEvent>? OnTooltip 
    {
        add => AddTooltipListener(value);
        remove => RemoveTooltipListener(value);
    }
    
    public void AddTooltipListener(EventHandler<TooltipEvent> handler)
    {
        _listenersTooltip = _listenersTooltip.Add(new (handler));
    }
    
    public void AddTooltipListener(Action handler)
    {
        _listenersTooltip = _listenersTooltip.Add(new (e => handler()));
    }
    
    public void RemoveTooltipListener(EventHandler<TooltipEvent> handler)
    {
        _listenersTooltip = _listenersTooltip.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddTooltipListener.
    /// </summary>
    protected virtual void HandleTooltip(TooltipEvent e)
    {
    }

    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnTooltip
    /// </summary>
    internal void DispatchTooltip(TooltipEvent e, bool localOnly = false)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersTooltip;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersTooltip = target._listenersTooltip.Remove(listener); 
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
                target.HandleTooltip(e);
            }
            
            if (localOnly)
            {
                break;
            }
        }
        
    }
    #endregion
    
    #region GetCursor

    private ImmutableList<RegisteredListener<GetCursorEvent>> _listenersGetCursor = ImmutableList<RegisteredListener<GetCursorEvent>>.Empty;
    
    public event EventHandler<GetCursorEvent>? OnGetCursor 
    {
        add => AddGetCursorListener(value);
        remove => RemoveGetCursorListener(value);
    }
    
    public void AddGetCursorListener(EventHandler<GetCursorEvent> handler)
    {
        _listenersGetCursor = _listenersGetCursor.Add(new (handler));
    }
    
    public void AddGetCursorListener(Action handler)
    {
        _listenersGetCursor = _listenersGetCursor.Add(new (e => handler()));
    }
    
    public void RemoveGetCursorListener(EventHandler<GetCursorEvent> handler)
    {
        _listenersGetCursor = _listenersGetCursor.Remove(new (handler));
    }
    
    /// <summary>
    /// Allows a class to handle events of this type without registering an event listener.
    /// These handlers always run after additional event handlers registered using AddGetCursorListener.
    /// </summary>
    protected virtual void HandleGetCursor(GetCursorEvent e)
    {
    }

    
    /// <summary>
    /// Allows a class to implicitly handle the event, without having to overwrite it.
    /// These handlers always run after additional event handlers registered using OnGetCursor
    /// </summary>
    internal void DispatchGetCursor(GetCursorEvent e, bool localOnly = false)
    {

        // Dispatch the event to this element, and then to all of its ancestors or until propagation is stopped
        for (var target = this; target != null && !e.IsPropagationStopped; target = target.Parent)
        {
            // Dispatch to additional registered handlers first
            var listeners = target._listenersGetCursor;
            foreach (var listener in listeners)
            {
                // We need to remove once-listeners now, since they may re-add themselves as a once-listener and we would immediately
                // remove it again.
                if (listener.Once)
                {
                    target._listenersGetCursor = target._listenersGetCursor.Remove(listener); 
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
                target.HandleGetCursor(e);
            }
            
            if (localOnly)
            {
                break;
            }
        }
        
    }
    #endregion
    
    
    private readonly record struct RegisteredListener<T>(EventHandler<T> Listener, bool Once = false) where T : UiEvent;
}

[global::System.CodeDom.Compiler.GeneratedCode("Core.WidgetEventGenerator", "2022-12-18T23:12:11.8606690Z")]
public enum UiEventType
{
    MouseDown,
    MouseUp,
    MouseEnter,
    MouseLeave,
    MouseMove,
    MouseWheel,
    Click,
    OtherClick,
    DoubleClick,
    TextInput,
    KeyDown,
    KeyUp,
    GotMouseCapture,
    LostMouseCapture,
    Tooltip,
    GetCursor,
}
