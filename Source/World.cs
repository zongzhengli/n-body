using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Lattice;
using Threading;

namespace NBody {

    /// <summary>
    /// Specifies the system type to generate. 
    /// </summary>
    enum SystemType { None, SlowParticles, FastParticles, MassiveBody, OrbitalSystem, BinarySystem, PlanetarySystem, DistributionTest };

    /// <summary>
    /// Represents the world of the simulation. 
    /// </summary>
    class World {

        /// <summary>
        /// The number of milliseconds between simulation frames. 
        /// </summary>
        private const int FrameInterval = 33;

        /// <summary>
        /// The multiplicative factor for easing the simulation FPS. 
        /// </summary>
        private const double FpsEasing = 0.2;

        /// <summary>
        /// The maximum FPS. 
        /// </summary>
        private const double FpsMax = 999.9;

        /// <summary>
        /// The camera field of view. 
        /// </summary>
        private const double CameraFOV = 9e8;

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
        private const double CameraZEasing = 0.94;

        /// <summary>
        /// The gravitational constant. 
        /// </summary>
        public static double G = 67;

        /// <summary>
        /// The maximum speed. 
        /// </summary>
        public static double C = 1e4;

        /// <summary>
        /// The world instance. 
        /// </summary>
        public static World Instance {
            get {
                if (_instance == null) {
                    _instance = new World();
                }
                return _instance;
            }
        }
        private static World _instance = null;

        /// <summary>
        /// The number of bodies allocated in the simulation. 
        /// </summary>
        public int BodyAllocationCount {
            get {
                return _bodies.Length;
            }
            set {
                if (_bodies.Length != value) {
                    lock (_bodyLock)
                        _bodies = new Body[value];
                    Frames = 0;
                }
            }
        }

        /// <summary>
        /// The number of bodies that exist in the simulation. 
        /// </summary>
        public int BodyCount {
            get;
            private set;
        }

        /// <summary>
        /// The total mass of the bodies that exist in the simulation. 
        /// </summary>
        public double TotalMass {
            get;
            private set;
        }

        /// <summary>
        /// The number of frames elapsed in the simulation. 
        /// </summary>
        public long Frames {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the simulation is active or paused. 
        /// </summary>
        public Boolean Active {
            get;
            set;
        }

        /// <summary>
        /// The simulation FPS. 
        /// </summary>
        public double Fps {
            get;
            private set;
        }

        /// <summary>
        /// The collection of bodies in the simulation. 
        /// </summary>
        private Body[] _bodies = new Body[1000];

        /// <summary>
        /// The lock that must be held to modify the bodies collection. 
        /// </summary>
        private readonly Object _bodyLock = new Object();

        /// <summary>
        /// The Renderer instance used to draw 3D graphics. 
        /// </summary>
        private Renderer _renderer = new Renderer();

        /// <summary>
        /// The stopwatch for the simulation FPS. 
        /// </summary>
        private Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// The camera's position on the z-axis. 
        /// </summary>
        private double _cameraZ = CameraZDefault;

        /// <summary>
        /// The camera's velocity along the z-axis. 
        /// </summary>
        private double _cameraZVelocity = 0;

        /// <summary>
        /// Constructs the world and starts the simulation. 
        /// </summary>
        private World() {

            // Initialize default values. 
            Active = true;
            Frames = 0;
            _renderer.Camera.Z = _cameraZ;
            _renderer.FOV = CameraFOV;

            // Start simulation thread. 
            new Thread(new ThreadStart(() => {
                while (true)
                    Simulate();
            })) {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// Advances the simulation by one frame if it is active. 
        /// </summary>
        private void Simulate() {
            if (Active)
                lock (_bodyLock) {

                    // Update the bodies and determine the required tree width. 
                    double halfWidth = 0;
                    foreach (Body body in _bodies)
                        if (body != null) {
                            body.Update();
                            halfWidth = Math.Max(Math.Abs(body.Location.X), halfWidth);
                            halfWidth = Math.Max(Math.Abs(body.Location.Y), halfWidth);
                            halfWidth = Math.Max(Math.Abs(body.Location.Z), halfWidth);
                        }

                    // Initialize the root tree and add the bodies. The root tree needs to be 
                    // slightly larger than twice the determined half width. 
                    Octree tree = new Octree(2.1 * halfWidth);
                    foreach (Body body in _bodies)
                        if (body != null)
                            tree.Add(body);

                    // Accelerate the bodies in parallel. 
                    Parallel.ForEach(_bodies, body => {
                        if (body != null)
                            tree.Accelerate(body);
                    });

                    // Update info properties. 
                    BodyCount = tree.BodyCount;
                    TotalMass = tree.Mass;
                    if (BodyCount > 0)
                        Frames++;
                }

            // Update the camera. 
            _cameraZ += _cameraZVelocity * _cameraZ;
            _cameraZ = Math.Max(1, _cameraZ);
            _cameraZVelocity *= CameraZEasing;
            _renderer.Camera.Z = _cameraZ;

            // Sleep for the necessary time. 
            int elapsed = (int)_stopwatch.ElapsedMilliseconds;
            if (elapsed < FrameInterval)
                Thread.Sleep(FrameInterval - elapsed);

            // Update the simulation FPS.
            _stopwatch.Stop();
            Fps += (1000.0 / _stopwatch.Elapsed.TotalMilliseconds - Fps) * FpsEasing;
            Fps = Math.Min(Fps, FpsMax);
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        /// <summary>
        /// Generates the specified gravitational system. 
        /// </summary>
        /// <param name="type">The system type to generate.</param>
        public void Generate(SystemType type) {

            // Reset frames elapsed. 
            Frames = 0;

            lock (_bodyLock) {
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
                                l = l.Rotate(0, 0, 0, 1, 1, 1, Math.PI * 0.1);
                                v = v.Rotate(0, 0, 0, 1, 1, 1, Math.PI * 0.1);
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
        /// Rotates the world by calling the bodies' rotate methods. 
        /// </summary>
        /// <param name="point">The starting point for the axis of rotation.</param>
        /// <param name="direction">The direction for the axis of rotation</param>
        /// <param name="angle">The angle to rotate by.</param>
        public void Rotate(Vector point, Vector direction, double angle) {
            lock (_bodyLock) {
                Parallel.ForEach(_bodies, body => {
                    if (body != null)
                        body.Rotate(point, direction, angle);
                });
            }
        }

        /// <summary>
        /// Moves the camera in association with the given mouse wheel delta. 
        /// </summary>
        /// <param name="delta">The signed number of dents the mouse wheel moved.</param>
        public void MoveCamera(int delta) {
            _cameraZVelocity += delta * CameraZAcceleration;
        }

        /// <summary>
        /// Resets the camera to its initial position. 
        /// </summary>
        public void ResetCamera() {
            _cameraZ = CameraZDefault;
            _cameraZVelocity = 0;
        }

        /// <summary>
        /// Draws the bodies in the world. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public void Draw(Graphics g) {
            using (SolidBrush brush = new SolidBrush(Color.White)) {
                for (int i = 0; i < _bodies.Length; i++)
                    if (_bodies[i] != null) {
                        Body body = _bodies[i];
                        _renderer.FillCircle2D(g, brush, body.Location, body.Radius);
                    }
            }
        }
    }
}
