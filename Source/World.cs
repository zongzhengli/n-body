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
            get {
                return _tree == null ? 0 : _tree.BodyCount;
            }
        }

        /// <summary>
        /// The total mass of the bodies that exist in the simulation. 
        /// </summary>
        public double TotalMass {
            get {
                return _tree == null ? 0 : _tree.Mass;
            }
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
        /// Determines whether to draw the tree structure for calculating forces. 
        /// </summary>
        public Boolean DrawTree {
            get;
            set;
        }

        /// <summary>
        /// Determines whether to draw tracers showing history of body locations. 
        /// </summary>
        public Boolean DrawTracers {
            get {
                return Body.DrawTracers;
            }
            set {
                Body.DrawTracers = value;
            }
        }

        /// <summary>
        /// The collection of bodies in the simulation. 
        /// </summary>
        private Body[] _bodies = new Body[1000];

        /// <summary>
        /// The lock for modifying the bodies collection. 
        /// </summary>
        private readonly Object _bodyLock = new Object();

        /// <summary>
        /// The tree for calculating forces. 
        /// </summary>
        private Octree _tree;

        /// <summary>
        /// The Renderer instance for drawing 3D graphics. 
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
                    _tree = new Octree(2.1 * halfWidth);
                    foreach (Body body in _bodies)
                        if (body != null)
                            _tree.Add(body);

                    // Accelerate the bodies in parallel. 
                    Parallel.ForEach(_bodies, body => {
                        if (body != null)
                            _tree.Accelerate(body);
                    });

                    // Update frame counter. 
                    if (_tree.BodyCount > 0)
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

                    // Clear bodies. 
                    case SystemType.None:
                        Array.Clear(_bodies, 0, _bodies.Length);
                        break;

                    // Generate slow particles. 
                    case SystemType.SlowParticles: {
                            for (int i = 0; i < _bodies.Length; i++) {
                                double distance = PseudoRandom.Double(1e6);
                                double angle    = PseudoRandom.Double(Math.PI * 2);
                                Vector location = new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-2e5, 2e5), Math.Sin(angle) * distance);
                                double mass     = PseudoRandom.Double(1e6) + 3e4;
                                Vector velocity = PseudoRandom.Vector(5);
                                _bodies[i] = new Body(location, mass, velocity);
                            }
                        }
                        break;

                    // Generate fast particles. 
                    case SystemType.FastParticles: {
                            for (int i = 0; i < _bodies.Length; i++) {
                                double distance = PseudoRandom.Double(1e6);
                                double angle    = PseudoRandom.Double(Math.PI * 2);
                                Vector location = new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-2e5, 2e5), Math.Sin(angle) * distance);
                                double mass     = PseudoRandom.Double(1e6) + 3e4;
                                Vector velocity = PseudoRandom.Vector(5e3);
                                _bodies[i] = new Body(location, mass, velocity);
                            }
                        }
                        break;

                    // Generate massive body demonstration. 
                    case SystemType.MassiveBody: {
                            _bodies[0] = new Body(Vector.Zero, 1e10);

                            Vector location1 = PseudoRandom.Vector(8e3) + new Vector(-3e5, 1e5 + _bodies[0].Radius, 0);
                            double mass1     = 1e6;
                            Vector velocity1 = new Vector(2e3, 0, 0);
                            _bodies[1] = new Body(location1, mass1, velocity1);

                            for (int i = 2; i < _bodies.Length; i++) {
                                double distance = PseudoRandom.Double(2e5) + _bodies[1].Radius;
                                double angle    = PseudoRandom.Double(Math.PI * 2);
                                double vertical = Math.Min(2e8 / distance, 2e4);
                                Vector location = (new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-vertical, vertical), Math.Sin(angle) * distance) + _bodies[1].Location);
                                double mass     = PseudoRandom.Double(5e5) + 1e5;
                                double speed    = Math.Sqrt(_bodies[1].Mass * _bodies[1].Mass * G / ((_bodies[1].Mass + mass) * distance));
                                Vector velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed + velocity1;
                                location = location.Rotate(0, 0, 0, 1, 1, 1, Math.PI * 0.1);
                                velocity = velocity.Rotate(0, 0, 0, 1, 1, 1, Math.PI * 0.1);
                                _bodies[i] = new Body(location, mass, velocity);
                            }
                        }
                        break;

                    // Generate orbital system. 
                    case SystemType.OrbitalSystem: {
                            _bodies[0] = new Body(1e10);

                            for (int i = 1; i < _bodies.Length; i++) {
                                double distance = PseudoRandom.Double(1e6) + _bodies[0].Radius;
                                double angle    = PseudoRandom.Double(Math.PI * 2);
                                Vector location = new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                                double mass     = PseudoRandom.Double(1e6) + 3e4;
                                double speed    = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + mass) * distance));
                                Vector velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed;
                                _bodies[i] = new Body(location, mass, velocity);
                            }
                        }
                        break;

                    // Generate binary system. 
                    case SystemType.BinarySystem: {
                            double mass1     = PseudoRandom.Double(9e9) + 1e9;
                            double mass2     = PseudoRandom.Double(9e9) + 1e9;
                            double angle0    = PseudoRandom.Double(Math.PI * 2);
                            double distance0 = PseudoRandom.Double(1e5) + 3e4;
                            double distance1 = distance0 / 2;
                            double distance2 = distance0 / 2;
                            Vector location1 = new Vector(Math.Cos(angle0) * distance1, 0, Math.Sin(angle0) * distance1);
                            Vector location2 = new Vector(-Math.Cos(angle0) * distance2, 0, -Math.Sin(angle0) * distance2);
                            double speed1    = Math.Sqrt(mass2 * mass2 * G / ((mass1 + mass2) * distance0));
                            double speed2    = Math.Sqrt(mass1 * mass1 * G / ((mass1 + mass2) * distance0));
                            Vector velocity1 = Vector.Cross(location1, Vector.YAxis).Unit() * speed1;
                            Vector velocity2 = Vector.Cross(location2, Vector.YAxis).Unit() * speed2;
                            _bodies[0] = new Body(location1, mass1, velocity1);
                            _bodies[1] = new Body(location2, mass2, velocity2);

                            for (int i = 2; i < _bodies.Length; i++) {
                                double distance = PseudoRandom.Double(1e6);
                                double angle    = PseudoRandom.Double(Math.PI * 2);
                                Vector location = new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                                double mass     = PseudoRandom.Double(1e6) + 3e4;
                                double speed    = Math.Sqrt((mass1 + mass2) * (mass1 + mass2) * G / ((mass1 + mass2 + mass) * distance));
                                speed /= distance >= distance0 / 2 ? 1 :  (distance0 / 2 / distance);
                                Vector velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed;
                                _bodies[i] = new Body(location, mass, velocity);
                            }
                        }
                        break;

                    // Generate planetary system. 
                    case SystemType.PlanetarySystem: {
                            _bodies[0] = new Body(1e10);
                            int planets = PseudoRandom.Int32(10) + 5;
                            int planetsWithRings = PseudoRandom.Int32(1) + 1;
                            int k = 1;
                            for (int i = 1; i < planets + 1 && k < _bodies.Length; i++) {
                                int planetK = k;
                                double distance = PseudoRandom.Double(2e6) + 1e5 + _bodies[0].Radius;
                                double angle    = PseudoRandom.Double(Math.PI * 2);
                                Vector location = new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                                double mass     = PseudoRandom.Double(1e8) + 1e7;
                                double speed    = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + mass) * distance));
                                Vector velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed;
                                _bodies[k++] = new Body(location, mass, velocity);

                                // Generate rings.
                                const int RingParticles = 100;
                                if (--planetsWithRings >= 0 && k < _bodies.Length - RingParticles) {
                                    for (int j = 0; j < RingParticles; j++) {
                                        double ringDistance = PseudoRandom.Double(1e1) + 1e4 + _bodies[planetK].Radius;
                                        double ringAngle    = PseudoRandom.Double(Math.PI * 2);
                                        Vector ringLocation = location + new Vector(Math.Cos(ringAngle) * ringDistance, 0, Math.Sin(ringAngle) * ringDistance);
                                        double ringMass     = PseudoRandom.Double(1e3) + 1e3;
                                        double ringSpeed    = Math.Sqrt(_bodies[planetK].Mass * _bodies[planetK].Mass * G / ((_bodies[planetK].Mass + ringMass) * ringDistance));
                                        Vector ringVelocity = Vector.Cross(location - ringLocation, Vector.YAxis).Unit() * ringSpeed + velocity;
                                        _bodies[k++] = new Body(ringLocation, ringMass, ringVelocity);
                                    }
                                    continue;
                                }

                                // Generate moons. 
                                int moons = PseudoRandom.Int32(4);
                                while (moons-- > 0 && k < _bodies.Length) {
                                    double moonDistance = PseudoRandom.Double(1e4) + 5e3 + _bodies[planetK].Radius;
                                    double moonAngle    = PseudoRandom.Double(Math.PI * 2);
                                    Vector moonLocation = location + new Vector(Math.Cos(moonAngle) * moonDistance, PseudoRandom.Double(-2e3, 2e3), Math.Sin(moonAngle) * moonDistance);
                                    double moonMass     = PseudoRandom.Double(1e6) + 1e5;
                                    double moonSpeed    = Math.Sqrt(_bodies[planetK].Mass * _bodies[planetK].Mass * G / ((_bodies[planetK].Mass + moonMass) * moonDistance));
                                    Vector moonVelocity = Vector.Cross(moonLocation - location, Vector.YAxis).Unit() * moonSpeed + velocity;
                                    _bodies[k++] = new Body(moonLocation, moonMass, moonVelocity);
                                }
                            }

                            // Generate asteroid belt.
                            while (k < _bodies.Length) {
                                double asteroidDistance = PseudoRandom.Double(4e5) + 1e6;
                                double asteroidAngle    = PseudoRandom.Double(Math.PI * 2);
                                Vector asteroidLocation = new Vector(Math.Cos(asteroidAngle) * asteroidDistance, PseudoRandom.Double(-1e3, 1e3), Math.Sin(asteroidAngle) * asteroidDistance);
                                double asteroidMass     = PseudoRandom.Double(1e6) + 3e4;
                                double asteroidSpeed    = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + asteroidMass) * asteroidDistance));
                                Vector asteroidVelocity = Vector.Cross(asteroidLocation, Vector.YAxis).Unit() * asteroidSpeed;
                                _bodies[k++] = new Body(asteroidLocation, asteroidMass, asteroidVelocity);
                            }
                        }
                        break;

                    // Generate distribution test. 
                    case SystemType.DistributionTest: {
                            Array.Clear(_bodies, 0, _bodies.Length);
                            double distance = 4e4;
                            double mass     = 5e6;

                            int side = (int)Math.Pow(_bodies.Length, 1.0 / 3);
                            int k = 0;
                            for (int a = 0; a < side; a++)
                                for (int b = 0; b < side; b++)
                                    for (int c = 0; c < side; c++)
                                        _bodies[k++] = new Body(distance * (new Vector(a - side / 2, b - side / 2, c - side / 2)), mass);
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
            lock (_bodyLock) 
                Parallel.ForEach(_bodies, body => {
                    if (body != null)
                        body.Rotate(point, direction, angle);
                });
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
            Pen treePen = new Pen(new SolidBrush(Color.FromArgb(100, Color.Red)));
            Pen bodyPen = new Pen(new SolidBrush(Color.FromArgb(100, Color.Red)));


            for (int i = 0; i < _bodies.Length; i++) {
                Body body = _bodies[i];
                if (body != null)
                    body.Draw(g, _renderer);
            }

            if (DrawTree)
                _tree.Draw(g, _renderer);
        }
    }
}
