using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Lattice;

namespace NBody {
    
    /// <summary>
    /// Represents the main window for the application. 
    /// </summary>
    class Window : Form {

        /// <summary>
        /// The number of milliseconds between draw frames. 
        /// </summary>
        private const int DrawInterval = 33;

        /// <summary>
        /// The multiplicative factor for easing the draw FPS. 
        /// </summary>
        private const double DrawFpsEasing = 0.2;

        /// <summary>
        /// The maximum FPS. 
        /// </summary>
        private const double FpsMax = 999.9;

        /// <summary>
        /// The distance from the right border to draw the info text. 
        /// </summary>
        private const int InfoWidth = 180;

        /// <summary>
        /// The world model of the simulation. 
        /// </summary>
        private World _world = World.Instance;

        /// <summary>
        /// Gives the current location of the mouse. 
        /// </summary>
        private Point _mouseLocation = new Point();

        /// <summary>
        /// Gives whether a mouse button is pressed down. 
        /// </summary>
        private Boolean _mouseIsDown = false;

        /// <summary>
        /// The stopwatch for the draw FPS. 
        /// </summary>
        private Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// The drawing FPS. 
        /// </summary>
        private double _fps = 0;

        /// <summary>
        /// Starts the application. 
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Window());
        }

        /// <summary>
        /// Constructs the main window, starts drawing, and shows the settings 
        /// window. 
        /// </summary>
        public Window() {

            // Initialize window settings and event handlers. 
            ClientSize = new Size(1000, 500);
            Text = "N-Body";
            DoubleBuffered = true;
            BackColor = Color.Black;

            MouseDown += MouseDownHandler;
            MouseUp += MouseUpHandler;
            MouseMove += MouseMoveHandler;
            MouseWheel += MouseWheelHandler;
            Paint += DrawHandler;

            // Start draw thread. 
            new Thread(new ThreadStart(() => {
                while (true) {
                    Invalidate();
                    Thread.Sleep(DrawInterval);
                }
            })) {
                IsBackground = true
            }.Start();

            // Display the settings window.
            new Settings().Show();
        }


        /// <summary>
        /// Draws the simulation. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DrawHandler(Object sender, PaintEventArgs e) {
            try {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw the world. 
                g.TranslateTransform(Width / 2, Height / 2);
                _world.Draw(g);
                g.ResetTransform();

                // Draw the info text. 
                using (Font font = new Font("Lucida Console", 8))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.White))) {
                    int x = Width - InfoWidth;

                    g.DrawString(String.Format("{0,-13}{1:#0.0}", "Simulation", _world.Fps), font, brush, x, 10);
                    g.DrawString(String.Format("{0,-13}{1:#0.0}", "Draw", _fps), font, brush, x, 24);
                    g.DrawString(String.Format("{0,-13}{1}", "Bodies", _world.BodyCount), font, brush, x, 38);
                    g.DrawString(String.Format("{0,-13}{1:e2}", "Total mass", _world.TotalMass), font, brush, x, 52);
                    g.DrawString(String.Format("{0,-13}{1}", "Frames", _world.Frames), font, brush, x, 66);

                    g.DrawString("ZONG ZHENG LI", font, brush, x, Height - 60);
                }

                // Update draw FPS. 
                _stopwatch.Stop();
                _fps += (1000.0 / _stopwatch.Elapsed.TotalMilliseconds - _fps) * DrawFpsEasing;
                _fps = Math.Min(_fps, FpsMax);
                _stopwatch.Reset();
                _stopwatch.Start();

            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Invoked when a mouse button is pressed down. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseDownHandler(Object sender, MouseEventArgs e) {
            _mouseIsDown = true;
        }

        /// <summary>
        /// Invoked when a mouse button is lifted up. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseUpHandler(Object sender, MouseEventArgs e) {
            _mouseIsDown = false;
        }

        /// <summary>
        /// Invoked when the mouse cursor is moved. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseMoveHandler(Object sender, MouseEventArgs e) {
            if (_mouseIsDown)
                RotationHelper.MouseDrag(_world.Rotate, e.X - _mouseLocation.X, e.Y - _mouseLocation.Y);
            _mouseLocation = e.Location;
        }

        /// <summary>
        /// Invoked when the mouse wheel is scrolled. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseWheelHandler(Object sender, MouseEventArgs e) {
        }
    }
}
