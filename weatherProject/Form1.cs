using OpenWeatherMap.Cache;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenWeatherMap.Cache.Enums;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Drawing;
using Nager.Country;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Bunifu.Framework.UI;
using System.Globalization;

/*
thunderstorm, drizzle, rain, snow, clouds, atmosphere etc.
*/

namespace weatherProject
{
    public partial class subPanel : Form
    {
        string apiKey = "50de9888d40185b7dbdf1f939e4b325d";

        private readonly OpenWeatherMapCache _openWeatherMapCache;
        private static readonly HttpClient client = new HttpClient();
        ICountryProvider countryProvider = new CountryProvider();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private bool locked = false;

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private bool isExpanded = false;

        private int originalHeight = 167;
        private int expandedHeight = 435;

        private bool temperatireDisplay = true;
        private bool humidityDisplay = false;
        private bool visibilityDisplay = false;
        private bool windSpeedDisplay = false;
        private bool pressureDisplay = false;
        private bool descriptionDisplay = true;
        private bool timeDisplay = true;


        public subPanel()
        {

            InitializeComponent();          
            _openWeatherMapCache = new OpenWeatherMapCache("50de9888d40185b7dbdf1f939e4b325d", 9_500, FetchMode.AlwaysUseLastMeasuredButExtendCache, 300_000);
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            cityNameLabel.Text = "Cebu";
            updateWeather(cityNameLabel.Text);
            initializeForecast(cityNameLabel.Text);

            this.Height = originalHeight;
            menuPanel.Location = new Point(1, 1);

            UpdateTimeForAllTimezones();

            humidityCB.Checked = false;
            visibilityCB.Checked = false;
            windSpeedCB.Checked = false;
            pressureCB.Checked = false;

            //default configuration
            weatherCB.Checked = true;
            TemperatureCB.Checked = true;
            descriptionCB.Checked = true;
            countryCB.Checked = true;
            timeCB.Checked = true;

            updateChanges();
        }

        void updateChanges()
        {
            humidityLabel.Visible = humidityCB.Checked;
            humidityTitle.Visible = humidityCB.Checked;

            visibilityLabel.Visible = visibilityCB.Checked;
            visibilityTitle.Visible = visibilityCB.Checked;

            windSpeedLabel.Visible = windSpeedCB.Checked;
            windTitle.Visible = windSpeedCB.Checked;

            pressureLabel.Visible = pressureCB.Checked;
            pressureTitle.Visible = pressureCB.Checked;

            weatherLabel.Visible = weatherCB.Checked;
            temperatureLabel.Visible = TemperatureCB.Checked;
            descriptionLabel.Visible = descriptionCB.Checked;
            countryLabel.Visible = countryCB.Checked;
            timeLabel.Visible = timeCB.Checked;
        }




        private async void updateWeather(String inputCity)
        {
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={inputCity}&APPID={apiKey}";
                        
            try
            {
                string responseBody = await client.GetStringAsync(url);
                JObject weatherData = JObject.Parse(responseBody);

                string detailWeather = weatherData["weather"][0]["description"].ToString();
                string temperature = (weatherData["main"]["temp"].ToObject<double>() - 273.15).ToString("F1");
                string iconCode = weatherData["weather"][0]["icon"].ToString();
                string country = weatherData["sys"]["country"].ToString();
                string LanguageCode = weatherData["sys"]["country"].ToString();

                string mainWeather = weatherData["weather"][0]["main"].ToString();

                string humidity = weatherData["main"]["humidity"].ToString();
                string pressure = weatherData["main"]["pressure"].ToString();
                string wind = weatherData["wind"]["speed"].ToString();
                string visibility = weatherData["visibility"].ToString();
                int convertedVisibility = int.Parse(visibility) /1000;
                visibility = convertedVisibility.ToString();


                var countryInfo = countryProvider.GetCountry(LanguageCode);

                temperatureLabel.Text = $"{temperature}°";
                countryLabel.Text = $"{countryInfo.CommonName}";
                weatherLabel.Text = $"{mainWeather}";
                descriptionLabel.Text = $"{detailWeather}";

                visibilityLabel.Text = $"{visibility} km";
                windSpeedLabel.Text = $"{wind} km/h";
                humidityLabel.Text = $"{humidity} %";
                pressureLabel.Text = $"{pressure} mb";

                formChangeColor(weatherLabel.Text);


            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Invalid Input: City/Country does not exist");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

        }



        private async void initializeForecast(String inputCity)
        {
            string url = $"http://api.openweathermap.org/data/2.5/forecast?q={inputCity}&appid={apiKey}";

            Label[] forecastDate = {forecastDay1, forecastDay2, forecastDay3,
             forecastDay4, forecastDay5};

            Label[] mainWeather = { forecastWeather1, forecastWeather2, forecastWeather3,
             forecastWeather4, forecastWeather5};

            Label[] descriptionWeather = { forecastDescription1, forecastDescription2, forecastDescription3,
             forecastDescription4, forecastDescription5};

            Label[] temperature = { temperature1 , temperature2 , temperature3 ,
                temperature4 , temperature5};

            BunifuImageButton[] forecastIcon = { forecastIcon1 , forecastIcon2 , forecastIcon3 ,
                forecastIcon4 , forecastIcon5};

            try
            {
                string responseBody = await client.GetStringAsync(url);
                JObject forecastData = JObject.Parse(responseBody);

                Console.WriteLine(forecastData["list"][0]["weather"][0].ToString());


                forecastDay1.Text = "tests";
                for (int ctr = 0; forecastDate.Length > ctr; ctr++)
                {
                    DateTime dateTime = DateTime.Parse(forecastData["list"][ctr*8]["dt_txt"].ToString());
                    string abbreviatedDayOfWeek = dateTime.ToString("ddd", CultureInfo.InvariantCulture);
                    mainWeather[ctr].Text = forecastData["list"][ctr * 8]["weather"][0]["main"].ToString();
                    descriptionWeather[ctr].Text = forecastData["list"][ctr * 8]["weather"][0]["description"].ToString();
                    temperature[ctr].Text = $"{(double.Parse(forecastData["list"][ctr * 8]["main"]["temp"].ToString()) - 273.15).ToString("F2")}°";

                    if (ctr == 0)
                    {
                        forecastDate[ctr].Text = "Today";

                    }
                    else {
                        forecastDate[ctr].Text = $"{abbreviatedDayOfWeek} {dateTime.Day}";
                    }


                    if (mainWeather[ctr].Text == "Clouds") { forecastIcon[ctr].Image = Properties.Resources.cloud_svgrepo_com; }
                    else if (mainWeather[ctr].Text == "Rain") { forecastIcon[ctr].Image = Properties.Resources.rainIcon; }
                    else if (mainWeather[ctr].Text == "Snow") { forecastIcon[ctr].Image = Properties.Resources.snow2; }
                    else if (mainWeather[ctr].Text == "Sunny") { forecastIcon[ctr].Image = Properties.Resources.sunny3; }
                    else if (mainWeather[ctr].Text == "Thunderstorm") { forecastIcon[ctr].Image = Properties.Resources.tunder; }
                    else{ forecastIcon[ctr].Image = Properties.Resources.tunder; }


                }

            }
            catch (HttpRequestException ex)
            {
            }
            catch (Exception ex)
            {
            }









        }


        private void UpdateTimeForAllTimezones()
        {
            DateTime utcNow = DateTime.UtcNow;
            timeLabel.Text = $"UTC: {utcNow.ToString("HH:mm:ss")}";
        }


        private void formChangeColor(String weather) {
        {
                Image backgroundImage = Properties.Resources.sunny2;


                if (weather == "Clouds")
            {
                backgroundImage = Properties.Resources.cloudy;                                         
                mainPanel.GradientBottomLeft = Color.LightSteelBlue;
                mainPanel.GradientBottomRight = Color.Olive;
                mainPanel.GradientTopLeft = Color.Ivory;
                mainPanel.GradientTopRight = Color.SlateBlue;
            }

            else if (weather == "Sunny")
            {
                backgroundImage = Properties.Resources.sunny2;
                mainPanel.GradientBottomLeft = Color.Orange;
                mainPanel.GradientBottomRight = Color.IndianRed;
                mainPanel.GradientTopLeft = Color.SlateBlue;
                mainPanel.GradientTopRight = Color.RoyalBlue;
                
            }

            else if (weather == "Snow")
            {
                backgroundImage = Properties.Resources.snow1;
                mainPanel.GradientBottomLeft = Color.WhiteSmoke;
                mainPanel.GradientBottomRight = Color.Silver;
                mainPanel.GradientTopLeft = Color.SlateGray;
                mainPanel.GradientTopRight = Color.LightSteelBlue;
            }

            else if (weather == "Thundestorm")
            {
                backgroundImage = Properties.Resources.thunder;
                mainPanel.GradientBottomLeft = Color.Thistle;
                mainPanel.GradientBottomRight = Color.Violet;
                mainPanel.GradientTopLeft = Color.DarkViolet;
                mainPanel.GradientTopRight = Color.DarkSlateBlue;
                

                }


                else if (weather == "Rain" || weather == "Drizzle")
                {
                    backgroundImage = Properties.Resources._000f72fa82485a9cb19b7c7978391ca91; ;
                    mainPanel.GradientBottomLeft = Color.White;
                    mainPanel.GradientBottomRight = Color.MediumPurple;
                    mainPanel.GradientTopLeft = Color.LightSteelBlue;
                    mainPanel.GradientTopRight = Color.DarkSlateBlue;
                    
                }


                else
              {
                    backgroundImage = Properties.Resources.sunny2;

                    mainPanel.GradientBottomLeft = Color.Orange;
                  mainPanel.GradientBottomRight = Color.IndianRed;
                  mainPanel.GradientTopLeft = Color.SlateBlue;
                  mainPanel.GradientTopRight = Color.RoyalBlue;
                

                }

                subPanel.ActiveForm.BackgroundImage = backgroundImage;


            }




        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && locked == false)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = false;
            }
        }
        
        private void ButtonExpand_Click(object sender, EventArgs e)
        {
            if (isExpanded == false)
            {
                
                bunifuImageButton3.Image = Properties.Resources.down_2_svgrepo_com;

                this.Height = originalHeight;
            }
            else
            {
                
                this.Height = expandedHeight;
                bunifuImageButton3.Image = Properties.Resources.up_2_svgrepo_com;

            }

            isExpanded = !isExpanded;
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateTimeForAllTimezones();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            updateWeather(cityNameLabel.Text);
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            menuPanel.Visible = !menuPanel.Visible;
        }

        private void menuPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void bunifuMaterialTextbox1_OnValueChanged(object sender, EventArgs e)
        {

        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void applyChange_Click(object sender, EventArgs e)
        {
            updateWeather(cityNameLabel.Text);
            initializeForecast(cityNameLabel.Text);
            updateChanges();



        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void weatherLabel2_Click(object sender, EventArgs e)
        {

        }

        private void bunifuCustomLabel1_Click(object sender, EventArgs e)
        {

        }

        private void bunifuGradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void bunifuCustomLabel1_Click_1(object sender, EventArgs e)
        {

        }

        private void bunifuCustomLabel3_Click(object sender, EventArgs e)
        {

        }

        private void bunifuCustomLabel21_Click(object sender, EventArgs e)
        {

        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void bunifuImageButton5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void bunifuImageButton7_Click(object sender, EventArgs e)
        {
            updateWeather(cityNameLabel.Text);
            initializeForecast(cityNameLabel.Text);


        }

        private void bunifuImageButton6_Click(object sender, EventArgs e)
        {
            locked = !locked;
            if (locked != true)
                bunifuImageButton6.Image = Properties.Resources.lock_keyhole_minimalistic_unlocked_svgrepo_com__1_;
            else
                bunifuImageButton6.Image = Properties.Resources.lock_svgrepo_com;

        }

        private void formOnEnter(object sender, EventArgs e)
        {
            bunifuImageButton2.Visible = true;
            bunifuImageButton6.Visible = true;
            bunifuImageButton5.Visible = true;
            bunifuImageButton1.Visible = true;
        }

        private void formOnExit(object sender, EventArgs e)
        {
            bunifuImageButton2.Visible = false;
            bunifuImageButton6.Visible = false;
            bunifuImageButton5.Visible = false;
            bunifuImageButton1.Visible = false;
        }
    } 
}
