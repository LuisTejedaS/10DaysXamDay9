using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TenDaysOfXamarin.ViewModels.Commands;

namespace TenDaysOfXamarin.ViewModels
{
    public class ExperiencesVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public NewExperienceCommand NewExperienceCommand { get; set; }

        public void NewExperience()
        {
            App.Current.MainPage.Navigation.PushAsync(new MainPage());
        }

        public ExperiencesVM()
        {
            NewExperienceCommand = new NewExperienceCommand(this);
        }
    }
}
