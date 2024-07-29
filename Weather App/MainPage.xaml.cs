using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace WeatherApp
{
    public partial class MainPage : ContentPage
    {
        RestService _restService;
        private List<string> _allCities = new List<string> // Example list of cities
        {
            "Auckland", "Wellington", "Christchurch", "Hamilton", "Tauranga", "Dunedin", "Palmerston North", "Napier-Hastings", "Rotorua", "New Plymouth",
            "Ponsonby", "Grey Lynn", "Parnell", "Victoria Park", "Grafton", "North Shore", "Takapuna", "Milford", "Browns Bay", "Devonport", "West Auckland",
            "New Lynn", "Henderson", "Glen Eden", "South Auckland", "Manukau", "Papatoetoe", "Otahuhu", "Central Wellington", "Te Aro", "Mount Victoria", 
            "Thorndon", "Karori", "Kilbirnie", "Miramar", "Newtown", "Cathedral Square", "Victoria Square", "Riccarton", "Merivale", "Papanui", "Spreydon", 
            "Hamilton East", "Hamilton West", "Rototuna", "Chartwell", "Glenview", "Dinsdale", "The Octagon", "Dunedin North", "Dunedin South", "Roslyn",
            "St. Clair", "Andersons Bay", "Tauranga Central", "Mount Maunganui", "Brookfield", "Greerton", "Otumoetai", "Mangere", "Middlemore", "Wiri",
            "Pakuranga", "Highland Park", "Farm Cove", "Howick, NZ", "Bucklands Beach", "Mellons Bay", "Botany Downs", "Flat Bush", "East Tamaki", "Cockle Bay",
            "Somerville", "Beachlands", "Maraetai", "Whitford", "Clevedon", "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Gold Coast", "Canberra", 
            "Hobart", "Darwin", "Newcastle", "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas",
            "San Jose"
        };

        public MainPage()
        {
            InitializeComponent();
            _restService = new RestService();
        }

        public Entry CityEntry => cityEntry;

        public async void OnGetWeatherButtonClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CityEntry.Text))
            {
                // Fetch and bind weather data
                WeatherData weatherData = await _restService.GetWeatherData(GenerateRequestURL(Constants.OpenWeatherMapEndpoint));
                BindingContext = weatherData;

                // Fetch and update the current date and time
                await UpdateCurrentDateTime(CityEntry.Text);

                // Simulate fetching additional data (if needed)
                await FetchWeatherAsync();
            }
        }

        private string GenerateRequestURL(string endPoint)
        {
            string requestUri = endPoint;
            requestUri += $"?q={CityEntry.Text}";
            requestUri += "&units=imperial";
            requestUri += $"&APPID={Constants.OpenWeatherMapAPIKey}";
            return requestUri;
        }

        private async Task UpdateCurrentDateTime(string city)
        {
            try
            {
                var currentDateTime = await GetCurrentDateTimeForCity(city);
                currentDateTimeLabel.Text = currentDateTime.ToString("MMMM dd, hh:mm tt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating date and time: {ex.Message}");
                currentDateTimeLabel.Text = "Unable to retrieve date and time";
            }
        }

        private async Task<DateTime> GetCurrentDateTimeForCity(string city)
        {
            var timezone = await GetTimezoneForCity(city);
            if (string.IsNullOrEmpty(timezone))
            {
                return DateTime.UtcNow;
            }

            var client = new HttpClient();
            string url = $"http://worldtimeapi.org/api/timezone/{timezone}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(responseBody);
                var utcDateTimeString = json["utc_datetime"]?.ToString();
                var offset = json["utc_offset"]?.ToString();

                if (string.IsNullOrEmpty(utcDateTimeString) || string.IsNullOrEmpty(offset))
                {
                    throw new Exception("Datetime or offset not found in response");
                }

                var utcDateTime = DateTime.Parse(utcDateTimeString);
                var offsetTimeSpan = TimeSpan.Parse(offset.Substring(1)); // Remove the sign for parsing
                var localDateTime = utcDateTime + offsetTimeSpan;

                return localDateTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching current time: {ex.Message}");
                return DateTime.UtcNow; // Fallback to UTC time
            }
        }

        private async Task<string> GetTimezoneForCity(string city)
        {
            var client = new HttpClient();
            string url = $"http://worldtimeapi.org/api/timezone/{city}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(responseBody);
                var timezoneId = json["timezone"]?.ToString();
                if (string.IsNullOrEmpty(timezoneId))
                {
                    throw new Exception("Timezone not found in response");
                }

                return timezoneId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching timezone: {ex.Message}");
                return null;
            }
        }

        private async Task FetchWeatherAsync()
        {
            await Task.Delay(1000); // Simulate async work

            // Ensure that UI updates are performed on the main thread
            Device.BeginInvokeOnMainThread(() =>
            {
                // Update UI with fetched data (example)
                // Example: someLabel.Text = "Weather data fetched!";
            });
        }

        private void OnCityEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.ToLower() ?? string.Empty;
            var suggestions = _allCities
                .Where(city => city.ToLower().StartsWith(searchText))
                .ToList();

            SuggestionsListView.ItemsSource = suggestions;
            SuggestionsListView.IsVisible = suggestions.Any();
        }

        private void OnSuggestionSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is string selectedCity)
            {
                CityEntry.Text = selectedCity;
                SuggestionsListView.IsVisible = false;
            }
        }
    }
}
