using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;

namespace TechPractice
{
    public partial class CreateEventView : System.Web.UI.Page
    {
        string minDateTime;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session["SelectedLocation"] = "";              
                // Отримуємо та заповнюємо випадаючий список країн
                FetchAndPopulateCountries();
                // Встановлюємо мінімальну дату та час
                minDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
                string script = string.Format("document.getElementById('{0}').min = '{1}';", TextBoxDateTime.ClientID, minDateTime);
                ScriptManager.RegisterStartupScript(this, GetType(), "SetMinDateTime", script, true);
            }
            else
            {
                if (Session["SelectedLocation"] as string != null)
                {
                    string location = Session["SelectedLocation"] as string;
                    if (TimeZoneDictionary.TimeZones.ContainsKey(location) && TimeZoneDictionary.TimeZones[location].Item2 == "UTCVariety")
                    {
                        AddTimeZoneDropdown(location);
                    }
                }
            }
        }

        // Отримуємо дані країн з зовнішнього джерела та заповнюємо випадаючий список
        private void FetchAndPopulateCountries()
        {
            try
            {
                string content = FetchCountries();
                if (!string.IsNullOrEmpty(content))
                {
                    Regex regex = new Regex("\"(\\w+)\":\\s*\\{\"country\":\\s*\"([\\w\\s]+)\"");
                    MatchCollection matches = regex.Matches(content);
                    foreach (Match match in matches)
                    {
                        string shortForm = match.Groups[1].Value;
                        string country = match.Groups[2].Value;
                        ddlLocation.Items.Add(new ListItem(country, shortForm));
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Отримуємо дані країн з зовнішнього URL
        private string FetchCountries()
        {
            using (HttpClient client = new HttpClient())
            {
                string url = "https://cdn.amcharts.com/lib/5/geodata/data/countries2.js";
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    Debug.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
                    return null;
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string username = Session["CurrentLogin"] as string;
            if (username != string.Empty)
            {
            // Отримання даних з полів вводу
                string eventName = txtName.Text;
                int ttl = int.Parse(txtTTL.Text);
                string location = ddlLocation.SelectedItem.Value;
                string clarificationTimeZone = "";

                // Якщо місце потребує конкретного часового поясу, отримуємо його.
                if (TimeZoneDictionary.TimeZones.ContainsKey(location) && TimeZoneDictionary.TimeZones[location].Item2 == "UTCVariety")
                {
                    DropDownList ddlTimezone = (DropDownList)pnlSelectedLocations.FindControl("ddlTimezone");
                    if (ddlTimezone != null && !string.IsNullOrEmpty(ddlTimezone.SelectedValue))
                    {
                        clarificationTimeZone = ddlTimezone.SelectedValue;
                    }
                }
                else
                {
                    // Використовуємо стандартний часовий пояс сервера
                    clarificationTimeZone = TimeZoneDictionary.TimeZones[location].Item2;
                }

                // Формуємо рядок даних події.
                DateTime eventDateTime = DateTime.Parse(TextBoxDateTime.Text);
                Guid eventId = Guid.NewGuid();
                string publicLink = $"https://localhost:44323/DetailedView.aspx/?id={eventId}";

                // Вставка даних у базу даних PostgreSQL
                InsertEventDataToPostgreSQL(eventId, eventName, ttl, location, publicLink, username, eventDateTime, clarificationTimeZone);
            }
            else 
            {
                
            }
        }

        // Метод для вставки даних у базу даних 
        private void InsertEventDataToPostgreSQL(Guid eventId, string eventName, int ttl, string location, string publicLink, string ownerId, DateTime eventDateTime, string clarificationTimeZone)
        {
            string connString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string sql = "INSERT INTO eventdata (id, eventname, ttl, eventlocation, publicLink, ownerid, eventdatetime, clarificationtimezone) " +
                                 "VALUES (@eventId, @eventName, @ttl, @location, @publicLink, @ownerId, @eventDateTime, @clarificationTimeZone)";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("eventId", eventId);
                        cmd.Parameters.AddWithValue("eventName", eventName);
                        cmd.Parameters.AddWithValue("ttl", ttl);
                        cmd.Parameters.AddWithValue("location", location);
                        cmd.Parameters.AddWithValue("publicLink", publicLink);
                        cmd.Parameters.AddWithValue("ownerId", ownerId);
                        cmd.Parameters.AddWithValue("eventDateTime", eventDateTime);
                        cmd.Parameters.AddWithValue("clarificationTimeZone", clarificationTimeZone);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при вставці даних у базу даних: {ex.Message}");
            }
        }

        // Обробляємо подію зміни тексту для поля TTL
        protected void txtTTL_TextChanged(object sender, EventArgs e)
        {
            // Валідуємо введений TTL за допомогою регулярного виразу
            string input = txtTTL.Text.Trim();
            string pattern = @"^\d+$";
            if (!Regex.IsMatch(input, pattern))
            {
                txtTTL.Text = string.Empty;
                txtTTL.BorderColor = Color.Red;
            }
            else
                txtTTL.BorderColor = Color.White;
        }

        // Обробляємо подію зміни вибору елемента випадаючого списку місць.
        protected void ddlLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            string location = ddlLocation.SelectedItem.Value;
            Session["SelectedLocation"] = location;
            if (TimeZoneDictionary.TimeZones.ContainsKey(location) && TimeZoneDictionary.TimeZones[location].Item2 == "UTCVariety")
            {
                AddTimeZoneDropdown(location);
            }
            else
            {
                RemoveTimeZoneDropdown();
            }
        }

        // Додаємо випадаючий список для вибору часового поясу на основі місця.
        private void AddTimeZoneDropdown(string location)
        {
            DropDownList ddlTimezone = new DropDownList();
            ddlTimezone.ID = "ddlTimezone";
            if (TimeZoneDictionary.TimeZones.ContainsKey(location))
            {
                // Заповнюємо випадаючий список UTC 
                List<string> utcOffsets = new List<string>();
                switch (location)
                {
                    case "RU": // Russia
                        utcOffsets.AddRange(new[] { "+02:00", "+03:00", "+04:00", "+05:00", "+06:00", "+07:00", "+08:00", "+09:00", "+10:00", "+11:00", "+12:00" });
                        break;
                    case "US": // USA
                        utcOffsets.AddRange(new[] { "-10:00", "-09:00", "-08:00", "-07:00", "-06:00", "-05:00", "-04:00" });
                        break;
                    case "MN": // Mongolia
                        utcOffsets.AddRange(new[] { "+07:00", "+08:00" });
                        break;
                    case "MX": // Mexico
                        utcOffsets.AddRange(new[] { "-05:00", "-06:00", "-07:00" });
                        break;
                    case "KZ": // Kazakhstan
                        utcOffsets.AddRange(new[] { "+05:00", "+06:00" });
                        break;
                    case "ID": // Indonesia
                        utcOffsets.AddRange(new[] { "+07:00", "+08:00", "+09:00" });
                        break;
                    case "BR": // Brazil
                        utcOffsets.AddRange(new[] { "-05:00", "-04:00", "-03:00", "-02:00" });
                        break;
                    case "AU": // Australia
                        utcOffsets.AddRange(new[] { "+10:30", "+10:00", "+09:30", "+08:45", "+08:00" });
                        break;
                    default:
                        for (int hour = -12; hour <= 12; hour++)
                        {
                            for (int minute = 0; minute <= 30; minute += 30)
                            {
                                string offset = $"{(hour >= 0 ? "+" : "-")}{Math.Abs(hour).ToString("00")}:{minute.ToString("00")}";
                                utcOffsets.Add(offset);
                            }
                        }
                        break;
                }

                RemoveTimeZoneDropdown();
                foreach (var offset in utcOffsets)
                {
                    ddlTimezone.Items.Add(new ListItem($"UTC{offset}", $"UTC{offset}"));
                }
                pnlSelectedLocations.Controls.Add(ddlTimezone);
            }
            else
            {
                RemoveTimeZoneDropdown();
            }
        }

        // Прибираємо випадаючий список часових поясів.
        private void RemoveTimeZoneDropdown()
        {
            DropDownList ddlTimezone = (DropDownList)pnlSelectedLocations.FindControl("ddlTimezone");
            if (ddlTimezone != null)
            {
                pnlSelectedLocations.Controls.Remove(ddlTimezone);
            }
        }

        // Обробляємо подію зміни тексту для поля вводу дати та часу
        protected void TextBoxDateTime_TextChanged(object sender, EventArgs e)
        {
            DateTime minDateTime = DateTime.Now;
            DateTime enteredDateTime;
            if (DateTime.TryParse(TextBoxDateTime.Text, out enteredDateTime))
            {
                if (enteredDateTime < minDateTime)
                {
                    DateTime now = DateTime.Now;
                    DateTime nextHour = now.AddHours(1);
                    TextBoxDateTime.Text = nextHour.ToString("yyyy-MM-ddTHH:mm");
                }
            }
        }
    }
}

