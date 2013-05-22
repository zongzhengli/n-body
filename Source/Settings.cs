using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NBody {

    /// <summary>
    /// The Settings window that controls the parameters of the simulation. 
    /// </summary>
    public partial class Settings : Form {

        public Settings() {
            InitializeComponent();
        }

        private void NoneClick(Object sender, EventArgs e) {
            World.Generate(SystemType.None);
        }

        private void SlowParticlesClick(Object sender, EventArgs e) {
            World.Generate(SystemType.SlowParticles);
        }

        private void FastParticlesClick(Object sender, EventArgs e) {
            World.Generate(SystemType.FastParticles);
        }

        private void MassiveBodyClick(Object sender, EventArgs e) {
            World.Generate(SystemType.MassiveBody);
        }

        private void OrbitalSystemClick(Object sender, EventArgs e) {
            World.Generate(SystemType.OrbitalSystem);
        }

        private void BinarySystemClick(Object sender, EventArgs e) {
            World.Generate(SystemType.BinarySystem);
        }

        private void PlanetarySystemClick(Object sender, EventArgs e) {
            World.Generate(SystemType.PlanetarySystem);
        }

        private void DistributionTestClick(Object sender, EventArgs e) {
            World.Generate(SystemType.DistributionTest);
        }

        private void ChangeGClick(Object sender, EventArgs e) {
            Double.TryParse(InputBox.Show("Please specify a value for G.", World.G.ToString()), out World.G);
        }

        private void changeCClick(Object sender, EventArgs e) {
            Double.TryParse(InputBox.Show("Please specify a value for C.", World.C.ToString()), out World.C);
        }

        private void ChangeNClick(Object sender, EventArgs e) {
            Int32 n;
            Int32.TryParse(InputBox.Show("Please specify a value for N.", World.Bodies.Length.ToString()), out n);
            World.Bodies = World.Bodies.Length != n ? new Body[n] : World.Bodies;
        }

        private void PauseClick(Object sender, EventArgs e) {
            World.Active = !World.Active;
        }
    }
}
