﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using NAIM_API;

namespace NAIM_client_test
{
    public partial class Form1 : Form
    {
        public static ClientInterface client = new ClientInterface(new IPEndPoint(IPAddress.Parse("192.168.1.137"), 1997));
        public static string authenticatedUser;
        public static string authenticatedPassword;

        private BackgroundWorker bwLogin = new BackgroundWorker();
        private BackgroundWorker bwRegister = new BackgroundWorker();
        public Form1()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            bwLogin.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bwLogin_Complete);
            bwRegister.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bwRegister_Complete);

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bwLogin.RunWorkerAsync();
        }

        private void bwLogin_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                client.Authorise(username.Text, password.Text);
                authenticatedUser = username.Text;
                authenticatedPassword = password.Text;
                MessageBox.Show("Authorised.");
                Form2 form = new Form2();
                form.Show();
                this.Hide();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bwRegister.RunWorkerAsync();
        }

        private void bwRegister_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                client.Register(usernameR.Text, passwordR.Text, emailR.Text);
                MessageBox.Show("Registered, please log in.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            client.Close();
        }
    }
}
