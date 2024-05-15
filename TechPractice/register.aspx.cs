using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;


namespace TechPractice
{
    public partial class register : System.Web.UI.Page
    {
        private string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void registerUser(object sender, EventArgs e)
        {
            if (checkTxtBox() == false)
            {
                errorTxt.Text = "All fields must be filled";
            }

            else if (tb_password.Text != tb_confirmPass.Text)
            {
                errorTxt.Text = "The entered passwords do not match";
            }

            else
            {
                // Insert user data into PostgreSQL database
                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO usersdata (login, password) VALUES (@login, @password)";
                        cmd.Parameters.AddWithValue("@login", tb_login.Text);
                        cmd.Parameters.AddWithValue("@password", tb_password.Text);
                        cmd.ExecuteNonQuery();
                    }
                }

                errorTxt.Text = "Registration successful!";
                Session["RegisteredStatus"] = "true";
                Session["CurrentLogin"] = tb_login.Text;
                Response.Redirect("~/WebForm1.aspx");
            }
        }

        protected bool checkTxtBox()
        {
            TextBox[] textBox = { tb_login, tb_password, tb_confirmPass };

            foreach (TextBox txt in textBox)
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    return false;
                }
            }
            return true;
        }
        protected void ReturnToFirstPage(object sender, EventArgs e)
        {
            Response.Redirect("~/WebForm1.aspx");
        }
    }
}