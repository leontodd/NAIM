﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data;

namespace NAIM
{
    class DatabaseInterface
    {
        private MySqlConnection connection;

        public DatabaseInterface(string credentialsPath)
        {
            string[] credentials = new string[0];
            try { credentials = File.ReadAllLines(credentialsPath); }
            catch (Exception e) { Console.WriteLine("DB credential error"); Console.ReadLine(); Environment.Exit(0); }

            string connectionString;
            connectionString = "SERVER=" + credentials[0] + ";" + "DATABASE=" +
            credentials[1] + ";" + "UID=" + credentials[2] + ";" + "PASSWORD=" + credentials[3] + ";";
            connection = new MySqlConnection(connectionString);
        }

        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: " + ex.Number + " - " + ex.Message);
                return false;
            }
        }

        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: " + ex.Number + " - " + ex.Message);
                return false;
            }
        }

        private DataTable ExecuteQuery(string query)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                DataTable data = new DataTable();
                data.Load(dataReader);
                dataReader.Close();
                this.CloseConnection();
                if (data.Rows.Count != 0) { return data; } else { return null; }
            }
            else { return null; }
        }

        public bool Authorise(string username, string password)
        {
            DataTable authCheck = ExecuteQuery("SELECT * FROM " + "users" + " WHERE " + "(" + "username" + "='" + username + "' AND password='" + password + "');");
            if (authCheck != null)
            {
                return true;
            }
            else { return false; }
        }

        public bool RegisterUser(string username, string password, string email)
        {
            DataTable usernameCheck = ExecuteQuery("SELECT * FROM " + "users" + " WHERE " + "username" + "='" + username + "';");
            if (usernameCheck == null)
            {
                ExecuteQuery("INSERT INTO users (username, password, email) VALUES ('" + username + "', '" + password + "', '" + email + "');");
                return true;
            }
            else { return false; }
        }

        public bool UnregisterUser(string username, string password)
        {
            if (Authorise(username, password) != false)
            {
                ExecuteQuery("DELETE FROM " + "users" + " WHERE " + "username" + "='" + username + "';");
                return true;
            }
            else { return false; }
        }

        public bool SendMessage(string username, string password, string content, string reciever)
        {
            if (Authorise(username, password) != false)
            {
                DataTable uid1Check = ExecuteQuery("SELECT * FROM " + "users" + " WHERE " + "(" + "username" + "='" + username + "');");
                if (uid1Check != null)
                {
                    string uid1 = uid1Check.Rows[0].Field<int>(0).ToString();
                    DataTable uid2Check = ExecuteQuery("SELECT * FROM " + "users" + " WHERE " + "(" + "username" + "='" + reciever + "');");
                    if (uid1Check != null)
                    {
                        string uid2 = uid2Check.Rows[0].Field<int>(0).ToString();
                        DataTable cidCheck = ExecuteQuery("SELECT * FROM " + "conversations" + " WHERE " + "(u_one = '" + uid1 + "' AND u_two='" + uid2 + "') OR  (u_one = '" + uid2 + "' AND u_two='" + uid1 + "');");
                        while(cidCheck == null)
                        {
                            ExecuteQuery("INSERT INTO conversations (u_one, u_two) VALUES ('" + uid1 + "', '" + uid2 + "');");
                            cidCheck = ExecuteQuery("SELECT * FROM " + "conversations" + " WHERE " + "(u_one = '" + uid1 + "' AND u_two='" + uid2 + "') OR  (u_one = '" + uid2 + "' AND u_two='" + uid1 + "');");
                        }
                        string cid = cidCheck.Rows[0].Field<int>(0).ToString();
                        ExecuteQuery("INSERT INTO messages (content, c_id, u_id) VALUES ('" + content + "', '" + cid + "', '" + uid1 + "');");
                        return true;
                    }
                    else { return false; }
                }
                else { return false; }
            }
            else { return false; }
        }
    }
}