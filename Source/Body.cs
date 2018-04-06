using System;
using System.Drawing;
using Lattice;

namespace NBody {

    /// <summary>
    /// Represents a massive body in the simulation. 
    /// </summary>
    class Body {

        /// <summary>
        /// The spatial location of the body. 
        /// </summary>
        public Vector Location = Vector.Zero;

        /// <summary>
        /// The velocity of the body. 
        /// </summary>
        public Vector Velocity = Vector.Zero;

        /// <summary>
        /// The acceleration accumulated for the body during a single simulation 
        /// step. 
        /// </summary>
        public Vector Acceleration = Vector.Zero;

        /// <summary>
        /// The mass of the body. 
        /// </summary>
        public double Mass;

        /// <summary>
        /// The previous locations of the body in a circular queue. 
        /// </summary>
        private Vector[] _locationHistory = new Vector[20];

        /// <summary>
        /// The current index in the location history queue. 
        /// </summary>
        private int _locationHistoryIndex = 0;

        /// <summary>
        /// The radius of the body. 
        /// </summary>
        public double Radius {
            get {
                return GetRadius(Mass);
            }
        }

        /// <summary>
        /// Constructs a body with the given mass. All other properties are assigned 
        /// default values of zero. 
        /// </summary>
        /// <param name="mass">The mass of the new body.</param>
        public Body(double mass) {
            Mass = mass;
        }

        /// <summary>
        /// Constructs a body with the given location, mass, and velocity. 
        /// Unspecified properties are assigned default values of zero except for
        /// mass, which is given the value 1e6.
        /// </summary>
        /// <param name="location">The location of the new body.</param>
        /// <param name="mass">The mass of the new body.</param>
        /// <param name="velocity">The velocity of the new body.</param>
        public Body(Vector location, double mass = 1e6, Vector velocity = new Vector())
            : this(mass) {
            Location = location;
            Velocity = velocity;

            for (int i = 0; i < _locationHistory.Length; i++) {
                _locationHistory[i] = location;
            }
        }

        /// <summary>
        /// Updates the properties of the body such as location, velocity, and 
        /// applied acceleration. This method should be invoked at each time step. 
        /// </summary>
        public void Update() {
            _locationHistory[_locationHistoryIndex] = Location;
            _locationHistoryIndex = ++_locationHistoryIndex % _locationHistory.Length;

            double speed = Velocity.Magnitude();
            if (speed > World.C) {
                Velocity = World.C * Velocity.Unit();
                speed = World.C;
            }

            if (speed == 0)
                Velocity += Acceleration;
            else {

                // Apply relativistic velocity addition. 
                Vector parallelAcc = Vector.Projection(Acceleration, Velocity);
                Vector orthogonalAcc = Vector.Rejection(Acceleration, Velocity);
                double alpha = Math.Sqrt(1 - Math.Pow(speed / World.C, 2));
                Velocity = (Velocity + parallelAcc + alpha * orthogonalAcc) / (1 + Vector.Dot(Velocity, Acceleration) / (World.C * World.C));
            }

            Location += Velocity;
            Acceleration = Vector.Zero;
        }

        /// <summary>
        /// Rotates the body along an arbitrary axis. 
        /// </summary>
        /// <param name="point">The starting point for the axis of rotation.</param>
        /// <param name="direction">The direction for the axis of rotation</param>
        /// <param name="angle">The angle to rotate by.</param>
        public void Rotate(Vector point, Vector direction, double angle) {
            Location = Location.Rotate(point, direction, angle);

            // To rotate velocity and acceleration we have to adjust for the starting 
            // point for the axis of rotation. This way the vectors are effectively 
            // rotated about their own starting points. 
            Velocity += point;
            Velocity = Velocity.Rotate(point, direction, angle);
            Velocity -= point;
            Acceleration += point;
            Acceleration = Acceleration.Rotate(point, direction, angle);
            Acceleration -= point;

            if (DrawTracers) {
                for (int i = 0; i < _locationHistory.Length; i++) {
                    _locationHistory[i] = _locationHistory[i].Rotate(point, direction, angle);
                }
            }
        }

        /// <summary>
        /// Returns the radius defined for the given mass value. 
        /// </summary>
        /// <param name="mass">The mass to calculate a radius for.</param>
        /// <returns>The radius defined for the given mass value.</returns>
        public static double GetRadius(double mass) {

            // We assume all bodies have the same density so volume is directly 
            // proportion to mass. Then we use the inverse of the equation for the 
            // volume of a sphere to solve for the radius. The end result is arbitrarily 
            // scaled and added to a constant so the Body is generally visible. 
            return 10 * Math.Pow(3 * mass / (4 * Math.PI), 1 / 3.0) + 10;
        }

        #region Drawing

        /// <summary>
        /// The brush to use for drawing the body. 
        /// </summary>
        private static Brush DrawingBrush = Brushes.White;
        
        /// <summary>
        /// The pen to use for drawing tracers. 
        /// </summary>
        private static Pen TracerPen = new Pen(new SolidBrush(Color.FromArgb(40, Color.Cyan)));

        /// <summary>
        /// Determines whether to draw tracers showing history of body locations. 
        /// </summary>
        public static bool DrawTracers {
            get;
            set;
        }

        /// <summary>
        /// Draws the body. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public void Draw(Graphics g, Renderer renderer) {
            renderer.FillCircle2D(g, DrawingBrush, Location, Radius);

            if (DrawTracers) {
                for (int i = 0; i < _locationHistory.Length; i++) {
                    int j = (_locationHistoryIndex + i) % _locationHistory.Length;
                    int k = (j + 1) % _locationHistory.Length;
                    Point start = renderer.ComputePoint(_locationHistory[j]);
                    Point end = renderer.ComputePoint(_locationHistory[k]);
                    g.DrawLine(TracerPen, start, end);
                }
            }
        }

        #endregion Drawing
    }
}
