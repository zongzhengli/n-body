using System;
using System.Drawing;
using Lattice;

namespace NBody {

    /// <summary>
    /// The tree structure in the Barnes-Hut algorithm. 
    /// </summary>
    class Octree {

        /// <summary>
        /// Defines the accuracy-speed tradeoff value of the simulation. The acceleration of a Body by the Octree 
        /// is only calculated when the ratio of the width to distance (from the tree's center of mass to the Body) 
        /// ratio is less than this.
        /// </summary>
        private const Double Tolerance = .5;

        /// <summary>
        /// Defines the minimum width of an Octree. Children are not created if their width would be smaller than 
        /// this value. 
        /// </summary>
        private const Double MinimumWidth = 1;

        /// <summary>
        /// The uninitialized subtrees, each as a separate field. Each is defined individually as an array 
        /// of subtrees didn't perform as well due to bounds checking. 
        /// </summary>
        private Octree SubtreeA;
        private Octree SubtreeB;
        private Octree SubtreeC;
        private Octree SubtreeD;
        private Octree SubtreeE;
        private Octree SubtreeF;
        private Octree SubtreeG;
        private Octree SubtreeH;

        /// <summary>
        /// The value and position of the center of mass of the tree. These fields are incrementally updated 
        /// when a Body is added. 
        /// </summary>
        private Vector CenterOfMass = Vector.Zero;
        private Double Mass = 0;

        /// <summary>
        /// The spatial properties of the tree. These fields define the bounds of the tree. 
        /// </summary>
        private Double X = 0;
        private Double Y = 0;
        private Double Z = 0;
        private Double Width = 0;

        /// <summary>
        /// The number of Bodies in the tree. This helps determine when to add bodies to children. 
        /// </summary>
        private Int32 Bodies = 0;

        /// <summary>
        /// The first Body added to the tree. This is used when the first Body must be added to children
        /// at a later time. 
        /// </summary>
        private Body FirstBody;

        /// <summary>
        /// Constructs an Octree with the given width located about the origin.
        /// </summary>
        /// <param name="width">The width of the new Octree.</param>
        public Octree(Double width) {
            Width = width;
        }

        /// <summary>
        /// Constructs an Octree with the given location and width.
        /// </summary>
        /// <param name="x">The x coordinate of the location of the new Octree.</param>
        /// <param name="y">The y coordinate of the location of the new Octree.</param>
        /// <param name="z">The z coordinate of the location of the new Octree.</param>
        /// <param name="width">The width of the new Octree.</param>
        public Octree(Double x, Double y, Double z, Double width)
            : this(width) {
            X = CenterOfMass.X = x;
            Y = CenterOfMass.Y = y;
            Z = CenterOfMass.Z = z;
        }

        /// <summary>
        /// Adds a Body to the tree and updates internal tree properties. If the tree contains more than one 
        /// Body afterwards, the Body is also added to the appropriate subtree. 
        /// </summary>
        /// <param name="body">The Body to add to the tree.</param>
        public void Add(Body body) {
            CenterOfMass.X = (Mass * CenterOfMass.X + body.Mass * body.X) / (Mass + body.Mass);
            CenterOfMass.Y = (Mass * CenterOfMass.Y + body.Mass * body.Y) / (Mass + body.Mass);
            CenterOfMass.Z = (Mass * CenterOfMass.Z + body.Mass * body.Z) / (Mass + body.Mass);
            Mass += body.Mass;
            Bodies++;
            if (Bodies == 1)
                FirstBody = body;
            else
                AddToChildren(body);
            if (Bodies == 2)
                AddToChildren(FirstBody);
        }

        /// <summary>
        /// Adds a Body to the appropriate subtree based on its position in space. The subtree is initialized 
        /// if it has not already been done so. 
        /// </summary>
        /// <param name="body">The Body to add to a subtree.</param>
        private void AddToChildren(Body body) {
            if (Width < MinimumWidth * 2)
                return;
            if (body.Y < Y) {
                if (body.X < X) {
                    if (body.Z < Z) {
                        if (SubtreeA == null)
                            SubtreeA = new Octree(X - .25 * Width, Y - .25 * Width, Z - .25 * Width, .5 * Width);
                        SubtreeA.Add(body);
                    } else if (body.Z > Z) {
                        if (SubtreeB == null)
                            SubtreeB = new Octree(X - .25 * Width, Y - .25 * Width, Z + .25 * Width, .5 * Width);
                        SubtreeB.Add(body);
                    }
                } else {
                    if (body.Z < Z) {
                        if (SubtreeC == null)
                            SubtreeC = new Octree(X + .25 * Width, Y - .25 * Width, Z - .25 * Width, .5 * Width);
                        SubtreeC.Add(body);
                    } else if (body.Z > Z) {
                        if (SubtreeD == null)
                            SubtreeD = new Octree(X + .25 * Width, Y - .25 * Width, Z + .25 * Width, .5 * Width);
                        SubtreeD.Add(body);
                    }
                }
            } else {
                if (body.X < X) {
                    if (body.Z < Z) {
                        if (SubtreeE == null)
                            SubtreeE = new Octree(X - .25 * Width, Y + .25 * Width, Z - .25 * Width, .5 * Width);
                        SubtreeE.Add(body);
                    } else if (body.Z > Z) {
                        if (SubtreeF == null)
                            SubtreeF = new Octree(X - .25 * Width, Y + .25 * Width, Z + .25 * Width, .5 * Width);
                        SubtreeF.Add(body);
                    }
                } else {
                    if (body.Z < Z) {
                        if (SubtreeG == null)
                            SubtreeG = new Octree(X + .25 * Width, Y + .25 * Width, Z - .25 * Width, .5 * Width);
                        SubtreeG.Add(body);
                    } else if (body.Z > Z) {
                        if (SubtreeH == null)
                            SubtreeH = new Octree(X + .25 * Width, Y + .25 * Width, Z + .25 * Width, .5 * Width);
                        SubtreeH.Add(body);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the acceleration of a Body based on the properties of the tree. 
        /// </summary>
        /// <param name="body">The Body to accelerate.</param>
        public void Accelerate(Body body) {
            Double dx = CenterOfMass.X - body.X;
            Double dy = CenterOfMass.Y - body.Y;
            Double dz = CenterOfMass.Z - body.Z;

            // Case 1. The tree contains only one Body and it isn't the Body to be accelerated. The second 
            //         condition (optimized for speed) determines if the Body to be accelerated lies outside 
            //         the bounds of the tree. If it does, it can't be the single Body so we perform the 
            //         acceleration. 
            if (Bodies == 1 
                && ((body.X - X) * (body.X - X) * 4 > Width * Width 
                   || (body.Y - Y) * (body.Y - Y) * 4 > Width * Width 
                   || (body.Z - Z) * (body.Z - Z) * 4 > Width * Width))
                PerformAcceleration(body, dx, dy, dz);

            // Case 2. The width to distance ratio is within Tolerance, so we perform the acceleration. This  
            //         condition is an optimized equivalent of Width / (distance) < Tolerance. 
            else if (Width * Width < Tolerance * Tolerance * (dx * dx + dy * dy + dz * dz))
                PerformAcceleration(body, dx, dy, dz);

            // Case 3. We can't perform the acceleration, so we pass the Body on to the children for a more 
            //         precise calculation. 
            else {
                if (SubtreeA != null)
                    SubtreeA.Accelerate(body);
                if (SubtreeB != null)
                    SubtreeB.Accelerate(body);
                if (SubtreeC != null)
                    SubtreeC.Accelerate(body);
                if (SubtreeD != null)
                    SubtreeD.Accelerate(body);
                if (SubtreeE != null)
                    SubtreeE.Accelerate(body);
                if (SubtreeF != null)
                    SubtreeF.Accelerate(body);
                if (SubtreeG != null)
                    SubtreeG.Accelerate(body);
                if (SubtreeH != null)
                    SubtreeH.Accelerate(body);
            }
        }

        /// <summary>
        /// Calculates and applies the appropriate acceleration for a Body.
        /// </summary>
        /// <param name="body">The Body to accelerate.</param>
        /// <param name="dx">The difference between the tree's center of mass and the Body's position in the x axis.</param>
        /// <param name="dy">The difference between the tree's center of mass and the Body's position in the y axis.</param>
        /// <param name="dz">The difference between the tree's center of mass and the Body's position in the z axis.</param>
        private void PerformAcceleration(Body body, Double dx, Double dy, Double dz) {

            // Don't accelerate the Body if the center of mass is within its radius, to prevent crazy slingshot. 
            // This condition is an optimized equivalent of (distance) < body.Radius. 
            if (dx * dx + dy * dy + dz * dz < body.Radius * body.Radius)
                return;

            // Calculate a normalized acceleration value and multiply it with the displacement in each coordinate
            // to get that coordinate's acceleration componenet. 
            Double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            Double normAcc = World.G * Mass / (distance * distance * distance);

            body.Acceleration.X += normAcc * dx;
            body.Acceleration.Y += normAcc * dy;
            body.Acceleration.Z += normAcc * dz;
        }
    }
}
