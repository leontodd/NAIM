using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using NAIM_API;

namespace NAIM_client_test
{
    public partial class Form1 : Form
    {
        public ClientInterface client = new ClientInterface(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1997));
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
                client.Authorise(username.Text, password.Text);
                MessageBox.Show("Authorised.");
            //}
            //catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
