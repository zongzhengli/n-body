namespace NBody {
    partial class Settings {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.none = new System.Windows.Forms.Button();
            this.slowParticles = new System.Windows.Forms.Button();
            this.fastParticles = new System.Windows.Forms.Button();
            this.massiveBody = new System.Windows.Forms.Button();
            this.orbitalSystem = new System.Windows.Forms.Button();
            this.binarySystem = new System.Windows.Forms.Button();
            this.planetarySystem = new System.Windows.Forms.Button();
            this.distributionTest = new System.Windows.Forms.Button();
            this.changeG = new System.Windows.Forms.Button();
            this.changeN = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.systemsTab = new System.Windows.Forms.TabPage();
            this.systemsLabel = new System.Windows.Forms.Label();
            this.parametersTab = new System.Windows.Forms.TabPage();
            this.changeC = new System.Windows.Forms.Button();
            this.parametersLabel = new System.Windows.Forms.Label();
            this.functionsTab = new System.Windows.Forms.TabPage();
            this.showTracers = new System.Windows.Forms.Button();
            this.showTree = new System.Windows.Forms.Button();
            this.resetCamera = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pause = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.systemsTab.SuspendLayout();
            this.parametersTab.SuspendLayout();
            this.functionsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // none
            // 
            this.none.Location = new System.Drawing.Point(7, 28);
            this.none.Name = "none";
            this.none.Size = new System.Drawing.Size(100, 23);
            this.none.TabIndex = 0;
            this.none.Text = "None";
            this.none.UseVisualStyleBackColor = true;
            this.none.Click += new System.EventHandler(this.NoneClick);
            // 
            // slowParticles
            // 
            this.slowParticles.Location = new System.Drawing.Point(113, 28);
            this.slowParticles.Name = "slowParticles";
            this.slowParticles.Size = new System.Drawing.Size(100, 23);
            this.slowParticles.TabIndex = 1;
            this.slowParticles.Text = "Slow Particles";
            this.slowParticles.UseVisualStyleBackColor = true;
            this.slowParticles.Click += new System.EventHandler(this.SlowParticlesClick);
            // 
            // fastParticles
            // 
            this.fastParticles.Location = new System.Drawing.Point(219, 28);
            this.fastParticles.Name = "fastParticles";
            this.fastParticles.Size = new System.Drawing.Size(100, 23);
            this.fastParticles.TabIndex = 2;
            this.fastParticles.Text = "Fast Particles";
            this.fastParticles.UseVisualStyleBackColor = true;
            this.fastParticles.Click += new System.EventHandler(this.FastParticlesClick);
            // 
            // massiveBody
            // 
            this.massiveBody.Location = new System.Drawing.Point(325, 28);
            this.massiveBody.Name = "massiveBody";
            this.massiveBody.Size = new System.Drawing.Size(100, 23);
            this.massiveBody.TabIndex = 4;
            this.massiveBody.Text = "Massive Body";
            this.massiveBody.UseVisualStyleBackColor = true;
            this.massiveBody.Click += new System.EventHandler(this.MassiveBodyClick);
            // 
            // orbitalSystem
            // 
            this.orbitalSystem.Location = new System.Drawing.Point(7, 57);
            this.orbitalSystem.Name = "orbitalSystem";
            this.orbitalSystem.Size = new System.Drawing.Size(100, 23);
            this.orbitalSystem.TabIndex = 5;
            this.orbitalSystem.Text = "Orbital System";
            this.orbitalSystem.UseVisualStyleBackColor = true;
            this.orbitalSystem.Click += new System.EventHandler(this.OrbitalSystemClick);
            // 
            // binarySystem
            // 
            this.binarySystem.Location = new System.Drawing.Point(113, 57);
            this.binarySystem.Name = "binarySystem";
            this.binarySystem.Size = new System.Drawing.Size(100, 23);
            this.binarySystem.TabIndex = 6;
            this.binarySystem.Text = "Binary System";
            this.binarySystem.UseVisualStyleBackColor = true;
            this.binarySystem.Click += new System.EventHandler(this.BinarySystemClick);
            // 
            // planetarySystem
            // 
            this.planetarySystem.Location = new System.Drawing.Point(219, 57);
            this.planetarySystem.Name = "planetarySystem";
            this.planetarySystem.Size = new System.Drawing.Size(100, 23);
            this.planetarySystem.TabIndex = 7;
            this.planetarySystem.Text = "Planetary System";
            this.planetarySystem.UseVisualStyleBackColor = true;
            this.planetarySystem.Click += new System.EventHandler(this.PlanetarySystemClick);
            // 
            // distributionTest
            // 
            this.distributionTest.Location = new System.Drawing.Point(325, 57);
            this.distributionTest.Name = "distributionTest";
            this.distributionTest.Size = new System.Drawing.Size(100, 23);
            this.distributionTest.TabIndex = 8;
            this.distributionTest.Text = "Distribution Test";
            this.distributionTest.UseVisualStyleBackColor = true;
            this.distributionTest.Click += new System.EventHandler(this.DistributionTestClick);
            // 
            // changeG
            // 
            this.changeG.Location = new System.Drawing.Point(7, 28);
            this.changeG.Name = "changeG";
            this.changeG.Size = new System.Drawing.Size(100, 23);
            this.changeG.TabIndex = 9;
            this.changeG.Text = "Change G";
            this.changeG.UseVisualStyleBackColor = true;
            this.changeG.Click += new System.EventHandler(this.ChangeGClick);
            // 
            // changeN
            // 
            this.changeN.Location = new System.Drawing.Point(219, 28);
            this.changeN.Name = "changeN";
            this.changeN.Size = new System.Drawing.Size(100, 23);
            this.changeN.TabIndex = 10;
            this.changeN.Text = "Change N";
            this.changeN.UseVisualStyleBackColor = true;
            this.changeN.Click += new System.EventHandler(this.ChangeNClick);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.systemsTab);
            this.tabControl.Controls.Add(this.parametersTab);
            this.tabControl.Controls.Add(this.functionsTab);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(442, 114);
            this.tabControl.TabIndex = 11;
            // 
            // systemsTab
            // 
            this.systemsTab.Controls.Add(this.systemsLabel);
            this.systemsTab.Controls.Add(this.none);
            this.systemsTab.Controls.Add(this.slowParticles);
            this.systemsTab.Controls.Add(this.fastParticles);
            this.systemsTab.Controls.Add(this.distributionTest);
            this.systemsTab.Controls.Add(this.massiveBody);
            this.systemsTab.Controls.Add(this.planetarySystem);
            this.systemsTab.Controls.Add(this.orbitalSystem);
            this.systemsTab.Controls.Add(this.binarySystem);
            this.systemsTab.Location = new System.Drawing.Point(4, 22);
            this.systemsTab.Name = "systemsTab";
            this.systemsTab.Padding = new System.Windows.Forms.Padding(3);
            this.systemsTab.Size = new System.Drawing.Size(434, 88);
            this.systemsTab.TabIndex = 0;
            this.systemsTab.Text = "Systems";
            this.systemsTab.UseVisualStyleBackColor = true;
            // 
            // systemsLabel
            // 
            this.systemsLabel.AutoSize = true;
            this.systemsLabel.Location = new System.Drawing.Point(6, 7);
            this.systemsLabel.Name = "systemsLabel";
            this.systemsLabel.Size = new System.Drawing.Size(272, 13);
            this.systemsLabel.TabIndex = 9;
            this.systemsLabel.Text = "Various gravitational systems are available for simulation.";
            // 
            // parametersTab
            // 
            this.parametersTab.Controls.Add(this.changeC);
            this.parametersTab.Controls.Add(this.parametersLabel);
            this.parametersTab.Controls.Add(this.changeG);
            this.parametersTab.Controls.Add(this.changeN);
            this.parametersTab.Location = new System.Drawing.Point(4, 22);
            this.parametersTab.Name = "parametersTab";
            this.parametersTab.Padding = new System.Windows.Forms.Padding(3);
            this.parametersTab.Size = new System.Drawing.Size(434, 88);
            this.parametersTab.TabIndex = 1;
            this.parametersTab.Text = "Parameters";
            this.parametersTab.UseVisualStyleBackColor = true;
            // 
            // changeC
            // 
            this.changeC.Location = new System.Drawing.Point(113, 28);
            this.changeC.Name = "changeC";
            this.changeC.Size = new System.Drawing.Size(100, 23);
            this.changeC.TabIndex = 12;
            this.changeC.Text = "Change C";
            this.changeC.UseVisualStyleBackColor = true;
            this.changeC.Click += new System.EventHandler(this.ChangeCClick);
            // 
            // parametersLabel
            // 
            this.parametersLabel.AutoSize = true;
            this.parametersLabel.Location = new System.Drawing.Point(6, 7);
            this.parametersLabel.Name = "parametersLabel";
            this.parametersLabel.Size = new System.Drawing.Size(347, 13);
            this.parametersLabel.TabIndex = 11;
            this.parametersLabel.Text = "Modify these parameters to change the constants used in the simulation.";
            // 
            // functionsTab
            // 
            this.functionsTab.Controls.Add(this.showTracers);
            this.functionsTab.Controls.Add(this.showTree);
            this.functionsTab.Controls.Add(this.resetCamera);
            this.functionsTab.Controls.Add(this.label1);
            this.functionsTab.Controls.Add(this.pause);
            this.functionsTab.Location = new System.Drawing.Point(4, 22);
            this.functionsTab.Name = "functionsTab";
            this.functionsTab.Padding = new System.Windows.Forms.Padding(3);
            this.functionsTab.Size = new System.Drawing.Size(434, 88);
            this.functionsTab.TabIndex = 2;
            this.functionsTab.Text = "Functions";
            this.functionsTab.UseVisualStyleBackColor = true;
            // 
            // showTracers
            // 
            this.showTracers.Location = new System.Drawing.Point(325, 28);
            this.showTracers.Name = "showTracers";
            this.showTracers.Size = new System.Drawing.Size(100, 23);
            this.showTracers.TabIndex = 14;
            this.showTracers.Text = "Show Tracers";
            this.showTracers.UseVisualStyleBackColor = true;
            this.showTracers.Click += new System.EventHandler(this.ShowTracersClick);
            // 
            // showTree
            // 
            this.showTree.Location = new System.Drawing.Point(219, 28);
            this.showTree.Name = "showTree";
            this.showTree.Size = new System.Drawing.Size(100, 23);
            this.showTree.TabIndex = 13;
            this.showTree.Text = "Show Tree";
            this.showTree.UseVisualStyleBackColor = true;
            this.showTree.Click += new System.EventHandler(this.ShowTreeClick);
            // 
            // resetCamera
            // 
            this.resetCamera.Location = new System.Drawing.Point(113, 28);
            this.resetCamera.Name = "resetCamera";
            this.resetCamera.Size = new System.Drawing.Size(100, 23);
            this.resetCamera.TabIndex = 12;
            this.resetCamera.Text = "Reset Camera";
            this.resetCamera.UseVisualStyleBackColor = true;
            this.resetCamera.Click += new System.EventHandler(this.ResetCameraClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(278, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Various functions are available to enhance the simulation.";
            // 
            // pause
            // 
            this.pause.Location = new System.Drawing.Point(7, 28);
            this.pause.Name = "pause";
            this.pause.Size = new System.Drawing.Size(100, 23);
            this.pause.TabIndex = 9;
            this.pause.Text = "Pause";
            this.pause.UseVisualStyleBackColor = true;
            this.pause.Click += new System.EventHandler(this.PauseClick);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 137);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Settings";
            this.Text = "Settings";
            this.tabControl.ResumeLayout(false);
            this.systemsTab.ResumeLayout(false);
            this.systemsTab.PerformLayout();
            this.parametersTab.ResumeLayout(false);
            this.parametersTab.PerformLayout();
            this.functionsTab.ResumeLayout(false);
            this.functionsTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button none;
        private System.Windows.Forms.Button slowParticles;
        private System.Windows.Forms.Button fastParticles;
        private System.Windows.Forms.Button massiveBody;
        private System.Windows.Forms.Button orbitalSystem;
        private System.Windows.Forms.Button binarySystem;
        private System.Windows.Forms.Button planetarySystem;
        private System.Windows.Forms.Button distributionTest;
        private System.Windows.Forms.Button changeG;
        private System.Windows.Forms.Button changeN;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage systemsTab;
        private System.Windows.Forms.TabPage parametersTab;
        private System.Windows.Forms.Label systemsLabel;
        private System.Windows.Forms.Label parametersLabel;
        private System.Windows.Forms.Button changeC;
        private System.Windows.Forms.TabPage functionsTab;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button pause;
        private System.Windows.Forms.Button resetCamera;
        private System.Windows.Forms.Button showTree;
        private System.Windows.Forms.Button showTracers;
    }
}