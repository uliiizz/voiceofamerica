using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Npgsql;

namespace TechPractice
{
    public partial class DetailedView : System.Web.UI.Page
    {
        private string eventId;
        private string[] lines;
        private string GMTEventTime;
        private List<string> relatedLocationsList = new List<string>();

        // Нужно будет брать это из БД с привязкой к юзерам и их персональным настройкам
        public static string usersCountrysTimeZone = TimeZoneDictionary.TimeZones["UA"].Item2;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                eventId = Request.QueryString["id"];
                if (string.IsNullOrEmpty(eventId))
                {
                    Debug.WriteLine("Event ID is null or empty.");
                    Response.Redirect("~/ErrorPage.aspx");
                    return;
                }
                FetchAndPopulateCountries();
                LoadEventData();
            }
            UpdateRelatedLocationsPanel();
        }

        // Завантаження даних події
        private void LoadEventData()
        {
            eventId = Request.QueryString["id"];
            Session["eventId"] = eventId;

            string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";
            string sql = "SELECT * FROM eventdata WHERE id = @eventId";

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("eventId", eventId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtName.Text = reader["eventname"].ToString();
                            txtTTL.Text = reader["ttl"].ToString();
                            ddlTimeZones.SelectedValue = reader["eventlocation"].ToString(); // Предполагая, что значение в столбце eventlocation соответствует значению в ddlTimeZones
                            txtDateTime.Value = ((DateTime)reader["eventdatetime"]).ToString("yyyy-MM-ddTHH:mm");
                            LinkLabel.Text = reader["publiclink"].ToString();



                            string clarificationTimeZone = reader["clarificationtimezone"].ToString();
                            if (!string.IsNullOrEmpty(clarificationTimeZone))
                            {
                                if (clarificationTimeZone != "UTCVariety")
                                {
                                    GMTEventTime = AddOrSubtractHours(txtDateTime.Value, clarificationTimeZone, true);
                                }
                                else
                                {
                                    GMTEventTime = AddOrSubtractHours(txtDateTime.Value, reader["clarificationtimezone"].ToString(), true);
                                    ViewState["TimeZoneClarification"] = reader["clarificationtimezone"].ToString();
                                }
                            }

                            Session["GMTEventTime"] = GMTEventTime;
                            UsersLocalEventTimeLabel.Text = $"Event Time by users local time: {AddOrSubtractHours(Session["GMTEventTime"] as string, usersCountrysTimeZone, false)}";

                            string relatedLocations = reader["relatedlocations"].ToString();
                            if (!string.IsNullOrEmpty(relatedLocations))
                            {
                                var relatedLocationsList = relatedLocations.Split(',').Select(loc => loc.Trim()).ToList();
                                Session["RelatedLocationsList"] = relatedLocationsList;
                            }
                            UpdateRelatedLocationsPanel();
                        }
                        else
                        {
                            Debug.WriteLine("No data found for the given event ID.");
                        }
                    }
                }
            }
        }

        // Метод для отримання даних про країни із зовнішнього джерела
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

        // Метод для заповнення списку країн на веб-сторінці
        private void FetchAndPopulateCountries()
        {
            try
            {
                Debug.WriteLine("Fetching countries synchronously...");
                string content = FetchCountries();
                if (!string.IsNullOrEmpty(content))
                {
                    Regex regex = new Regex("\"(\\w+)\":\\s*\\{\"country\":\\s*\"([\\w\\s]+)\"");
                    MatchCollection matches = regex.Matches(content);
                    foreach (Match match in matches)
                    {
                        string shortForm = match.Groups[1].Value;
                        string country = match.Groups[2].Value;
                        Debug.WriteLine($"Short Form: {shortForm}, Country: {country}");
                        ddlTimeZones.Items.Add(new ListItem(country, shortForm));
                        ddlRelatedLocations.Items.Add(new ListItem(country, shortForm));
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to fetch countries data.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Метод для додавання або віднімання годин до зазначеної дати 
        public static string AddOrSubtractHours(string dateTimeString, string utcOffset, bool invertSign)
        {
            string[] eventData = HttpContext.Current.Session["TimeZoneClarification"] as string[];

            if (utcOffset == "UTCVariety" && eventData != null && eventData.Length > 7)
            {
                utcOffset = eventData[7];
            }
            DateTime dateTime = DateTime.Parse(dateTimeString);
            char sign = utcOffset[3];
            int hours = int.Parse(utcOffset.Substring(4, 2));
            int minutes = 0;
            if (utcOffset.Length > 3)
            {
                minutes = int.Parse(utcOffset.Substring(7, 2));
            }
            int totalOffsetMinutes = (hours * 60 + minutes) * (sign == '-' ? -1 : 1);
            if (invertSign)
            {
                totalOffsetMinutes *= -1;
            }

            TimeSpan offsetTimeSpan = TimeSpan.FromMinutes(totalOffsetMinutes);
            DateTime resultDateTime = dateTime.Add(offsetTimeSpan);

            return resultDateTime.ToString("yyyy-MM-ddTHH:mm");
        }

        // Оновлення панелі Related locations на веб-сторінці
        private void UpdateRelatedLocationsPanel()
        {
            RelatedLocationsButtonsPanel.Controls.Clear();

            if (Session["RelatedLocationsList"] != null)
            {
                relatedLocationsList = (List<string>)Session["RelatedLocationsList"];
                foreach (var relatedLocation in relatedLocationsList)
                {
                    var cardDiv = new HtmlGenericControl("div");
                    cardDiv.Attributes["class"] = "card";

                    var CountryParagraph = new HtmlGenericControl("p");
                    CountryParagraph.Attributes["class"] = "person_name";
                    CountryParagraph.InnerText = $"Country: {TimeZoneDictionary.TimeZones[relatedLocation].Item1}";

                    var TimeZoneParagraph = new HtmlGenericControl("p");
                    TimeZoneParagraph.Attributes["class"] = "person_name";
                    TimeZoneParagraph.InnerText = $"Time Zone: {TimeZoneDictionary.TimeZones[relatedLocation].Item2}";

                    var personDesgParagraph = new HtmlGenericControl("p");
                    personDesgParagraph.Attributes["class"] = "person_name";
                    Debug.WriteLine(Session["GMTEventTime"] as string);
                    personDesgParagraph.InnerText = $"Event Time by local time: {AddOrSubtractHours(Session["GMTEventTime"] as string, TimeZoneDictionary.TimeZones[relatedLocation].Item2, false)}";

                    var deleteButton = new Button();
                    deleteButton.CssClass = "delete_btn";
                    deleteButton.Text = "Delete";
                    deleteButton.Click += (sender, e) => {
                        RelatedLocationsButtonsPanel.Controls.Remove(cardDiv);
                        relatedLocationsList.Remove(relatedLocation);
                        Session["RelatedLocationsList"] = relatedLocationsList;
                        UpdateDBRelatedLocations();
                    };

                    cardDiv.Controls.Add(CountryParagraph);
                    cardDiv.Controls.Add(TimeZoneParagraph);
                    cardDiv.Controls.Add(personDesgParagraph);
                    cardDiv.Controls.Add(deleteButton);
                    RelatedLocationsButtonsPanel.Controls.Add(cardDiv);
                }
                UpdateDBRelatedLocations();
            }
        }

        // Оновлення даних про Related locations у бд
        private void UpdateDBRelatedLocations()
        {
            string updatedLine = string.Join(",", relatedLocationsList);
            string eventId = Session["eventId"] as string;

            if (eventId == null)
            {
                Debug.WriteLine("Event ID is null in the session.");
                return;
            }

            string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "UPDATE eventdata SET relatedlocations = @relatedlocations WHERE id = @eventId";
                    cmd.Parameters.AddWithValue("relatedlocations", updatedLine);
                    cmd.Parameters.AddWithValue("eventId", eventId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Обробник кнопки "Зберегти"
        protected void btnSave_Click(object sender, EventArgs e)
        {
            ToggleEditability(false);
            string newTimeZones = ddlTimeZones.SelectedValue;
            string eventId = Request.QueryString["id"];
            string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica;SSL Mode=Require";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                { 
                    connection.Open();
                    string updateQuery = "UPDATE eventdata SET eventlocation = @newTimeZones WHERE id = @eventId";
                    using (var cmd = new NpgsqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("newTimeZones", newTimeZones);
                        cmd.Parameters.AddWithValue("eventId", eventId);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

        // Обробник кнопки "Редагувати"
        protected void btnToggleEdit_Click(object sender, EventArgs e)
        {
            ToggleEditability(true);
        }

        // Метод для перемикання можливості редагування даних на сторінці
        private void ToggleEditability(bool enableEditing)
        {
            PanelEventDetails.Enabled = !PanelEventDetails.Enabled;
            btnSave.Visible = enableEditing;
            btnToggleEdit.Visible = !enableEditing;
            txtDateTime.Disabled = !enableEditing;
        }

        // Обробник зміни вибраного розташування
        protected void ddlRelatedLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Session["RelatedLocationsList"] == null)
            {
                relatedLocationsList = new List<string>();
            }
            else
            {
                relatedLocationsList = (List<string>)Session["RelatedLocationsList"];
            }

            relatedLocationsList.Add(ddlRelatedLocations.SelectedValue);
            Session["RelatedLocationsList"] = relatedLocationsList;
            UpdateRelatedLocationsPanel();
        }

        // Метод для додавання випадаючуго списку часового поясу
        private void AddTimeZoneClarificationDropDown(string location)
        {
            DropDownList ddlTimeZoneClarification = new DropDownList();
            ddlTimeZoneClarification.ID = "ddlTimeZoneClarification_" + location;
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
            foreach (var timeZone in utcOffsets)
            {
                ddlTimeZoneClarification.Items.Add(new ListItem(timeZone, timeZone));
            }
            pnlSelectedLocations.Controls.Add(ddlTimeZoneClarification);
        }

        // Обробник зміни вибраного часового поясу
        protected void ddlTimeZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            string location = ddlTimeZones.SelectedItem.Value;
            Session["SelectedLocation"] = location;
            if (TimeZoneDictionary.TimeZones.ContainsKey(location) && TimeZoneDictionary.TimeZones[location].Item2 == "UTCVariety")
            {
                AddTimeZoneClarificationDropDown(location);
            }
            else
            {
                RemoveTimeZoneDropdown();
            }
            UpdatePanel1.Update();
        }

        // Метод для прибирання випадаючого списку
        private void RemoveTimeZoneDropdown()
        {
            DropDownList ddlTimezone = (DropDownList)pnlSelectedLocations.FindControl("ddlTimezone");
            if (ddlTimezone != null)
            {
                pnlSelectedLocations.Controls.Remove(ddlTimezone);
            }
        }

        // Обробник кнопку видалення Related location
        protected void RelLocButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string locationName = btn.Text.Trim();

            Debug.WriteLine($"Clicked button name: {locationName}");

            if (Session["RelatedLocationsList"] != null)
            {
                relatedLocationsList = (List<string>)Session["RelatedLocationsList"];
                relatedLocationsList.Remove(locationName);
                Session["RelatedLocationsList"] = relatedLocationsList;

                Debug.WriteLine("Updated list of selected related regions:");
                foreach (var region in relatedLocationsList)
                {
                    Debug.WriteLine(region);
                }
                UpdateRelatedLocationsPanel();
            }
            UpdateRelatedLocationsPanel();
        }
    }
}
