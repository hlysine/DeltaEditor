// Copyright @ MyScript. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyScript.IInk.UI
{
    public enum InputMode
    {
        AUTO = 0,
        PEN = 1,
        TOUCH = 2
    }

    public enum InputType
    {
        NONE = -1,
        MOUSE = 0,
        STYLUS = 1,
        TOUCH = 2
    }

    public class RendererListener : IRendererListener
    {
        private EditorUserControl _ucEditor;

        public RendererListener(EditorUserControl ucEditor)
        {
            _ucEditor = ucEditor;
        }

        public void ViewTransformChanged(Renderer renderer)
        {
            if (_ucEditor.SmartGuideEnabled && _ucEditor.smartGuide != null)
            {
                var dispatcher = _ucEditor.Dispatcher;
                dispatcher.BeginInvoke(new Action(() => { _ucEditor.smartGuide.OnTransformChanged(); }));
            }
        }
    }

    public class EditorListener : IEditorListener2
    {
        private EditorUserControl _ucEditor;

        public EditorListener(EditorUserControl ucEditor)
        {
            _ucEditor = ucEditor;
        }

        public void PartChanged(Editor editor)
        {
            if (_ucEditor.SmartGuideEnabled && _ucEditor.smartGuide != null)
            {
                var dispatcher = _ucEditor.Dispatcher;
                dispatcher.BeginInvoke(new Action(() => { _ucEditor.smartGuide.OnPartChanged(); }));
            }
        }

        public void ContentChanged(Editor editor, string[] blockIds)
        {
            if (_ucEditor.SmartGuideEnabled && _ucEditor.smartGuide != null)
            {
                var dispatcher = _ucEditor.Dispatcher;
                dispatcher.BeginInvoke(new Action(() => { _ucEditor.smartGuide.OnContentChanged(blockIds); }));
            }
        }

        public void SelectionChanged(Editor editor, string[] blockIds)
        {
            if (_ucEditor.SmartGuideEnabled && _ucEditor.smartGuide != null)
            {
                var dispatcher = _ucEditor.Dispatcher;
                dispatcher.BeginInvoke(new Action(() => { _ucEditor.smartGuide.OnSelectionChanged(blockIds); }));
            }
        }

        public void ActiveBlockChanged(Editor editor, string blockId)
        {
            if (_ucEditor.SmartGuideEnabled && _ucEditor.smartGuide != null)
            {
                var dispatcher = _ucEditor.Dispatcher;
                dispatcher.BeginInvoke(new Action(() => { _ucEditor.smartGuide.OnActiveBlockChanged(blockId); }));
            }
        }

        public void OnError(Editor editor, string blockId, string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    /// <summary>
    /// Interaction logic for EditorControl.xaml
    /// </summary>
    public sealed partial class EditorUserControl : UserControl, IRenderTarget
    {
        private Engine _engine;
        private Editor _editor;
        private Renderer _renderer;
        private ImageLoader _loader;
        private bool _smartGuideEnabled = true;
        private float _pixelDensity = 1.0f;

        public Engine Engine
        {
            get => _engine;

            set => _engine = value;
        }

        public Editor Editor => _editor;

        public Renderer Renderer => _renderer;

        public ImageLoader ImageLoader => _loader;

        public SmartGuideUserControl SmartGuide => smartGuide;

        public bool SmartGuideEnabled
        {
            get => _smartGuideEnabled;

            set => EnableSmartGuide(value);
        }

        public InputMode InputMode { get; set; }

        private InputType _inputType = InputType.NONE;
        private int _inputDeviceId = -1;
        private bool _onScroll = false;
        private Graphics.Point _lastPointerPosition;

        public EditorUserControl()
        {
            InitializeComponent();
            InputMode = InputMode.PEN;
        }

        public void Initialize(Window window)
        {
            Vector rawDPI = DisplayResolution.GetDpi(window, true);
            Vector effectiveDPI = DisplayResolution.GetDpi(window, false);
            float dpiX = (float)effectiveDPI.X;
            float dpiY = (float)effectiveDPI.Y;
            _pixelDensity = (float)rawDPI.Y / dpiY;

            _renderer = _engine.CreateRenderer(dpiX, dpiY, this);
            _renderer.AddListener(new RendererListener(this));

            modelLayer.Renderer = _renderer;
            captureLayer.Renderer = _renderer;

            _editor = _engine.CreateEditor(Renderer);
            _editor.SetViewSize((int)Math.Round(captureLayer.ActualWidth), (int)Math.Round(captureLayer.ActualHeight));
            _editor.SetFontMetricsProvider(new FontMetricsProvider(dpiX, dpiY));
            _editor.AddListener(new EditorListener(this));

            smartGuide.Editor = _editor;

            var tempFolder = _engine.Configuration.GetString("content-package.temp-folder");
            _loader = new ImageLoader(_editor, tempFolder);

            modelLayer.ImageLoader = _loader;
            captureLayer.ImageLoader = _loader;

            float verticalMarginPX = 60;
            float horizontalMarginPX = 40;
            float verticalMarginMM = 25.4f * verticalMarginPX / dpiY;
            float horizontalMarginMM = 25.4f * horizontalMarginPX / dpiX;
            _engine.Configuration.SetNumber("text.margin.top", verticalMarginMM);
            _engine.Configuration.SetNumber("text.margin.left", horizontalMarginMM);
            _engine.Configuration.SetNumber("text.margin.right", horizontalMarginMM);
            _engine.Configuration.SetNumber("math.margin.top", verticalMarginMM);
            _engine.Configuration.SetNumber("math.margin.bottom", verticalMarginMM);
            _engine.Configuration.SetNumber("math.margin.left", horizontalMarginMM);
            _engine.Configuration.SetNumber("math.margin.right", horizontalMarginMM);
        }

        public void Closing()
        {
            smartGuide?.Closing();
        }

        /// <summary>Retranscribe the size changed event to editor</summary>
        private void Control_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Editor != null)
            {
                // Dirty fix to avoid the editor crashing while resizing with only one dot drawn
                var part = _editor.Part;
                _editor.Part = null;
                _editor.Clear();
                _editor.SetViewSize((int)Math.Round(e.NewSize.Width), (int)Math.Round(e.NewSize.Height));
                try
                {
                    _editor.Part = part;
                }
                catch (Exception)
                {
                    _editor.Part = part.Package.CreatePart(part.Type);
                }
            }

            ((LayerControl)(sender)).InvalidateVisual();
        }

        /// <summary>Force inks layer to be redrawn</summary>
        public void Invalidate(Renderer renderer, LayerType layers)
        {
            if ((layers & LayerType.MODEL) != 0)
                modelLayer.Update();
            if ((layers & LayerType.CAPTURE) != 0)
                captureLayer.Update();
        }

        /// <summary>Force inks layer to be redrawn</summary>
        public void Invalidate(Renderer renderer, int x, int y, int width, int height, LayerType layers)
        {
            if (height >= 0)
            {
                if ((layers & LayerType.MODEL) != 0)
                    modelLayer.Update();
                if ((layers & LayerType.CAPTURE) != 0)
                    captureLayer.Update();
            }
        }

        public float GetPixelDensity()
        {
            return _pixelDensity;
        }

        private void EnableSmartGuide(bool enable)
        {
            if (_smartGuideEnabled == enable)
                return;

            _smartGuideEnabled = enable;

            if (!_smartGuideEnabled && smartGuide != null)
                smartGuide.Visibility = Visibility.Hidden;
        }

        private System.Windows.Point GetPosition(InputEventArgs e)
        {
            if (e is MouseEventArgs)
                return ((MouseEventArgs)e).GetPosition(this);
            else if (e is StylusEventArgs)
                return ((StylusEventArgs)e).GetPosition(this);
            else if (e is TouchEventArgs)
                return ((TouchEventArgs)e).GetTouchPoint(this).Position;
            else
                return new System.Windows.Point();
        }

        private System.Int64 GetTimestamp(InputEventArgs e)
        {
            return -1;
        }

        private float GetForce(InputEventArgs e)
        {
            if (e is StylusEventArgs)
            {
                var points = ((StylusEventArgs)e).GetStylusPoints(this);
                return ((points.Count > 0) ? points[0].PressureFactor : 0);
            }

            return 0;
        }

        private PointerType GetPointerType(InputEventArgs e)
        {
            switch (InputMode)
            {
                case InputMode.AUTO:
                    if (e is StylusEventArgs)
                        return PointerType.PEN;
                    else // if (e is TouchEventArgs || e is MouseEventArgs)
                        return PointerType.TOUCH;
                case InputMode.PEN:
                    return PointerType.PEN;
                case InputMode.TOUCH:
                    return PointerType.TOUCH;

                default:
                    return PointerType.PEN; // unreachable
            }
        }

        private int GetPointerId(InputEventArgs e)
        {
            if (e is StylusEventArgs)
                return (int)InputType.STYLUS;
            else if (e is MouseEventArgs)
                return (int)InputType.MOUSE;
            else if (e is TouchEventArgs)
                return (int)InputType.TOUCH;

            return (int)InputType.NONE;
        }

        private bool HasPart()
        {
            return (_editor != null) && (_editor.Part != null);
        }

        /// <summary>Retranscribe pointer event to editor</summary>
        public void OnPointerDown(InputEventArgs e)
        {
            var p = GetPosition(e);

            _lastPointerPosition = new Graphics.Point((float)p.X, (float)p.Y);
            _onScroll = false;

            _editor.PointerDown((float)p.X, (float)p.Y, GetTimestamp(e), GetForce(e), GetPointerType(e), GetPointerId(e));
        }

        /// <summary>Retranscribe pointer event to editor</summary>
        public void OnPointerMove(InputEventArgs e)
        {
            var p = GetPosition(e);
            var pointerType = GetPointerType(e);
            var pointerId = GetPointerId(e);
            var previousPosition = _lastPointerPosition;

            _lastPointerPosition = new Graphics.Point((float)p.X, (float)p.Y);

            if (!_onScroll && (pointerType == PointerType.TOUCH))
            {
                float deltaMin = 3.0f;
                float deltaX = _lastPointerPosition.X - previousPosition.X;
                float deltaY = _lastPointerPosition.Y - previousPosition.Y;

                _onScroll = _editor.IsScrollAllowed() && ((System.Math.Abs(deltaX) > deltaMin) || (System.Math.Abs(deltaY) > deltaMin));

                if (_onScroll)
                {
                    // Entering scrolling mode, cancel previous pointerDown event
                    _editor.PointerCancel(pointerId);
                }
            }

            if (_onScroll)
            {
                // Scroll the view
                float deltaX = _lastPointerPosition.X - previousPosition.X;
                float deltaY = _lastPointerPosition.Y - previousPosition.Y;
                Scroll(-deltaX, -deltaY);
            }
            else if (e is StylusEventArgs)
            {
                var pointList = ((StylusEventArgs)e).GetStylusPoints(this);
                if (pointList.Count > 0)
                {
                    var events = new PointerEvent[pointList.Count];
                    for (int i = 0; i < pointList.Count; ++i)
                    {
                        var p_ = pointList[i].ToPoint();
                        events[i] = new PointerEvent(PointerEventType.MOVE, (float)p_.X, (float)p_.Y, GetTimestamp(e), GetForce(e), pointerType, pointerId);
                    }

                    _editor.PointerEvents(events);
                }
            }
            else
            {
                // Send pointer move event to the editor
                _editor.PointerMove((float)p.X, (float)p.Y, GetTimestamp(e), GetForce(e), pointerType, pointerId);
            }
        }

        /// <summary>Retranscribe pointer event to editor</summary>
        public void OnPointerUp(InputEventArgs e)
        {
            var p = GetPosition(e);
            var previousPosition = _lastPointerPosition;

            _lastPointerPosition = new Graphics.Point((float)p.X, (float)p.Y);

            if (_onScroll)
            {
                // Scroll the view
                float deltaX = _lastPointerPosition.X - previousPosition.X;
                float deltaY = _lastPointerPosition.Y - previousPosition.Y;
                Scroll(-deltaX, -deltaY);

                // Exiting scrolling mode
                _onScroll = false;
            }
            else
            {
                // Send pointer up event to the editor
                _editor.PointerUp((float)p.X, (float)p.Y, GetTimestamp(e), GetForce(e), GetPointerType(e), GetPointerId(e));
            }
        }

        /// <summary>Retranscribe touch event to editor</summary>
        private void captureLayer_TouchDown(object sender, TouchEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.NONE)
                return;

            if (_inputDeviceId != -1)
                return;

            // Capture the touch device so all touch input is routed to this control.
            e.TouchDevice?.Capture(sender as UIElement, CaptureMode.SubTree);

            _inputType = InputType.TOUCH;
            _inputDeviceId = e.TouchDevice.Id;

            OnPointerDown(e);

            e.Handled = true;
        }

        /// <summary>Retranscribe touch event to editor</summary>
        private void captureLayer_TouchMove(object sender, TouchEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.TOUCH)
                return;

            if (_inputDeviceId != e.TouchDevice.Id)
                return;

            OnPointerMove(e);

            e.Handled = true;
        }

        /// <summary>Retranscribe touch event to editor</summary>
        private void captureLayer_TouchUp(object sender, TouchEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.TOUCH)
                return;

            if (_inputDeviceId != e.TouchDevice.Id)
                return;

            OnPointerUp(e);

            _inputType = InputType.NONE;
            _inputDeviceId = -1;

            e.TouchDevice?.Capture(null);

            e.Handled = true;
        }

        private bool IsStylusTipDown(StylusEventArgs e)
        {
            // Check that we only have the stylus tip down, without any other button
            bool ok = false;

            for (int i = 0; i < e.StylusDevice.StylusButtons.Count; ++i)
            {
                var button = e.StylusDevice.StylusButtons[i];
                var down = (button.StylusButtonState == StylusButtonState.Down);

                if (button.Guid == StylusPointProperties.TipButton.Id)
                    ok = down;
                else if (down)
                    return false;
            }

            return ok;
        }

        /// <summary>Retranscribe stylus event to editor</summary>
        private void captureLayer_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.NONE)
                return;

            if (_inputDeviceId != -1)
                return;

            if (!IsStylusTipDown(e))
                return;

            if ((e.StylusDevice.TabletDevice != null) && (e.StylusDevice.TabletDevice.Type != TabletDeviceType.Stylus))
                return; // Ignore if not generated by a stylus

            // Capture the stylus so all stylus input is routed to this control.
            e.StylusDevice?.Capture(sender as UIElement, CaptureMode.SubTree);

            _inputType = InputType.STYLUS;
            _inputDeviceId = e.StylusDevice.Id;

            OnPointerDown(e);

            e.Handled = true;
        }

        /// <summary>Retranscribe stylus event to editor</summary>
        private void captureLayer_StylusMove(object sender, StylusEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.STYLUS)
                return;

            if (_inputDeviceId != e.StylusDevice.Id)
                return;

            if (e.InAir)
                return; // Ignore stylus move when the pointing device is up

            if ((e.StylusDevice.TabletDevice != null) && (e.StylusDevice.TabletDevice.Type != TabletDeviceType.Stylus))
                return; // Ignore if not generated by a stylus

            OnPointerMove(e);

            e.Handled = true;
        }

        /// <summary>Retranscribe stylus event to editor</summary>
        private void captureLayer_StylusUp(object sender, StylusEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.STYLUS)
                return;

            if ((e.StylusDevice.TabletDevice != null) && (e.StylusDevice.TabletDevice.Type != TabletDeviceType.Stylus))
                return; // Ignore if not generated by a stylus

            OnPointerUp(e);

            _inputType = InputType.NONE;
            _inputDeviceId = -1;

            e.StylusDevice?.Capture(null);

            e.Handled = true;
        }

        /// <summary>Retranscribe mouse event to editor</summary>
        private void captureLayer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!HasPart())
                return;

            if (e.StylusDevice != null)
            {
                // Cancel the sampling if the event is sent by a long press with a stylus
                _editor.PointerCancel((int)(_inputType));
                _inputType = InputType.NONE;
                _inputDeviceId = -1;
            }
        }

        /// <summary>Retranscribe mouse event to editor</summary>
        private void captureLayer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.NONE)
                return;

            if (_inputDeviceId != -1)
                return;

            if (e.StylusDevice != null)
                return; // Ignore if not generated by a mouse

            // Capture the mouse so all mouse input is routed to this control.
            e.MouseDevice?.Capture(sender as UIElement, CaptureMode.SubTree);

            _inputType = InputType.MOUSE;
            _inputDeviceId = -1;

            OnPointerDown(e);

            e.Handled = true;
        }

        /// <summary>Retranscribe mouse event to editor</summary>
        private void captureLayer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.MOUSE)
                return;

            if (_inputDeviceId != -1)
                return;

            if (e.StylusDevice != null)
                return; // Ignore if not generated by a mouse

            OnPointerMove(e);

            e.Handled = true;
        }

        /// <summary>Retranscribe mouse event to editor</summary>
        private void captureLayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!HasPart())
                return;

            if (_inputType != InputType.MOUSE)
                return;

            if (_inputDeviceId != -1)
                return;

            if (e.StylusDevice != null)
                return; // Ignore if not generated by a mouse

            OnPointerUp(e);

            _inputType = InputType.NONE;
            _inputDeviceId = -1;

            e.MouseDevice?.Capture(null);

            e.Handled = true;
        }

        /// <summary>Retranscribe mouse event to editor</summary>
        private void captureLayer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!HasPart())
                return;

            if (e.StylusDevice != null)
                return; // Ignore if not generated by a mouse

            const int WHEEL_DELTA = Mouse.MouseWheelDeltaForOneLine;    // 120
            var controlDown = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
            var shiftDown = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
            var wheelDelta = e.Delta / WHEEL_DELTA;

            if (controlDown)
            {
                if (wheelDelta > 0)
                    ZoomIn((uint)wheelDelta);
                else if (wheelDelta < 0)
                    ZoomOut((uint)(-wheelDelta));
            }
            else
            {
                const int SCROLL_SPEED = 100;
                float delta = (float)(-SCROLL_SPEED * wheelDelta);
                float deltaX = shiftDown ? delta : 0.0f;
                float deltaY = shiftDown ? 0.0f : delta;

                Scroll(deltaX, deltaY);
            }

            e.Handled = true;
        }

        public void ResetView(bool forceInvalidate)
        {
            _renderer.ViewScale = 1;
            _renderer.ViewOffset = new MyScript.IInk.Graphics.Point(0, 0);

            if (forceInvalidate)
                Invalidate(_renderer, LayerType.LayerType_ALL);
        }

        public void ZoomIn(uint delta)
        {
            _renderer.Zoom((float)delta * (110.0f / 100.0f));
            Invalidate(_renderer, LayerType.LayerType_ALL);
        }

        public void ZoomOut(uint delta)
        {
            _renderer.Zoom((float)delta * (100.0f / 110.0f));
            Invalidate(_renderer, LayerType.LayerType_ALL);
        }

        private void Scroll(float deltaX, float deltaY)
        {
            var oldOffset = _renderer.ViewOffset;
            var newOffset = new MyScript.IInk.Graphics.Point(oldOffset.X + deltaX, oldOffset.Y + deltaY);

            _editor.ClampViewOffset(newOffset);

            _renderer.ViewOffset = newOffset;
            Invalidate(_renderer, LayerType.LayerType_ALL);
        }
    }
}
