using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Npgsql;
using System.Timers;
using System.Xml.Linq;

namespace TechPractice
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private Dictionary<string, EventData> eventDataDict = new Dictionary<string, EventData>();
        private static Timer aTimer;
        static Random rnd = new Random();
        protected void Page_Load(object sender, EventArgs e)
        {
            //розкоментите для того что бы включить таймер, 1 ТТЛ = 1 минута
            //aTimer = new Timer();
            //aTimer.Interval = 60000;
            //aTimer.Elapsed += OnTimedEvent;
            //aTimer.AutoReset = true;
            //aTimer.Enabled = true;
            if (Session["RegisteredStatus"] as string == "true")
            {
                btnCreateEvent.Visible = true;
                Button3.Visible = true;
                Button1.Visible = true;
                registerPrompt.Visible = false;
                displayEventCount();
            }
            else
            {
                btnCreateEvent.Visible = false;
                Button3.Visible = false;
                Button1.Visible = false;
                registerPrompt.Visible = true;
            }
        }

        public class EventData
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int TTL { get; set; }
            public List<string> Locations { get; set; }
            public string PublicLink { get; set; }
            public string OwnerID { get; set; }
        }
        protected void Page_Init(object sender, EventArgs e)
        {
            LoadEventData(); 
        }
        protected void rnd_Click(object sender, EventArgs e) //случайные ивенты только для регистрированых юзеров с их листа
        {
            List<EventData> eventDataList = ReadEventDataFromDatabase();
            int r = rnd.Next(eventDataList.Count);
            EventData eventData = eventDataList[r];
            Response.Redirect($"DetailedView.aspx?&id={eventData.Id}");
        }
        protected void rndExpClick(object sender, EventArgs e) //случайные ивенты только для регистрированых юзеров с их листа у которых ттл < 60
        {
            List<EventData> eventDataList = new List<EventData>();
            string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";
            string sql = "SELECT id, eventname, ttl, eventlocation, publiclink, ownerid FROM eventdata WHERE ttl > 0 AND ttl <= 60";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EventData eventData = new EventData();
                            eventData.Id = Guid.Parse(reader["id"].ToString());
                            eventData.Name = reader["eventname"].ToString();
                            eventData.TTL = Convert.ToInt32(reader["ttl"]);
                            eventData.Locations = reader["eventlocation"].ToString().Split(',').ToList();
                            eventData.PublicLink = reader["publiclink"].ToString();
                            eventData.OwnerID = reader["ownerid"].ToString();
                            eventDataList.Add(eventData);
                        }
                    }
                }
            }
            int r = rnd.Next(eventDataList.Count);
            EventData expData = eventDataList[r];
            Response.Redirect($"DetailedView.aspx?&id={expData.Id}");
        }
        protected void displayEventCount() 
        {
            List<EventData> eventDataList = ReadAllData();
            int totalCount = 0, hourCount = 0, validCount = 0;
            foreach (EventData eventData in eventDataList)
            {
                if (eventData.TTL > 0 && eventData.OwnerID == Session["CurrentLogin"] as string)
                    validCount++;
                if (eventData.TTL > 0 && eventData.TTL < 60 && eventData.OwnerID == Session["CurrentLogin"] as string)
                    hourCount++;
                if (eventData.OwnerID == Session["CurrentLogin"] as string)
                    totalCount++;
            }
            validEventCount.Text = "Valid event count "+totalCount.ToString();
            totalEventCount.Text = "Total events created: "+totalCount.ToString();
            hourEventCount.Text = "Events that end in one hour: "+hourCount.ToString();
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e) //функция таймера
        {
            List<EventData> eventDataList = ReadAllData();
            string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";
            foreach(EventData eventData in eventDataList)
            {
                string sql = "UPDATE eventdata SET ttl = " + (eventData.TTL-1).ToString() + " WHERE id = '" + eventData.Id + "' AND ttl > 0";
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                if (eventData.TTL == 0)
                {
                    string sql2 = "UPDATE eventdata SET Id = 'null' WHERE id = '" + eventData.Id + "' AND ttl = 0";
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        using (NpgsqlCommand command = new NpgsqlCommand(sql2, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        protected void btnClose_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
            LoadEventData();
        }
        private static List<EventData> ReadAllData() //статик лист для таймера и подсчета
        {
            List<EventData> eventDataList = new List<EventData>();
            string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";
            string sql = "SELECT id, eventname, ttl, eventlocation, publiclink, ownerid FROM eventdata WHERE ttl > 0";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EventData eventData = new EventData();
                            eventData.Id = Guid.Parse(reader["id"].ToString());
                            eventData.Name = reader["eventname"].ToString();
                            eventData.TTL = Convert.ToInt32(reader["ttl"]);
                            eventData.Locations = reader["eventlocation"].ToString().Split(',').ToList();
                            eventData.PublicLink = reader["publiclink"].ToString();
                            eventData.OwnerID = reader["ownerid"].ToString();
                            eventDataList.Add(eventData);
                        }
                    }
                }
            }

            return eventDataList;
        }
        private void LoadEventData()
        {
            if (Session["RegisteredStatus"] as string == "true")
            {
                List<EventData> eventDataList = ReadEventDataFromDatabase();
                List<string> buttonIds = ViewState["ButtonIds"] as List<string> ?? new List<string>();
                foreach (EventData eventData in eventDataList)
                {
                    string buttonId = "btn" + eventData.Id.ToString("N");
                    if (buttonIds.Contains(buttonId))
                    {
                        Debug.WriteLine("CSV IS CORRUPTED, UUID IS UNUNIQUE");
                        continue;
                    }
                    buttonIds.Add(buttonId);
                    Button button = new Button();
                    button.ID = buttonId;
                    button.CssClass = "container-text button-wrap";
                    button.Text = eventData.Name;
                    button.Click += new EventHandler(btn_Click);

                    HtmlGenericControl listItem = new HtmlGenericControl("li");
                    listItem.Controls.Add(button);

                    container.Controls.Add(listItem);
                    eventDataDict.Add(button.ID, eventData);
                }
                ViewState["ButtonIds"] = buttonIds;
            }
        }

        private List<EventData> ReadEventDataFromDatabase()
        {
            List<EventData> eventDataList = new List<EventData>();
            string connectionString = "Host=dpg-conamq21hbls73ffok50-a.oregon-postgres.render.com;Port=5432;Username=voiceofamerica_user;Password=azfxYN0YTc3VXYoFns5sZ0dvmV1xSa6T;Database=voiceofamerica; SSL Mode=Require";            
            string sql = "SELECT id, eventname, ttl, eventlocation, publiclink, ownerid FROM eventdata WHERE ownerid = '" + Session["CurrentLogin"] as string +"' AND ttl > 0";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EventData eventData = new EventData();
                            eventData.Id = Guid.Parse(reader["id"].ToString());
                            eventData.Name = reader["eventname"].ToString();
                            eventData.TTL = Convert.ToInt32(reader["ttl"]);
                            eventData.Locations = reader["eventlocation"].ToString().Split(',').ToList();
                            eventData.PublicLink = reader["publiclink"].ToString();
                            eventData.OwnerID = reader["ownerid"].ToString();
                            eventDataList.Add(eventData);
                        }
                    }
                }
            }

            return eventDataList;
        }
        protected void btn_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string buttonText = clickedButton.Text;
            EventData eventData = eventDataDict[clickedButton.ID];
            string eventName = eventData.Name;
            int eventTTL = eventData.TTL;
            List<string> eventLocations = eventData.Locations;
            string eventPublicLink = eventData.PublicLink;
            Response.Redirect($"DetailedView.aspx?&id={eventData.Id}");
        }
    }
}