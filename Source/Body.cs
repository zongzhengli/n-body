using System;
using System.Drawing;
using Lattice;

namespace NBody {

    /// <summary>
    /// Represents a massive body in the simulation. 
    /// </summary>
    class Body {

        /// <summary>
        /// Returns the radius defined for the given mass value. 
        /// </summary>
        /// <param name="mass">The mass to calculate a radius for.</param>
        /// <returns>The radius defined for the given mass value.</returns>
        public static Double GetRadius(Double mass) {
            return 10 * Math.Pow(3 * mass / (4 * Math.PI), 1 / 3D) + 1;
        }

        public Double X = 0;
        public Double Y = 0;
        public Double Z = 0;
        public Vector Velocity = Vector.Zero;
        public Vector Acceleration = Vector.Zero;
        public Double Mass;
        public Double Radius;

        /// <summary>
        /// Constructs a Body with the given mass. All other properties are assigned default values of zero. 
        /// </summary>
        /// <param name="mass">The mass of the new Body.</param>
        public Body(Double mass) {
            Mass = mass;
            Radius = GetRadius(mass);
        }

        /// <summary>
        /// Constructs a Body with the given properties. Unspecified properties are assigned default values of zero
        /// except for mass, which is given a value of 1e6.
        /// </summary>
        /// <param name="x">The x coordinate of the location of the new Body.</param>
        /// <param name="y">The y coordinate of the location of the new Body.</param>
        /// <param name="z">The z coordinate of the location of the new Body.</param>
        /// <param name="mass">The mass of the new Body.</param>
        /// <param name="velocity">The velocity of the new Body.</param>
        public Body(Double x, Double y, Double z, Double mass = 1e6, Vector velocity = new Vector())
            : this(mass) {
            X = x;
            Y = y;
            Z = z;
            Velocity = velocity;
        }

        /// <summary>
        /// Constructs a Body with the given location, mass, and velocity. Unspecified properties are assigned default values of zero
        /// except for mass, which is given a value of 1e6.
        /// </summary>
        /// <param name="location">The location of the new Body.</param>
        /// <param name="mass">The mass of the new Body.</param>
        /// <param name="velocity">The velocity of the new Body.</param>
        public Body(Vector location, Double mass = 1e6, Vector velocity = new Vector())
            : this(location.X, location.Y, location.Z, mass, velocity) {
        }

        /// <summary>
        /// Constructs a Body with the given location, mass, and velocity. 
        /// </summary>
        /// <param name="x">The x coordinate of the location of the new Body.</param>
        /// <param name="y">The x coordinate of the location of the new Body.</param>
        /// <param name="z">The x coordinate of the location of the new Body.</param>
        /// <param name="mass">The mass of the new Body.</param>
        /// <param name="velocityX">The x component of the velocity of the new Body.</param>
        /// <param name="velocityY">The y component of the velocity of the new Body.</param>
        /// <param name="velocityZ">The z component of the velocity of the new Body.</param>
        public Body(Double x, Double y, Double z, Double mass, Double velocityX, Double velocityY, Double velocityZ)
            : this(x, y, z, mass, new Vector(velocityX, velocityY, velocityZ)) {
        }

        /// <summary>
        /// Updates the properties of the body. This method should be invoked at each time step. 
        /// </summary>
        public void Update() {
            Velocity += Acceleration;
            Double speed = Velocity.Magnitude();
            if (speed > World.C)
                Velocity *= World.C / speed;
            X += Velocity.X;
            Y += Velocity.Y;
            Z += Velocity.Z;
            Acceleration = Vector.Zero;
        }

        /// <summary>
        /// Rotates the body along an arbitrary axis. 
        /// </summary>
        /// <param name="point">The starting point for the axis of rotation.</param>
        /// <param name="direction">The direction for the axis of ration</param>
        /// <param name="angle">The angle to rotate by.</param>
        public void Rotate(Vector point, Vector direction, Double angle) {

            // rotate vector representing the position coordinates, then set the coordinates to the rotated vector
            Vector vector = new Vector(X, Y, Z);
            vector = vector.Rotate(point, direction, angle);
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;

            // rotate velocity and acceleration
            Velocity += point;
            Velocity = Velocity.Rotate(point, direction, angle);
            Velocity -= point;
            Acceleration += point;
            Acceleration = Acceleration.Rotate(point, direction, angle);
            Acceleration -= point;
        }

        /// <summary>
        /// Rotates the body along an arbitrary axis. 
        /// </summary>
        /// <param name="pX">The x coordinate of the starting point for the axis of rotation.</param>
        /// <param name="pY">The y coordinate of the starting point for the axis of rotation.</param>
        /// <param name="pZ">The z coordinate of the starting point for the axis of rotation.</param>
        /// <param name="dX">The x coordinate of the vector for axis of rotation.</param>
        /// <param name="dY">The y coordinate of the vector for axis of rotation.</param>
        /// <param name="dZ">The z coordinate of the vector for axis of rotation.</param>
        /// <param name="angle">The angle to rotate by.</param>
        public virtual void Rotate(Double pX, Double pY, Double pZ, Double dX, Double dY, Double dZ, Double angle) {
            Rotate(new Vector(pX, pY, pZ), new Vector(dX, dY, dZ), angle);
        }
    }
}
