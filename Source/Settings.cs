using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NBody {

    /// <summary>
    /// This is the Settings window that controls the parameters of the simulation. 
    /// </summary>
    public partial class Settings : Form {

        public Settings() {
            InitializeComponent();
        }

        void NoneClick(Object sender, EventArgs e) {
            World.Generate(SystemType.None);
        }

        void SlowParticlesClick(Object sender, EventArgs e) {
            World.Generate(SystemType.SlowParticles);
        }

        void FastParticlesClick(Object sender, EventArgs e) {
            World.Generate(SystemType.FastParticles);
        }

        void MassiveBodyClick(Object sender, EventArgs e) {
            World.Generate(SystemType.MassiveBody);
        }

        void OrbitalSystemClick(Object sender, EventArgs e) {
            World.Generate(SystemType.OrbitalSystem);
        }

        void BinarySystemClick(Object sender, EventArgs e) {
            World.Generate(SystemType.BinarySystem);
        }

        void PlanetarySystemClick(Object sender, EventArgs e) {
            World.Generate(SystemType.PlanetarySystem);
        }

        void DistributionTestClick(Object sender, EventArgs e) {
            World.Generate(SystemType.DistributionTest);
        }

        void ChangeGClick(Object sender, EventArgs e) {
            Double.TryParse(InputBox.Show("Please specify a value for G.", World.G.ToString()), out World.G);
        }

        void changeCClick(Object sender, EventArgs e) {
            Double.TryParse(InputBox.Show("Please specify a value for C.", World.C.ToString()), out World.C);
        }

        void ChangeNClick(Object sender, EventArgs e) {
            Int32 n;
            Int32.TryParse(InputBox.Show("Please specify a value for N.", World.Bodies.Length.ToString()), out n);
            World.Bodies = World.Bodies.Length != n ? new Body[n] : World.Bodies;
        }

        void pauseClick(Object sender, EventArgs e) {
            World.Active = !World.Active;
        }
    }
}
