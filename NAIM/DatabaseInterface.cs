using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using System.Data;
using System.Web.Script.Serialization;

namespace NAIM
{
    class DatabaseInterface
    {
        private MySqlConnection connection;

        public DatabaseInterface(string credentialsPath)
        {
            string[] credentials = new string[0];
            try { credentials = File.ReadAllLines(credentialsPath); }
            catch (Exception ex) { Console.WriteLine("DB credential error"); Console.ReadLine(); Environment.Exit(0); }

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
            else { throw new NAIMException("Unable to connect to central database."); }
        }

        public bool Authorise(string username, string password)
        {
            DataTable authCheck = ExecuteQuery("SELECT * FROM " + "users" + " WHERE " + "(" + "username" + "='" + username + "' AND password='" + password + "');");
            if (authCheck != null)
            {
                return true;
            }
            else { throw new NAIMException("Unable to authorise user."); }
        }

        public bool RegisterUser(string username, string password, string email)
        {
            DataTable usernameCheck = ExecuteQuery("SELECT * FROM " + "users" + " WHERE " + "username" + "='" + username + "';");
            if (usernameCheck == null)
            {
                ExecuteQuery("INSERT INTO users (username, password, email) VALUES ('" + username + "', '" + password + "', '" + email + "');");
                return true;
            }
            else { throw new NAIMException("Registration failed, username already exists."); }
        }

        public bool UnregisterUser(string username, string password)
        {
            if (Authorise(username, password) != false)
            {
                ExecuteQuery("DELETE FROM " + "users" + " WHERE " + "username" + "='" + username + "';");
                return true;
            }
            else { throw new NAIMException("Unregistration failed, unable to authorise user."); }
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
                    if (uid2Check != null)
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
                    else { throw new NAIMException("Sending failed, unable to verify recipient."); }
                }
                else { throw new NAIMException("Sending failed, unable to verify user."); }
            }
            else { throw new NAIMException("Sending failed, unable to authorise user."); }
        }

        public string CheckMessages(string username, string password)
        {
            if (Authorise(username, password) != false)
            {
                DataTable uidCheck = ExecuteQuery("SELECT * FROM " + "users" + " WHERE " + "(" + "username" + "='" + username + "');");
                if (uidCheck != null)
                {
                    string uid = uidCheck.Rows[0].Field<int>(0).ToString();
                    DataTable conversationCheck = ExecuteQuery("SELECT * FROM " + "conversations" + " WHERE " + "(u_one = '" + uid + "' OR u_two='" + uid + "');");
                    if (conversationCheck != null)
                    {
                        List<Conversation> conCollection = new List<Conversation>();
                        string query = "(c_id = '" + conversationCheck.Rows[0][0] + "'";
                        foreach (DataRow d in conversationCheck.Rows)
                        {
                            conCollection.Add(new Conversation(d[0].ToString(), d[1].ToString(), d[2].ToString()));
                            query += " OR c_id='" + d[0] + "'";
                        }
                        query += ")";

                        DataTable messageCheck = ExecuteQuery("SELECT * FROM " + "messages" + " WHERE " + query + ";");
                        foreach (Conversation c in conCollection)
                        {
                            DataRow[] result = messageCheck.Select("c_id = '" + c.cid + "'");
                            foreach (DataRow d in result)
                            {
                                c.messages.Add(new Message(d[1].ToString(), d[3].ToString(), d[4].ToString()));
                            }
                        }
                        string json = new JavaScriptSerializer().Serialize(conCollection);
                        return json;
                    }
                    else { return null; }
                }
                else { throw new NAIMException("Syncing failed, unable to verify user."); }
            }
            else { throw new NAIMException("Syncing failed, unable to authorise user."); }
        }
    }
}
