using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NBody {

    /// <summary>
    /// Represents a window that controls the parameters of the simulation. 
    /// </summary>
    public partial class Settings : Form {

        public Settings() {
            InitializeComponent();
        }

        private void NoneClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.None);
        }

        private void SlowParticlesClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.SlowParticles);
        }

        private void FastParticlesClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.FastParticles);
        }

        private void MassiveBodyClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.MassiveBody);
        }

        private void OrbitalSystemClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.OrbitalSystem);
        }

        private void BinarySystemClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.BinarySystem);
        }

        private void PlanetarySystemClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.PlanetarySystem);
        }

        private void DistributionTestClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.DistributionTest);
        }

        private void ChangeGClick(Object sender, EventArgs e) {
            double.TryParse(InputBox.Show("Please specify a value for G.", World.G.ToString()), out World.G);
        }

        private void ChangeCClick(Object sender, EventArgs e) {
            double.TryParse(InputBox.Show("Please specify a value for C.", World.C.ToString()), out World.C);
        }

        private void ChangeNClick(Object sender, EventArgs e) {
            int n;
            int.TryParse(InputBox.Show("Please specify a value for N.", World.Instance.BodyAllocationCount.ToString()), out n);
            World.Instance.BodyAllocationCount = n;
        }

        private void PauseClick(Object sender, EventArgs e) {
            World.Instance.Active = !World.Instance.Active;
        }

        private void ResetCameraClick(Object sender, EventArgs e) {
            World.Instance.ResetCamera();
        }
    }
}
