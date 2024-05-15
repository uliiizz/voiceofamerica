using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System.Net.Http;
using System.Reflection.Emit;
using System.Xml.Linq;
using Facebook;
using Npgsql;



namespace TechPractice
{
    public partial class signIn : System.Web.UI.Page
    {
        private string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";

        private string clientId = "1032225343859-j9cl52p82esglk4295c3f6218p7fnd94.apps.googleusercontent.com";
        private string clientSecret = "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";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string registrationSource = Session["RegistrationSource"] as string;
                if (!string.IsNullOrEmpty(registrationSource))
                {
                    if (registrationSource == "facebook")
                    {
                        FacebookAuthorizationCode(Request.QueryString["code"]);
                    }
                    else if (registrationSource == "google")
                    {
                        GoogleAuthorizationCode(Request.QueryString["code"]);
                    }

                    Session.Remove("RegistrationSource");
                }
                else
                {
                    // Handle other cases
                }
            }
        }

        protected void b_FacebookClick(object sender, EventArgs e)
        {
            string appId = "269972492849779";
            string redirectUri = "https://localhost:44323/signIn.aspx";
            Session["RegistrationSource"] = "facebook";
            Response.Redirect(string.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}", appId, redirectUri));
        }

        protected void b_googleClick(object sender, EventArgs e)
        {
            GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { "email", "profile" },
                DataStore = new FileDataStore("PeopleService.Auth.Store")
            });

            string authUrl = flow.CreateAuthorizationCodeRequest("https://localhost:44323/signIn.aspx").Build().ToString();
            Session["RegistrationSource"] = "google";
            Response.Redirect(authUrl);

        }

        private void FacebookAuthorizationCode(string accessCode)
        {
            try
            {
                string appId = "269972492849779";
                string appSecret = "2681812bf09c0e49aa10644f6eddcf95";
                string redirectUri = "https://localhost:44323/signIn.aspx";

                var fb = new FacebookClient();
                dynamic result = fb.Get("oauth/access_token", new
                {
                    client_id = appId,
                    client_secret = appSecret,
                    redirect_uri = redirectUri,
                    code = accessCode
                });

                string accessToken = result.access_token;

                fb.AccessToken = accessToken;
                dynamic userInfo = fb.Get("me?fields=id,name,email");

                string userId = userInfo.id;
                string userName = userInfo.name;
                string email = userInfo.email;

          

                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO usersdata (login, email) VALUES (@login, @email)";
                        cmd.Parameters.AddWithValue("@login", userName);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.ExecuteNonQuery();
                    }
                }
                Session["RegisteredStatus"] = "true";
                Session["CurrentLogin"] = userName;
                Response.Redirect("~/WebForm1.aspx");
            }
            catch (Exception ex)
            {
                Response.Write($"An error occurred: {ex.Message}");
            }
        }

        private void GoogleAuthorizationCode(string code)
        {
            try
            {
                GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    Scopes = new[] { "email", "profile" },
                    DataStore = new FileDataStore("PeopleService.Auth.Store")
                });

                TokenResponse token = flow.ExchangeCodeForTokenAsync(clientId, code, "https://localhost:44323/signIn.aspx", CancellationToken.None).Result;

                PeopleServiceService service = new PeopleServiceService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = GoogleCredential.FromAccessToken(token.AccessToken)
                });

                PeopleResource.GetRequest getRequest = service.People.Get("people/me");

                getRequest.PersonFields = "names,emailAddresses";

                Person me = getRequest.Execute();

                string email = me.EmailAddresses[0].Value;
                string name = me.Names[0].DisplayName;

                string[] emailParts = email.Split('@');
                string username = emailParts[0];

                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO usersdata (login, email) VALUES (@login, @email)";
                        cmd.Parameters.AddWithValue("@login", username);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.ExecuteNonQuery();
                    }
                }
                Session["RegisteredStatus"] = "true";
                Session["CurrentLogin"] = username;
                Response.Redirect("~/WebForm1.aspx");
            }
            catch (Exception ex)
            {
                Response.Write($"An error occurred: {ex.Message}");
            }


        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string name = tb_login.Text.Trim();
            string password = tb_password.Text.Trim();

            if (IsValidUser(name, password))
            {
                Session["RegisteredStatus"] = "true";
                Session["CurrentLogin"] = name;
                Response.Redirect("~/WebForm1.aspx");
            }

            else
            {
                errorTxt.Text = "Incorrect login or password";
            }
        }

        protected bool IsValidUser(string username, string password)
        {
 
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT COUNT(*) FROM usersdata WHERE login = @login AND password = @password";
                    cmd.Parameters.AddWithValue("@login", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    // If count is greater than 0, the user is valid
                    return count > 0;
                }
            }

        }

        protected void ReturnToFirstPage(object sender, EventArgs e)
        {
            Response.Redirect("~/WebForm1.aspx");
        }
    }
}