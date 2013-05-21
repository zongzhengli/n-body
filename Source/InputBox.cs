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

        public DialogResult ShowDialog(String message, String defaultInputText) {
            promptLabel.Text = message;
            responseBox.Text = defaultInputText;
            CenterToScreen();
            return ShowDialog();
        }

        public static String Show(String message, String defaultInputText = "") {
            using (InputBox a = new InputBox()) {
                if (a.ShowDialog(message, defaultInputText) == DialogResult.OK)
                    return a.responseBox.Text;
                return defaultInputText;
            }
        }
    }
}