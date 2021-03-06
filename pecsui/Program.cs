﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelEngine;

/*
Goal:
    Reuseable, extensible UI for PixelEngine C#
*/

namespace pecsui {
    class Program {
        static void Main(string[] args) {
            new Testing();
        }
    }

    class Testing : Game {
        List<UIElement> ui = new List<UIElement>() {
            new UIElement(){X=10,Y=10,Width=100,Height=50,MarginX=20,MarginY=20},
            new ResizeableContainer(100, 100, 100, 100),
        };

        public Testing() {
            ui[0].MousePress += Testing_MouseClick;
            ui[0].Parent = ui[1];

            Construct(800, 600, 1, 1); Start();
        }

        private void Testing_MouseClick(int x, int y, Mouse button) {
            Console.WriteLine($"Click at {x},{y}");
        }

        public override void OnUpdate(float elapsed) {
            base.OnUpdate(elapsed);
            Clear(Pixel.Empty);

            foreach (UIElement e in ui) {
                e.Update(this);
                e.Draw(this, true);
            }
        }

        public override void OnDestroy() {
            base.OnDestroy();
        }
    }

    /// <summary>
    /// Base element for all UI.
    /// </summary>
    public class UIElement {
        #region Events
        #region Mouse Events
        public delegate void MouseEnterEventHandler();
        /// <summary>
        /// Fires when the mouse enters this element
        /// </summary>
        public event MouseEnterEventHandler MouseEnter;
        public delegate void MouseLeaveEventHandler();
        /// <summary>
        /// Fires when the mouse leaves this element
        /// </summary>
        public event MouseLeaveEventHandler MouseLeave;

        public delegate void MouseMoveEventHandler(int x, int y);
        /// <summary>
        /// Fires when the mouse is moved on this element
        /// </summary>
        public event MouseMoveEventHandler MouseMove;
        public delegate void MouseDragEventHandler(int x, int y, Mouse button);
        /// <summary>
        /// Fires when the mouse is moved on this element with a button pressed
        /// </summary>
        public event MouseDragEventHandler MouseDrag;

        public delegate void MouseDownEventHandler(int x, int y, Mouse button);
        /// <summary>
        /// Fires when a mouse button is pressed on this element
        /// </summary>
        public event MouseDownEventHandler MouseDown;
        public delegate void MouseUpEventHandler(int x, int y, Mouse button);
        /// <summary>
        /// Fires when a mouse button is released on this element
        /// </summary>
        public event MouseUpEventHandler MouseUp;
        public delegate void MousePressEventHandler(int x, int y, Mouse button);
        /// <summary>
        /// Fires when a mouse button is pressed and released on this element
        /// </summary>
        public event MousePressEventHandler MousePress;

        #endregion Mouse Events
        #region Position Events
        public delegate void PositionChangeEventHandler(int newX, int newY);
        /// <summary>
        /// Fires when the position of this element changes
        /// </summary>
        public event PositionChangeEventHandler PositionChange;

        public delegate void DimensionsChangeEventHandler(int newWidth, int newHeight);
        /// <summary>
        /// Fires when the dimensions of this element change
        /// </summary>
        public event DimensionsChangeEventHandler DimensionsChange;

        #endregion Position Events
        public delegate void ParentChangeEventHandler(UIElement newParent);
        public event ParentChangeEventHandler ParentChange;

        #endregion Events
        #region Mouse Tracking
        static int mouseLastX, mouseLastY;

        #endregion Mouse Tracking
        #region Properties
        #region Position
        public int X { get; set; }
        public int Y { get; set; }

        public int Left => X;
        public int Right => X + Width;
        public int Top => Y;
        public int Bottom => Y + Height;

        public int MidX => X + (Width / 2);
        public int MidY => Y + (Height / 2);

        public Point TopLeft => new Point(Left, Top);
        public Point TopRight => new Point(Right, Top);
        public Point BottomLeft => new Point(Left, Bottom);
        public Point BottomRight => new Point(Right, Bottom);

        public Point MidTop => new Point(MidX, Top);
        public Point MidLeft => new Point(Left, MidY);
        public Point MidRight => new Point(Right, MidY);
        public Point MidBottom => new Point(MidX, Bottom);

        #endregion Position
        #region Dimensions
        public int Width { get; set; }
        public int Height { get; set; }

        #endregion Dimensions
        #region Positioning
        public int MarginX { get; set; }
        public int MarginY { get; set; }

        public int MaxWidth { get; set; }
        public int MinWidth { get; set; }
        public int MaxHeight { get; set; }
        public int MinHeight { get; set; }

        #endregion Positioning

        public UIElement Parent {
            get => _parent; set {
                _parentOld?.Children?.Remove(this);
                _parent = value;
                _parent?.Children?.Add(this);
            }
        }
        public List<UIElement> Children { get; set; } = new List<UIElement>();
        #endregion Properties
        #region Fields
        int oldX, oldY, oldWidth, oldHeight;
        UIElement _parent, _parentOld;

        #endregion Fields
        #region Utility Functions
        public static bool PointWithin(int pX, int pY, Point rectTopLeft, Point rectBottomRight)
            => pX >= rectTopLeft.X && pX <= rectBottomRight.X && pY >= rectTopLeft.Y && pY <= rectBottomRight.Y;
        public static bool PointWithin(int pX, int pY, int left, int top, int right, int bottom)
            => PointWithin(pX, pY, new Point(left, top), new Point(bottom, right));

        #endregion Utility Functions

        public virtual void Update(Game context) {
            #region Fire Mouse Events
            int mouseX = context.MouseX;
            int mouseY = context.MouseY;
            int localX = mouseX - X;
            int localY = mouseY - Y;

            if (PointWithin(mouseX, mouseY, TopLeft, BottomRight)) {
                if ((mouseX != mouseLastX || mouseY != mouseLastY)) {
                    MouseMove?.Invoke(mouseX, mouseY);
                    if (context.GetMouse(Mouse.Any).Down) {
                        for (int i = 0; i < 3; i++) {
                            Mouse current = (Mouse)i;
                            if (context.GetMouse(current).Down) MouseDrag?.Invoke(mouseX, mouseY, current);
                        }
                    }
                }
                mouseLastX = mouseX;
                mouseLastY = mouseY;

                for (int i = 0; i < 3; i++) {
                    Mouse current = (Mouse)i;
                    if (context.GetMouse(current).Down) MouseDown?.Invoke(mouseX, mouseY, current);
                    if (context.GetMouse(current).Up) MouseUp?.Invoke(mouseX, mouseY, current);
                    if (context.GetMouse(current).Pressed) MousePress?.Invoke(mouseX, mouseY, current);
                }
            }
            #endregion Fire Mouse Events
            #region Fire Position Events
            if (oldX != X || oldY != Y)
                PositionChange?.Invoke(X, Y);
            if (oldWidth != Width || oldHeight != Height)
                DimensionsChange?.Invoke(Width, Height);

            oldX = X;
            oldY = Y;
            oldWidth = Width;
            oldHeight = Height;

            #endregion Fire Position Events
            #region Fire Familial Events
            if (Parent != _parentOld)
                ParentChange?.Invoke(Parent);
            _parentOld = _parent;

            #endregion Fire Familial Events

            foreach (UIElement child in Children) child.Update(context);
        }
        public virtual void Draw(Game context, bool drawDebug = false) {
            if (drawDebug) {
                // Bounds
                context.DrawRect(TopLeft, BottomRight, Pixel.Presets.Red);
            }
            foreach (UIElement child in Children) child.Draw(context, drawDebug);
        }
    }

    public class ResizeableElement : UIElement {
        protected UIElement TopLeftHandle, BottomRightHandle;
        int handleWidth = 40;
        int halfHandleWidth => handleWidth / 2;

        public ResizeableElement(int x, int y, int wid, int hgt) {
            X = x; Y = y; Width = wid; Height = hgt;

            TopLeftHandle = new UIElement() { X = X - handleWidth, Y = Y - handleWidth, Width = handleWidth, Height = handleWidth };
            BottomRightHandle = new UIElement() { X = Right + handleWidth, Y = Bottom + handleWidth, Width = handleWidth, Height = handleWidth };

            TopLeftHandle.MouseDrag += TopLeftHandle_MouseDrag;
            BottomRightHandle.MouseDrag += BottomRightHandle_MouseDrag;

            Children.Add(TopLeftHandle);
            Children.Add(BottomRightHandle);

            PositionChange += OnChangePosition;
            DimensionsChange += OnChangePosition;
        }

        public override void Update(Game context) {
            base.Update(context);

            X = TopLeftHandle.X + handleWidth;
            Y = TopLeftHandle.Y + handleWidth;

            Width = BottomRightHandle.X - X;
            Height = BottomRightHandle.Y - Y;
        }

        private void TopLeftHandle_MouseDrag(int x, int y, Mouse button) {
            TopLeftHandle.X = x - halfHandleWidth;
            TopLeftHandle.Y = y - halfHandleWidth;
        }
        private void BottomRightHandle_MouseDrag(int x, int y, Mouse button) {
            BottomRightHandle.X = x - halfHandleWidth;
            BottomRightHandle.Y = y - halfHandleWidth;
        }

        private void OnChangePosition(int newX, int newY) {
            //TopLeftHandle.X = Left - 5;
            //TopLeftHandle.Y = Top - 5;
            //BottomRightHandle.X = Right - 5;
            //BottomRightHandle.Y = Bottom - 5;
        }
    }
    public class ResizeableContainer : ResizeableElement {
        public ResizeableContainer(int x, int y, int wid, int hgt) : base(x, y, wid, hgt) {

        }

        public override void Update(Game context) {
            base.Update(context);

            foreach(UIElement c in Children) {
                if (c == TopLeftHandle || c == BottomRightHandle) continue;

                c.X = Left + c.MarginX;
                c.Y = Top + c.MarginY;
                c.Width = Width - 2*c.MarginX;
                c.Height = Height - 2*c.MarginY;
            }
        }
    }
}
