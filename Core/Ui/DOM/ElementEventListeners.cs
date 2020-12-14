namespace OpenTemple.Core.Ui.DOM
{
    public partial class Element
    {
        // See https://html.spec.whatwg.org/multipage/webappapis.html#event-handlers-on-elements,-document-objects,-and-window-objects

        #region OnClick

        private EventListener _onClickCastingListener;
        private MouseEventListener _onClickListener;

        public MouseEventListener OnClick
        {
            set
            {
                if (_onClickCastingListener != null)
                {
                    RemoveEventListener("click", _onClickCastingListener);
                    _onClickCastingListener = null;
                }

                _onClickListener = value;
                if (value != null)
                {
                    _onClickCastingListener = evt => value((MouseEvent) evt);
                    AddEventListener("click", _onClickCastingListener);
                }
            }
            get => _onClickListener;
        }

        #endregion

        #region OnMouseDown

        private EventListener _onMouseDownCastingListener;
        private MouseEventListener _onMouseDownListener;

        public MouseEventListener OnMouseDown
        {
            set
            {
                if (_onMouseDownCastingListener != null)
                {
                    RemoveEventListener("mousedown", _onMouseDownCastingListener);
                    _onMouseDownCastingListener = null;
                }

                _onMouseDownListener = value;
                if (value != null)
                {
                    _onMouseDownCastingListener = evt => value((MouseEvent) evt);
                    AddEventListener("mousedown", _onMouseDownCastingListener);
                }
            }
            get => _onMouseDownListener;
        }

        #endregion

        #region OnMouseUp

        private EventListener _onMouseUpCastingListener;
        private MouseEventListener _onMouseUpListener;

        public MouseEventListener OnMouseUp
        {
            set
            {
                if (_onMouseUpCastingListener != null)
                {
                    RemoveEventListener("mouseup", _onMouseUpCastingListener);
                    _onMouseUpCastingListener = null;
                }

                _onMouseUpListener = value;
                if (value != null)
                {
                    _onMouseUpCastingListener = evt => value((MouseEvent) evt);
                    AddEventListener("mouseup", _onMouseUpCastingListener);
                }
            }
            get => _onMouseUpListener;
        }

        #endregion

    }
}