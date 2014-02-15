using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NBody {

    /// <summary>
    /// Represents a window that controls the parameters of the simulation. 
    /// </summary>
    public partial class Settings : Form {

        /// <summary>
        /// Constructs a settings window. 
        /// </summary>
        public Settings() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the None button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void NoneClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.None);
        }

        /// <summary>
        /// Handles the Slow Particles button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void SlowParticlesClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.SlowParticles);
        }

        /// <summary>
        /// Handles the Fast Particles button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void FastParticlesClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.FastParticles);
        }

        /// <summary>
        /// Handles the Massive Body button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void MassiveBodyClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.MassiveBody);
        }

        /// <summary>
        /// Handles the Orbital System button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void OrbitalSystemClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.OrbitalSystem);
        }

        /// <summary>
        /// Handles the Binary System button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void BinarySystemClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.BinarySystem);
        }

        /// <summary>
        /// Handles the Planetary System button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void PlanetarySystemClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.PlanetarySystem);
        }

        /// <summary>
        /// Handles the Distribution Test button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void DistributionTestClick(Object sender, EventArgs e) {
            World.Instance.Generate(SystemType.DistributionTest);
        }

        /// <summary>
        /// Handles the Change G button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void ChangeGClick(Object sender, EventArgs e) {
            Double.TryParse(InputBox.Show("Please specify a value for G.", World.G.ToString()), out World.G);
        }

        /// <summary>
        /// Handles the Change C button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void ChangeCClick(Object sender, EventArgs e) {
            Double.TryParse(InputBox.Show("Please specify a value for C.", World.C.ToString()), out World.C);
        }

        /// <summary>
        /// Handles the Change N button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void ChangeNClick(Object sender, EventArgs e) {
            int n;
            Int32.TryParse(InputBox.Show("Please specify a value for N.", World.Instance.BodyAllocationCount.ToString()), out n);
            World.Instance.BodyAllocationCount = n;
        }

        /// <summary>
        /// Handles the Pause button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void PauseClick(Object sender, EventArgs e) {
            World.Instance.Active ^= true;
            pause.Text = World.Instance.Active ? "Pause" : "Resume";
        }

        /// <summary>
        /// Handles the Reset Camera button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void ResetCameraClick(Object sender, EventArgs e) {
            World.Instance.ResetCamera();
        }

        /// <summary>
        /// Handles the Reset Camera button click. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void ShowTreeClick(object sender, EventArgs e) {
            World.Instance.DrawTree ^= true;
            showTree.Text = (World.Instance.DrawTree ? "Hide" : "Show") + " Tree";
        }
    }
}
