﻿using System.Threading.Tasks;
using Acr.UserDialogs;
using IdentifyMe.Services.Interfaces;
using ReactiveUI;

namespace IdentifyMe.ViewModels
{
    public abstract class ABaseViewModel : ReactiveObject, IABaseViewModel
    {
        protected readonly IUserDialogs DialogService;
        protected readonly INavigationServiceV2 NavigationService;

        protected ABaseViewModel(string name, IUserDialogs userDialogs, INavigationServiceV2 navigationService)
        {
            Name = name;
            DialogService = userDialogs;
            NavigationService = navigationService;
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }
    }
}