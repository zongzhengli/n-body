using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Lattice;

namespace NBody {

    // The preset system types that can be generated. 
    enum SystemType { None, SlowParticles, FastParticles, MassiveBody, OrbitalSystem, BinarySystem, PlanetarySystem, DistributionTest };

    /// <summary>
    /// The main window of the application and the "world" of the simulation. 
    /// </summary>
    class World : Form {

        /// <summary>
        /// The accessor for the World instance. 
        /// </summary>
        public static World Instance {
            get {
                if (_instance == null) {
                    _instance = new World();
                }
                return _instance;
            }
        }

        /// <summary>
        /// The number of Bodes in the simulation. 
        /// </summary>
        public int Bodies {
            get {
                return _bodies.Length;
            }
            set {
                if (_bodies.Length != value)
                    _bodies = new Body[value];
            }
        }

        /// <summary>
        /// The gravitational constant. 
        /// </summary>
        public static double G = 67;

        /// <summary>
        /// The maximum speed. 
        /// </summary>
        public static double C = 1e4;

        /// <summary>
        /// Determines whether the simulation is active or paused. 
        /// </summary>
        public Boolean Active = true;

        /// <summary>
        /// The World instance. 
        /// </summary>
        private static World _instance = null;

        /// <summary>
        /// The collection of Bodies in the simulation. 
        /// </summary>
        private Body[] _bodies = new Body[1000];

        /// <summary>
        /// The lock that must be held to modify the Bodies collection. 
        /// </summary>
        private readonly Object BodyLock = new Object();

        /// <summary>
        /// The target number of milliseconds between steps in the simulation. 
        /// </summary>
        private const int SimInterval = 33;

        /// <summary>
        /// The target number of milliseconds between draw frames. 
        /// </summary>
        private const int DrawInterval = 33;

        /// <summary>
        /// The camera field of view. 
        /// </summary>
        private const double CameraFOV = 1e9;

        /// <summary>
        /// The default value for the camera's position on the z-axis. 
        /// </summary>
        private const double CameraZDefault = 1e6;

        /// <summary>
        /// The acceleration constant for camera scrolling. 
        /// </summary>
        private const double CameraZAcceleration = -2e-4;

        /// <summary>
        /// The easing factor for camera scrolling. 
        /// </summary>
        private const double CameraZEasing = .94;

        /// <summary>
        /// The camera's position on the z-axis. 
        /// </summary>
        private double _cameraZ = CameraZDefault;

        /// <summary>
        /// The camera's velocity along the z-axis. 
        /// </summary>
        private double _cameraZVelocity = 0;

        /// <summary>
        /// Gives the current location of the mouse. 
        /// </summary>
        private Point _mouseLocation = new Point();

        /// <summary>
        /// Gives whether a mouse button is pressed down. 
        /// </summary>
        private Boolean _mouseIsDown = false;

        /// <summary>
        /// The stopwatch for the simulation FPS counter. 
        /// </summary>
        private Stopwatch _simStopwatch = new Stopwatch();

        /// <summary>
        /// The stopwatch for the drawing FPS counter. 
        /// </summary>
        private Stopwatch _drawStopwatch = new Stopwatch();

        /// <summary>
        /// The simulation FPS counter. 
        /// </summary>
        private double _simFps;

        /// <summary>
        /// The drawing FPS counter. 
        /// </summary>
        private double _drawFps;

        /// <summary>
        /// The Renderer instance used to draw 3D graphics. 
        /// </summary>
        private Renderer _renderer = new Renderer();

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(World.Instance);
        }

        /// <summary>
        /// Constructs a World by initializing window properties, starting draw and 
        /// simulation threads, and displaying the Settings window. 
        /// </summary>
        public World() {

            // Initialize window settings and event handlers. 
            ClientSize = new Size(1000, 500);
            Text = "N-Body";
            DoubleBuffered = true;
            BackColor = Color.Black;

            MouseDown += new MouseEventHandler(MouseDownEvent);
            MouseUp += new MouseEventHandler(MouseUpEvent);
            MouseMove += new MouseEventHandler(MouseMoveEvent);
            MouseWheel += new MouseEventHandler(MouseWheelEvent);
            Paint += new PaintEventHandler(DrawEvent);

            // Start draw thread. 
            new Thread(new ThreadStart(() => {
                while (true) {
                    UpdateCamera();
                    Invalidate();
                    Thread.Sleep(DrawInterval);
                }
            })) {
                IsBackground = true
            }.Start();

            // Start simulation thread. 
            new Thread(new ThreadStart(Simulate)) {
                IsBackground = true
            }.Start();

            // Center the window and display the Settings window.
            CenterToScreen();
            new Settings().Show();

            // Initialize the camera field of view. 
            _renderer.FOV = CameraFOV;
        }

        /// <summary>
        /// Performs the simulation. 
        /// </summary>
        private void Simulate() {
            while (true) {
                if (Active)
                    lock (BodyLock) {

                        // Determine half the length of the cube containing all the Bodies. 
                        double halfLength = 0;
                        foreach (Body body in _bodies)
                            if (body != null) {
                                body.Update();
                                halfLength = Math.Max(Math.Abs(body.Location.X), halfLength);
                                halfLength = Math.Max(Math.Abs(body.Location.Y), halfLength);
                                halfLength = Math.Max(Math.Abs(body.Location.Z), halfLength);
                            }

                        // Initialize the root tree and add the Bodies. The root tree needs to be 
                        // slightly larger than twice the determined half length. 
                        Octree tree = new Octree(2.1 * halfLength);
                        foreach (Body body in _bodies)
                            if (body != null)
                                tree.Add(body);

                        // Accelerate the bodies in parallel. 
                        Parallel.ForEach(_bodies, body => {
                            if (body != null)
                                tree.Accelerate(body);
                        });
                    }

                // Sleep for the necessary time. 
                int elapsed = (int)_simStopwatch.ElapsedMilliseconds;
                if (elapsed < SimInterval)
                    Thread.Sleep(SimInterval - elapsed);

                // Update the simluation FPS counter.
                _simStopwatch.Stop();
                _simFps += (1000D / _simStopwatch.Elapsed.TotalMilliseconds - _simFps) * .2;
                _simFps = _simFps > 1e7 ? 60 : _simFps;
                _simStopwatch.Reset();
                _simStopwatch.Start();
            }
        }

        /// <summary>
        /// Generates the specified gravitational system. 
        /// </summary>
        /// <param name="type">The system type to generate.</param>
        public void Generate(SystemType type) {

            // TODO: improve the low quality code in this method.  
            lock (BodyLock) {
                switch (type) {

                    case SystemType.None:
                        Array.Clear(_bodies, 0, _bodies.Length);
                        break;

                    case SystemType.SlowParticles: {
                            for (int i = 0; i < _bodies.Length; i++) {
                                double d = PseudoRandom.Double(1e6);
                                double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e5, 2e5), Math.Sin(a) * d);
                                double m = PseudoRandom.Double(1e6) + 3e4;
                                Vector v = PseudoRandom.Vector(5);
                                _bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;

                    case SystemType.FastParticles: {
                            for (int i = 0; i < _bodies.Length; i++) {
                                double d = PseudoRandom.Double(1e6);
                                double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e5, 2e5), Math.Sin(a) * d);
                                double m = PseudoRandom.Double(1e6) + 3e4;
                                Vector v = PseudoRandom.Vector(5e3);
                                _bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;

                    case SystemType.MassiveBody: {
                            _bodies[0] = new Body(Vector.Zero, 1e10);
                            Vector l1 = PseudoRandom.Vector(8e3) + new Vector(-3e5, 1e5 + _bodies[0].Radius, 0);
                            double m1 = 1e6;
                            Vector v1 = new Vector(2e3, 0, 0);
                            _bodies[1] = new Body(l1, m1, v1);
                            for (int i = 2; i < _bodies.Length; i++) {
                                double d = PseudoRandom.Double(2e5) + _bodies[1].Radius;
                                double a = PseudoRandom.Double(Math.PI * 2);
                                double h = Math.Min(2e8 / d, 2e4);
                                Vector l = (new Vector(Math.Cos(a) * d, PseudoRandom.Double(-h, h), Math.Sin(a) * d) + _bodies[1].Location);
                                double m = PseudoRandom.Double(5e5) + 1e5;
                                double s = Math.Sqrt(_bodies[1].Mass * _bodies[1].Mass * G / ((_bodies[1].Mass + m) * d));
                                Vector v = Vector.Cross(l, Vector.YAxis).Unit() * s + v1;
                                l = l.Rotate(0, 0, 0, 1, 1, 1, Math.PI * .1);
                                v = v.Rotate(0, 0, 0, 1, 1, 1, Math.PI * .1);
                                _bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;

                    case SystemType.OrbitalSystem: {
                            _bodies[0] = new Body(1e10);
                            for (int i = 1; i < _bodies.Length; i++) {
                                double d = PseudoRandom.Double(1e6) + _bodies[0].Radius;
                                double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e4, 2e4), Math.Sin(a) * d);
                                double m = PseudoRandom.Double(1e6) + 3e4;
                                double s = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + m) * d));
                                Vector v = Vector.Cross(l, Vector.YAxis).Unit() * s;
                                _bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;

                    // TODO: thoroughly clean up the code in this case. 
                    case SystemType.BinarySystem: {
                            double m1 = PseudoRandom.Double(9e9) + 1e9;
                            double m2 = PseudoRandom.Double(9e9) + 1e9;
                            double d0 = PseudoRandom.Double(1e5) + 3e4;
                            double a0 = PseudoRandom.Double(Math.PI * 2);
                            double d1 = d0 / 2;
                            double d2 = d0 / 2;
                            Vector l1 = new Vector(Math.Cos(a0) * d1, 0, Math.Sin(a0) * d1);
                            Vector l2 = new Vector(-Math.Cos(a0) * d2, 0, -Math.Sin(a0) * d2);
                            double s1 = Math.Sqrt(m2 * m2 * G / ((m1 + m2) * d0));
                            double s2 = Math.Sqrt(m1 * m1 * G / ((m1 + m2) * d0));
                            Vector v1 = Vector.Cross(l1, Vector.YAxis).Unit() * s1;
                            Vector v2 = Vector.Cross(l2, Vector.YAxis).Unit() * s2;
                            _bodies[0] = new Body(l1, m1, v1);
                            _bodies[1] = new Body(l2, m2, v2);
                            for (int i = 2; i < _bodies.Length; i++) {
                                double d = PseudoRandom.Double(1e6);
                                double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e4, 2e4), Math.Sin(a) * d);
                                double m = PseudoRandom.Double(1e6) + 3e4;
                                double s = Math.Sqrt((m1 + m2) * (m1 + m2) * G / ((m1 + m2 + m) * d));
                                s = d < d0 / 2 ? s / (d0 / 2 / d) : s;
                                Vector v = Vector.Cross(l, Vector.YAxis).Unit() * s;
                                _bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;

                    // TODO: thoroughly clean up the code in this case. 
                    case SystemType.PlanetarySystem: {
                            _bodies[0] = new Body(1e10);
                            int pn = PseudoRandom.Int32(10) + 5;
                            int pr = PseudoRandom.Int32(1) + 1;
                            int k = 1;
                            for (int i = 1; i < pn + 1 && k < _bodies.Length; i++) {
                                double d = PseudoRandom.Double(2e6) + 1e5 + _bodies[0].Radius;
                                double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e4, 2e4), Math.Sin(a) * d);
                                double m = PseudoRandom.Double(1e8) + 1e7;
                                double s = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + m) * d));
                                Vector v = Vector.Cross(l, Vector.YAxis).Unit() * s;
                                int p = k;
                                _bodies[k++] = new Body(l, m, v);

                                // Generate rings.
                                if (--pr >= 0 && k < _bodies.Length - 100) {
                                    for (int j = 0; j < 100; j++) {
                                        double dr = PseudoRandom.Double(1e1) + 1e4 + _bodies[p].Radius;
                                        double ar = PseudoRandom.Double(Math.PI * 2);
                                        Vector lr = l + new Vector(Math.Cos(ar) * dr, 0, Math.Sin(ar) * dr);
                                        double mr = PseudoRandom.Double(1e3) + 1e3;
                                        double sr = Math.Sqrt(_bodies[p].Mass * _bodies[p].Mass * G / ((_bodies[p].Mass + mr) * dr));
                                        Vector vr = Vector.Cross(l - lr, Vector.YAxis).Unit() * sr + v;
                                        _bodies[k++] = new Body(lr, mr, vr);
                                    }
                                    continue;
                                }

                                // Generate moons. 
                                int mn = PseudoRandom.Int32(4);
                                while (mn-- > 0 && k < _bodies.Length) {
                                    double dm = PseudoRandom.Double(1e4) + 5e3 + _bodies[p].Radius;
                                    double am = PseudoRandom.Double(Math.PI * 2);
                                    Vector lm = l + new Vector(Math.Cos(am) * dm, PseudoRandom.Double(-2e3, 2e3), Math.Sin(am) * dm);
                                    double mm = PseudoRandom.Double(1e6) + 1e5;
                                    double sm = Math.Sqrt(_bodies[p].Mass * _bodies[p].Mass * G / ((_bodies[p].Mass + mm) * dm));
                                    Vector vm = Vector.Cross(lm - l, Vector.YAxis).Unit() * sm + v;
                                    _bodies[k++] = new Body(lm, mm, vm);
                                }
                            }

                            // Generate asteroid belt.
                            while (k < _bodies.Length) {
                                double db = PseudoRandom.Double(4e5) + 1e6;
                                double ab = PseudoRandom.Double(Math.PI * 2);
                                Vector lb = new Vector(Math.Cos(ab) * db, PseudoRandom.Double(-1e3, 1e3), Math.Sin(ab) * db);
                                double mb = PseudoRandom.Double(1e6) + 3e4;
                                double sb = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + mb) * db));
                                Vector vb = Vector.Cross(lb, Vector.YAxis).Unit() * sb;
                                _bodies[k++] = new Body(lb, mb, vb);
                            }
                        }
                        break;

                    case SystemType.DistributionTest: {
                            Array.Clear(_bodies, 0, _bodies.Length);
                            double d = 4e4;
                            double m = 5e6;
                            int s = (int)Math.Pow(_bodies.Length, 1 / 3D);
                            int k = 0;
                            for (int a = 0; a < s; a++)
                                for (int b = 0; b < s; b++)
                                    for (int c = 0; c < s; c++)
                                        _bodies[k++] = new Body(d * (new Vector((a - s / 2), (b - s / 2), (c - s / 2))), m);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Rotates the world by calling the Bodies' rotate methods. 
        /// </summary>
        /// <param name="point">The starting point for the axis of rotation.</param>
        /// <param name="direction">The direction for the axis of rotation</param>
        /// <param name="angle">The angle to rotate by.</param>
        public void Rotate(Vector point, Vector direction, double angle) {
            lock (BodyLock) {
                Parallel.ForEach(_bodies, body => {
                    if (body != null)
                        body.Rotate(point, direction, angle);
                });
            }
        }

        /// <summary>
        /// Updates the camera properties to allow movement through the z axis. 
        /// </summary>
        private void UpdateCamera() {
            _cameraZ += _cameraZVelocity * _cameraZ;
            _cameraZ = Math.Max(1, _cameraZ);
            _cameraZVelocity *= CameraZEasing;

            _renderer.Camera = new Vector(0, 0, _cameraZ);
            _renderer.Origin = new Point(Width / 2, Height / 2);
        }

        /// <summary>
        /// Resets the camera to its initial state. 
        /// </summary>
        public void ResetCamera() {
            _cameraZ = CameraZDefault;
            _cameraZVelocity = 0;
        }

        /// <summary>
        /// Draws the simulation. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void DrawEvent(Object sender, PaintEventArgs e) {
            try {
                Graphics g = e.Graphics;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                SolidBrush brush = new SolidBrush(Color.White);

                // Draw the Bodies. 
                for (int i = 0; i < _bodies.Length; i++) {
                    Body body = _bodies[i];
                    if (body != null)
                        _renderer.FillCircle2D(g, brush, body.Location, body.Radius);
                }

                brush = new SolidBrush(Color.FromArgb(50, Color.White));
                g.DrawString("DRAW FPS: " + Math.Round(_drawFps * 10) / 10D, new Font("Arial", 8), brush, Width - 280, 10);
                g.DrawString("SIMULATION FPS: " + Math.Round(_simFps * 10) / 10D, new Font("Arial", 8), brush, Width - 170, 10);
                g.DrawString("ZONG ZHENG LI", new Font("Arial", 8), brush, new Point(Width - 120, Height - 60));

                // Update draw FPS counter. 
                _drawStopwatch.Stop();
                _drawFps += (1000D / _drawStopwatch.Elapsed.TotalMilliseconds - _drawFps) * .2;
                _drawFps = _drawFps > 3e3 ? 60 : _drawFps;
                _drawStopwatch.Reset();
                _drawStopwatch.Start();
            } catch (Exception x) {
                Console.WriteLine(x);
            }
        }

        /// <summary>
        /// Invoked when a mouse button is pressed down. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseDownEvent(Object sender, MouseEventArgs e) {
            _mouseIsDown = true;
        }

        /// <summary>
        /// Invoked when a mouse button is lifted up. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseUpEvent(Object sender, MouseEventArgs e) {
            _mouseIsDown = false;
        }

        /// <summary>
        /// Invoked when the mouse cursor is moved. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseMoveEvent(Object sender, MouseEventArgs e) {
            if (_mouseIsDown)
                RotationHelper.MouseDrag(Rotate, e.X - _mouseLocation.X, e.Y - _mouseLocation.Y);
            _mouseLocation.X = e.X;
            _mouseLocation.Y = e.Y;
        }

        /// <summary>
        /// Invoked when the mouse wheel is scrolled. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseWheelEvent(Object sender, MouseEventArgs e) {
            _cameraZVelocity += e.Delta * CameraZAcceleration;
        }
    }
}
