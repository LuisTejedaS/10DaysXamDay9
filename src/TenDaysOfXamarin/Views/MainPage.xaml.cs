﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using SQLite;
using TenDaysOfXamarin.Model;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using TenDaysOfXamarin.ViewModels;

namespace TenDaysOfXamarin
{
    public partial class MainPage : ContentPage
    {
        // added using Plugin.Geolocator;
        // added using Plugin.Geolocator.Abstractions;

        public string YOUR_CLIENT_SECRET = " ";
        public string YOUR_CLIENT_ID = " ";

        IGeolocator locator = CrossGeolocator.Current;
        Position position;
        MainVM viewModel;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainVM();
            BindingContext = viewModel;

            locator.PositionChanged += Locator_PositionChanged;
        }

        void Locator_PositionChanged(object sender, PositionEventArgs e)
        {
            position = e.Position;
        }


        private async void GetLocationPermission()
        {
            // added using Plugin.Permissions;
            // added using Plugin.Permissions.Abstractions;
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationWhenInUse);
            if(status != PermissionStatus.Granted)
            {
                // Not granted, request permission
                if(await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.LocationWhenInUse))
                {
                    // This is not the actual permission request
                    await DisplayAlert("Need your permission", "We need to access your location", "Ok");
                }

                // This is the actual permission request
                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);
                if (results.ContainsKey(Permission.LocationWhenInUse))
                    status = results[Permission.LocationWhenInUse];
            }

            // Already granted (maybe), go on
            if(status == PermissionStatus.Granted)
            {
                // Granted! Get the location
                GetLocation();
            }
            else
            {
                await DisplayAlert("Access to location denied", "We don't have access to your location", "Ok");
            }
        }

        private async void GetLocation()
        {
            position = await locator.GetPositionAsync();
            await locator.StartListeningAsync(TimeSpan.FromMinutes(30), 500);
        }

        void SaveButton_Clicked(object sender, System.EventArgs e)
        {
            Experience newExperience = new Experience()
            {
                Title = viewModel.Title,
                Content = viewModel.Content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                VenueName = viewModel.SelectedVenue.name,
                VenueCategory = viewModel.SelectedVenue.MainCategory,
                VenueLat = float.Parse(viewModel.SelectedVenue.location.Coordinates.Split(',')[0]),
                VenueLng = float.Parse(viewModel.SelectedVenue.location.Coordinates.Split(',')[1])
            };

            int insertedItems = 0;
            // added using SQLite;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabasePath))
            {
                conn.CreateTable<Experience>();
                insertedItems = conn.Insert(newExperience);
            }
            // here the conn has been disposed of, hence closed
            if (insertedItems > 0)
            {
                viewModel.Title = string.Empty;
                viewModel.Content = string.Empty;
                viewModel.SelectedVenue = null;
            }
            else
            {
                DisplayAlert("Error", "There was an error inserting the Experience, please try again", "Ok");
            }
        }

        void ContentEntry_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }

        async void SearchEntry_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.Query))
            {
                string url = $"https://api.foursquare.com/v2/venues/search?ll={position.Latitude},{position.Longitude}&radius=500&query={searchEntry.Text}&limit=3&client_id={YOUR_CLIENT_ID}&client_secret={YOUR_CLIENT_SECRET}&v={DateTime.Now.ToString("yyyyMMdd")}";

                // added using System.Net.Http;
                using (HttpClient client = new HttpClient())
                {
                    // made the method async
                    string json = await client.GetStringAsync(url);

                    // added using Newtonsoft.Json;
                    Search searchResult = JsonConvert.DeserializeObject<Search>(json);
                    viewModel.ShowVenues = true; 
                    venuesListView.ItemsSource = searchResult.response.venues;
                }
            }
            else
            {
                viewModel.ShowVenues = false;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetLocationPermission();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            locator.StopListeningAsync();
        }
    }
}
