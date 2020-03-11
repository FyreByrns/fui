using System;
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
            new UIElement(){X=10,Y=10,Width=100,Height=50},
        };

        public Testing() {
            ui[0].MousePress += Testing_MouseClick;

            Construct(800, 600, 1, 1); Start();
        }

        private void Testing_MouseClick(int x, int y, Mouse button) {
            Console.WriteLine("Clicc");
        }

        public override void OnUpdate(float elapsed) {
            base.OnUpdate(elapsed);

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

        #endregion Properties
        #region Utility Functions
        public static bool PointWithin(int pX, int pY, Point rectTopLeft, Point rectBottomRight)
            => pX > rectTopLeft.X && pX < rectBottomRight.X && pY > rectTopLeft.Y && pY < rectBottomRight.Y;
        public static bool PointWithin(int pX, int pY, int left, int top, int right, int bottom)
            => PointWithin(pX, pY, new Point(left, top), new Point(bottom, right));

        #endregion Utility Functions

        public virtual void Update(Game context) {
            #region Fire Mouse Events
            int mouseX = context.MouseX;
            int mouseY = context.MouseY;

            if ((mouseX != mouseLastX || mouseY != mouseLastY) && PointWithin(mouseX, mouseY, TopLeft, BottomRight)) {
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
            #endregion Fire Mouse Events
        }
        public virtual void Draw(Game context, bool drawDebug = false) {
            if (drawDebug) {
                // Bounds
                context.DrawRect(TopLeft, BottomRight, Pixel.Presets.Red);
            }
        }
    }
}
