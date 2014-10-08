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
        public SetTextToClipboard clipboardTextDelegate;

        public MainForm()
        {
            InitializeComponent();
            clipboardTextDelegate += new SetTextToClipboard(SetText);
        }

        public static void Main()
        {
            ConnectionHandler handler = new ConnectionHandler();
            handler.Listen();
            Application.Run(new MainForm());
        }

        private void SetText(string contentToPaste)
        {
            System.Windows.Forms.Clipboard.SetText(contentToPaste);
        }
    }
}
