using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAIM_API;

namespace NAIM_client_test
{
    public partial class Form2 : Form
    {
        private BackgroundWorker bwCheck = new BackgroundWorker();
        private BackgroundWorker bwSend = new BackgroundWorker();

        public static List<Conversation> convoList = new List<Conversation>();
        public Form2()
        {
            InitializeComponent();
            bwCheck.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bwCheck_Complete);
            bwSend.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bwSend_Complete);

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            bwCheck.RunWorkerAsync();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBox1.Text))
            {
                button2.Enabled = false;
            }
            else
            {
                button2.Enabled = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshSelectedConvo();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bwSend.RunWorkerAsync();
        }

        private void bwSend_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                Form1.client.SendMessage(Form1.authenticatedUser, Form1.authenticatedPassword, textBox1.Text, listBox1.SelectedItem.ToString());
                bwCheck.RunWorkerAsync();
                listBox1.SelectedIndex = 0;
                RefreshSelectedConvo();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void bwCheck_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            listBox1.Items.Clear();
            try
            {
                convoList = Form1.client.CheckMessages(Form1.authenticatedUser, Form1.authenticatedPassword);
                foreach(Conversation c in convoList)
                {
                    if (c.user1 != Form1.authenticatedUser)
                    {
                        listBox1.Items.Add(c.user1);
                    }
                    else
                    {
                        listBox1.Items.Add(c.user2);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void RefreshSelectedConvo()
        {
            listBox2.Items.Clear();
            if (listBox1.SelectedItem != null && (string)listBox1.SelectedItem != "")
            {
                Conversation selectedConvo = convoList[listBox1.SelectedIndex];
                foreach (NAIM_API.Message m in selectedConvo.messages)
                {
                    listBox2.Items.Add("[" + m.time + "] " + m.user + "; " + m.content);
                }
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit(e);
        }
    }
}
