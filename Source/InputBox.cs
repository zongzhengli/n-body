using System;
using System.Windows.Forms;

namespace NBody {

    /// <summary>
    /// This is the dialog box that appears to prompt for input. 
    /// </summary>
    partial class InputBox : Form {
        public InputBox() {
            InitializeComponent();
            Button b = new Button();
            b.Click += delegate { 
                Close();
            };
            CancelButton = b;
        }

        void buttonClick(Object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        public DialogResult ShowDialog(String s, String d) {
            promptLabel.Text = s;
            responseBox.Text = d;
            CenterToScreen();
            return ShowDialog();
        }

        public static String Show(String s, String d = "") {
            using (InputBox a = new InputBox()) {
                if (a.ShowDialog(s, d) == DialogResult.OK)
                    return a.responseBox.Text;
                return d;
            }
        }
    }
}