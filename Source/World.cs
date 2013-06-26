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
    /// The main window of the application and acts as the "world" of the simulation. 
    /// </summary>
    class World : Form {


        /// <summary>
        /// The property giving access to the single World instance. 
        /// </summary>
        public static World Instance {
            get {
                if (instance == null) {
                    instance = new World();
                }
                return instance;
            }
        }

        /// <summary>
        /// The field the holds the single World instance. 
        /// </summary>
        public static World instance = null;

        /// <summary>
        /// The gravitational constant. 
        /// </summary>
        public static Double G = 67;

        /// <summary>
        /// The maximum speed. 
        /// </summary>
        public static Double C = 1e4;

        /// <summary>
        /// The target number of milliseconds in between steps in the simulation. 
        /// </summary>
        private const Int32 SimInterval = 33;

        /// <summary>
        /// The target number of milliseconds between draw frames. 
        /// </summary>
        private const Int32 DrawInterval = 33;

        /// <summary>
        /// The collection of Bodies in the simulation. 
        /// </summary>
        public Body[] Bodies = new Body[1000];

        /// <summary>
        /// The lock for any actions that modify the Bodies collection. 
        /// </summary>
        private readonly Object BodyLock = new Object();

        /// <summary>
        /// Determines whether the simulation is active or paused. 
        /// </summary>
        public Boolean Active = true;

        /// <summary>
        /// Determines the properties of the z coordinate of the camera. These are used to when the mouse is 
        /// scrolled and the camera moves along the z axis. 
        /// </summary>
        private const Double CameraZDefault = 1e6;
        private const Double CameraZAcceleration = -2e-4;
        private const Double CameraZEasing = .94;
        private Double CameraZ = CameraZDefault;
        private Double CameraZVelocity = 0;

        /// <summary>
        /// Represents the current location of the mouse. 
        /// </summary>
        private Point MouseLocation = new Point();

        /// <summary>
        /// Represents whether a mouse button is pressed down. 
        /// </summary>
        private Boolean MouseIsDown = false;

        /// <summary>
        /// Stopwatches for timing purposes. 
        /// </summary>
        private Stopwatch SimStopwatch = new Stopwatch();
        private Stopwatch DrawStopwatch = new Stopwatch();

        /// <summary>
        /// These are FPS counters update at each step. 
        /// </summary>
        private Double SimFps;
        private Double DrawFps;

        /// <summary>
        /// An instance of the Renderer for the Lattice library. This is used to draw 3D graphics. 
        /// </summary>
        private Renderer Renderer = new Renderer();

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(World.Instance);
        }

        /// <summary>
        /// Initializes the World. This sets window properties and other default values, starts threads, and 
        /// shows the Settings window. 
        /// </summary>
        public World() {

            // Set window settings and event handlers. 
            ClientSize = new Size(1000, 500);
            Text = "N-Body";

            MouseDown += new MouseEventHandler(MouseDownEvent);
            MouseUp += new MouseEventHandler(MouseUpEvent);
            MouseMove += new MouseEventHandler(MouseMoveEvent);
            MouseWheel += new MouseEventHandler(MouseWheelEvent);
            Paint += new PaintEventHandler(DrawEvent);
            DoubleBuffered = true;
            BackColor = Color.Black;

            // Start draw thread. 
            new Thread(new ThreadStart(delegate {
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

            // Center the window, start the Settings window, and set the initial field of value for the 
            // camera. 
            CenterToScreen();
            new Settings().Show();
            Renderer.FOV = 1e9;
        }

        /// <summary>
        /// Performs the simulation. 
        /// </summary>
        private void Simulate() {
            while (true) {
                if (Active)
                    lock (BodyLock) {
                        // Determine half the length of the cube containing all the Bodies. 
                        Double halfLength = 0;
                        foreach (Body body in Bodies)
                            if (body != null) {
                                body.Update();
                                halfLength = Math.Max(Math.Abs(body.Location.X), halfLength);
                                halfLength = Math.Max(Math.Abs(body.Location.Y), halfLength);
                                halfLength = Math.Max(Math.Abs(body.Location.Z), halfLength);
                            }

                        // Initialize the root tree and add the Bodies. The root tree needs to be slightly larger 
                        // than twice the determined half length. 
                        Octree tree = new Octree(2.1 * halfLength);
                        foreach (Body body in Bodies)
                            if (body != null)
                                tree.Add(body);

                        // Accelerate the bodies in parallel. 
                        Parallel.ForEach(Bodies, body => {
                            if (body != null)
                                tree.Accelerate(body);
                        });
                    }

                // Sleep for the necessary time. 
                Int32 elapsed = (Int32)SimStopwatch.ElapsedMilliseconds;
                if (elapsed < SimInterval)
                    Thread.Sleep(SimInterval);

                // Update simluation FPS counter.
                SimStopwatch.Stop();
                SimFps += (1000D / SimStopwatch.Elapsed.TotalMilliseconds - SimFps) * .2;
                SimFps = SimFps > 1e7 ? 60 : SimFps;
                SimStopwatch.Reset();
                SimStopwatch.Start();
            }
        }

        /// <summary>
        /// Generates the specified gravitational system. 
        /// </summary>
        /// <param name="type">The system type to generate.</param>
        public void Generate(SystemType type) {

            // This method needs to be cleaned up. There are lots of arbitrary constants, unclear variable names, and low
            // quality code. 
            lock (BodyLock) {
                switch (type) {
                    case SystemType.None:
                        Array.Clear(Bodies, 0, Bodies.Length);
                        break;
                    case SystemType.SlowParticles: {
                            for (Int32 i = 0; i < Bodies.Length; i++) {
                                Double d = PseudoRandom.Double(1e6);
                                Double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e5, 2e5), Math.Sin(a) * d);
                                Double m = PseudoRandom.Double(1e6) + 3e4;
                                Vector v = PseudoRandom.Vector(5);
                                Bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;
                    case SystemType.FastParticles: {
                            for (Int32 i = 0; i < Bodies.Length; i++) {
                                Double d = PseudoRandom.Double(1e6);
                                Double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e5, 2e5), Math.Sin(a) * d);
                                Double m = PseudoRandom.Double(1e6) + 3e4;
                                Vector v = PseudoRandom.Vector(5e3);
                                Bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;
                    case SystemType.MassiveBody: {
                            Bodies[0] = new Body(Vector.Zero, 1e10);
                            Vector l1 = PseudoRandom.Vector(8e3) + new Vector(-3e5, 1e5 + Bodies[0].Radius, 0);
                            Double m1 = 1e6;
                            Vector v1 = new Vector(2e3, 0, 0);
                            Bodies[1] = new Body(l1, m1, v1);
                            for (Int32 i = 2; i < Bodies.Length; i++) {
                                Double d = PseudoRandom.Double(2e5) + Bodies[1].Radius;
                                Double a = PseudoRandom.Double(Math.PI * 2);
                                Double h = Math.Min(2e8 / d, 2e4);
                                Vector l = (new Vector(Math.Cos(a) * d, PseudoRandom.Double(-h, h), Math.Sin(a) * d) + Bodies[1].Location);
                                Double m = PseudoRandom.Double(5e5) + 1e5;
                                Double s = Math.Sqrt(Bodies[1].Mass * Bodies[1].Mass * G / ((Bodies[1].Mass + m) * d));
                                Vector v = Vector.Cross(l, Vector.YAxis).Unit() * s + v1;
                                l = l.Rotate(0, 0, 0, 1, 1, 1, Math.PI * .1);
                                v = v.Rotate(0, 0, 0, 1, 1, 1, Math.PI * .1);
                                Bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;
                    case SystemType.OrbitalSystem: {
                            Bodies[0] = new Body(1e10);
                            for (Int32 i = 1; i < Bodies.Length; i++) {
                                Double d = PseudoRandom.Double(1e6) + Bodies[0].Radius;
                                Double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e4, 2e4), Math.Sin(a) * d);
                                Double m = PseudoRandom.Double(1e6) + 3e4;
                                Double s = Math.Sqrt(Bodies[0].Mass * Bodies[0].Mass * G / ((Bodies[0].Mass + m) * d));
                                Vector v = Vector.Cross(l, Vector.YAxis).Unit() * s;
                                Bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;
                    // This case exhibits especially terrible code. Needs to be cleaned up. 
                    case SystemType.BinarySystem: {
                            Double m1 = PseudoRandom.Double(9e9) + 1e9;
                            Double m2 = PseudoRandom.Double(9e9) + 1e9;
                            Double d0 = PseudoRandom.Double(1e5) + 3e4;
                            Double a0 = PseudoRandom.Double(Math.PI * 2);
                            Double d1 = d0 / 2;
                            Double d2 = d0 / 2;
                            Vector l1 = new Vector(Math.Cos(a0) * d1, 0, Math.Sin(a0) * d1);
                            Vector l2 = new Vector(-Math.Cos(a0) * d2, 0, -Math.Sin(a0) * d2);
                            Double s1 = Math.Sqrt(m2 * m2 * G / ((m1 + m2) * d0));
                            Double s2 = Math.Sqrt(m1 * m1 * G / ((m1 + m2) * d0));
                            Vector v1 = Vector.Cross(l1, Vector.YAxis).Unit() * s1;
                            Vector v2 = Vector.Cross(l2, Vector.YAxis).Unit() * s2;
                            Bodies[0] = new Body(l1, m1, v1);
                            Bodies[1] = new Body(l2, m2, v2);
                            for (Int32 i = 2; i < Bodies.Length; i++) {
                                Double d = PseudoRandom.Double(1e6);
                                Double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e4, 2e4), Math.Sin(a) * d);
                                Double m = PseudoRandom.Double(1e6) + 3e4;
                                Double s = Math.Sqrt((m1 + m2) * (m1 + m2) * G / ((m1 + m2 + m) * d));
                                s = d < d0 / 2 ? s / (d0 / 2 / d) : s;
                                Vector v = Vector.Cross(l, Vector.YAxis).Unit() * s;
                                Bodies[i] = new Body(l, m, v);
                            }
                        }
                        break;
                    // This case exhibits especially terrible code. Needs to be cleaned up. 
                    case SystemType.PlanetarySystem: {
                            Bodies[0] = new Body(1e10);
                            Int32 pn = PseudoRandom.Int32(10) + 5;
                            Int32 pr = PseudoRandom.Int32(1) + 1;
                            Int32 k = 1;
                            for (Int32 i = 1; i < pn + 1 && k < Bodies.Length; i++) {
                                Double d = PseudoRandom.Double(2e6) + 1e5 + Bodies[0].Radius;
                                Double a = PseudoRandom.Double(Math.PI * 2);
                                Vector l = new Vector(Math.Cos(a) * d, PseudoRandom.Double(-2e4, 2e4), Math.Sin(a) * d);
                                Double m = PseudoRandom.Double(1e8) + 1e7;
                                Double s = Math.Sqrt(Bodies[0].Mass * Bodies[0].Mass * G / ((Bodies[0].Mass + m) * d));
                                Vector v = Vector.Cross(l, Vector.YAxis).Unit() * s;
                                Int32 p = k;
                                Bodies[k++] = new Body(l, m, v);

                                // Generate rings.
                                if (--pr >= 0 && k < Bodies.Length - 100) {
                                    for (Int32 j = 0; j < 100; j++) {
                                        Double dr = PseudoRandom.Double(1e1) + 1e4 + Bodies[p].Radius;
                                        Double ar = PseudoRandom.Double(Math.PI * 2);
                                        Vector lr = l + new Vector(Math.Cos(ar) * dr, 0, Math.Sin(ar) * dr);
                                        Double mr = PseudoRandom.Double(1e3) + 1e3;
                                        Double sr = Math.Sqrt(Bodies[p].Mass * Bodies[p].Mass * G / ((Bodies[p].Mass + mr) * dr));
                                        Vector vr = Vector.Cross(l - lr, Vector.YAxis).Unit() * sr + v;
                                        Bodies[k++] = new Body(lr, mr, vr);
                                    }
                                    continue;
                                }

                                // Generate moons. 
                                Int32 mn = PseudoRandom.Int32(4);
                                while (mn-- > 0 && k < Bodies.Length) {
                                    Double dm = PseudoRandom.Double(1e4) + 5e3 + Bodies[p].Radius;
                                    Double am = PseudoRandom.Double(Math.PI * 2);
                                    Vector lm = l + new Vector(Math.Cos(am) * dm, PseudoRandom.Double(-2e3, 2e3), Math.Sin(am) * dm);
                                    Double mm = PseudoRandom.Double(1e6) + 1e5;
                                    Double sm = Math.Sqrt(Bodies[p].Mass * Bodies[p].Mass * G / ((Bodies[p].Mass + mm) * dm));
                                    Vector vm = Vector.Cross(lm - l, Vector.YAxis).Unit() * sm + v;
                                    Bodies[k++] = new Body(lm, mm, vm);
                                }
                            }

                            // Generate asteroid belt.
                            while (k < Bodies.Length) {
                                Double db = PseudoRandom.Double(4e5) + 1e6;
                                Double ab = PseudoRandom.Double(Math.PI * 2);
                                Vector lb = new Vector(Math.Cos(ab) * db, PseudoRandom.Double(-1e3, 1e3), Math.Sin(ab) * db);
                                Double mb = PseudoRandom.Double(1e6) + 3e4;
                                Double sb = Math.Sqrt(Bodies[0].Mass * Bodies[0].Mass * G / ((Bodies[0].Mass + mb) * db));
                                Vector vb = Vector.Cross(lb, Vector.YAxis).Unit() * sb;
                                Bodies[k++] = new Body(lb, mb, vb);
                            }
                        }
                        break;
                    case SystemType.DistributionTest: {
                            Array.Clear(Bodies, 0, Bodies.Length);
                            Double d = 4e4;
                            Double m = 5e6;
                            Int32 s = (Int32)Math.Pow(Bodies.Length, 1 / 3D);
                            Int32 k = 0;
                            for (Int32 a = 0; a < s; a++)
                                for (Int32 b = 0; b < s; b++)
                                    for (Int32 c = 0; c < s; c++)
                                        Bodies[k++] = new Body(d * (new Vector((a - s / 2), (b - s / 2), (c - s / 2))), m);
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
        public void Rotate(Vector point, Vector direction, Double angle) {
            lock (BodyLock) {
                Parallel.ForEach(Bodies, body => {
                    if (body != null)
                        body.Rotate(point, direction, angle);
                });
            }
        }

        /// <summary>
        /// Updates the camera properties to allow movement through the z axis. 
        /// </summary>
        private void UpdateCamera() {
            CameraZ += CameraZVelocity * CameraZ;
            CameraZ = Math.Max(1, CameraZ);
            CameraZVelocity *= CameraZEasing;

            Renderer.Camera = new Vector(0, 0, CameraZ);
            Renderer.Origin = new Point(Width / 2, Height / 2);
        }

        /// <summary>
        /// Resets the camera to its initial state. 
        /// </summary>
        public void ResetCamera() {
            CameraZ = CameraZDefault;
            CameraZVelocity = 0;
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
                for (Int32 i = 0; i < Bodies.Length; i++) {
                    Body body = Bodies[i];
                    if (body != null)
                        Renderer.FillCircle2D(g, brush, body.Location, body.Radius);
                }

                brush = new SolidBrush(Color.FromArgb(50, Color.White));
                g.DrawString("DRAW FPS: " + Math.Round(DrawFps * 10) / 10D, new Font("Arial", 8), brush, Width - 280, 10);
                g.DrawString("SIMULATION FPS: " + Math.Round(SimFps * 10) / 10D, new Font("Arial", 8), brush, Width - 170, 10);
                g.DrawString("ZONG ZHENG LI", new Font("Arial", 8), brush, new Point(Width - 120, Height - 60));

                // Update draw FPS counter. 
                DrawStopwatch.Stop();
                DrawFps += (1000D / DrawStopwatch.Elapsed.TotalMilliseconds - DrawFps) * .2;
                DrawFps = DrawFps > 3e3 ? 60 : DrawFps;
                DrawStopwatch.Reset();
                DrawStopwatch.Start();
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
            MouseIsDown = true;
        }

        /// <summary>
        /// Invoked when a mouse button is lifted up. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseUpEvent(Object sender, MouseEventArgs e) {
            MouseIsDown = false;
        }

        /// <summary>
        /// Invoked when the mouse cursor is moved. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        private void MouseMoveEvent(Object sender, MouseEventArgs e) {
            if (MouseIsDown)
                RotationHelper.MouseDrag(Rotate, e.X - MouseLocation.X, e.Y - MouseLocation.Y);
            MouseLocation.X = e.X;
            MouseLocation.Y = e.Y;
        }

        /// <summary>
        /// Invoked when the mouse wheel is scrolled. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The mouse event.</param>
        void MouseWheelEvent(Object sender, MouseEventArgs e) {
            CameraZVelocity += e.Delta * CameraZAcceleration;
        }
    }
}
