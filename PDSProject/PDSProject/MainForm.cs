using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CommunicationLibrary;

namespace PDSProject
{
    public partial class MainForm : Form
    {

        public delegate void SetTextToClipboard(string stringData);
        public static SetTextToClipboard clipboardTextDelegate;
        public static MainForm mainForm;

        public MainForm()
        {
            InitializeComponent();
            clipboardTextDelegate += new SetTextToClipboard(SetText);
        }

        [STAThread]
        public static void Main()
        {
            ConnectionHandler handler = new ConnectionHandler();
            handler.Listen();
            mainForm = new MainForm();
            Application.Run(mainForm);
        }

        private void SetText(string contentToPaste)
        {
            System.Windows.Forms.Clipboard.SetText(contentToPaste);
        }
    }
}
