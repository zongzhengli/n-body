using System;
using System.Windows.Forms;

namespace NBody {

    /// <summary>
    /// A dialog box that prompts for text input. 
    /// </summary>
    partial class InputBox : Form {

        /// <summary>
        /// Initializes the InputBox. 
        /// </summary>
        public InputBox() {
            InitializeComponent();
            Button button = new Button();
            button.Click += (sender, e) => {
                Close();
            };
            CancelButton = button;
        }

        /// <summary>
        /// Invoked upon release of the OK button. This closes the dialog box. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event.</param>
        void ButtonClick(Object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Displays the InputBox with the given message and default text in the 
        /// input field. 
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="defaultInputText">The default text in the the input field.</param>
        /// <returns>The exit flag of the InputBox.</returns>
        public DialogResult ShowDialog(String message, String defaultInputText) {
            promptLabel.Text = message;
            responseBox.Text = defaultInputText;
            CenterToScreen();
            return ShowDialog();
        }

        /// <summary>
        /// Initializes and displays a new InputBox witih the given message and 
        /// default text in the input field. 
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="defaultInputText">The default text in the the input field.</param>
        /// <returns>The value in the input field if the OK button is pressed, otherwise the default input text.</returns>
        public static String Show(String message, String defaultInputText = "") {
            using (InputBox a = new InputBox()) {
                if (a.ShowDialog(message, defaultInputText) == DialogResult.OK)
                    return a.responseBox.Text;
                return defaultInputText;
            }
        }
    }
}