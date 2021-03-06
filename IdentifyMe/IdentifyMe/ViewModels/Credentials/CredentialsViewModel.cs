﻿using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using IdentifyMe.Events;
using IdentifyMe.Extensions;
using IdentifyMe.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace IdentifyMe.ViewModels.Credentials
{
    public class CredentialsViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentProvider;
        private readonly ICredentialService _credentialService;
        private readonly ILifetimeScope _scope;
        private readonly IConnectionService _connectionService;
        private readonly IEventAggregator _eventAggregator;

        public CredentialsViewModel(IUserDialogs userDialogs,
                                  INavigationService navigationService,
                                  IAgentProvider agentProvider,
                                  ICredentialService credentialService,
                                  IConnectionService connectionService,
                                  IEventAggregator eventAggregator,
                                  ILifetimeScope scope) :
            base(nameof(CredentialsViewModel), userDialogs, navigationService)
        {
            _agentProvider = agentProvider;
            _credentialService = credentialService;
            _scope = scope;
            _connectionService = connectionService;
            _eventAggregator = eventAggregator;
            Title = "Credentials";
        }

        public async override Task InitializeAsync(object navigationData)
        {
            await base.InitializeAsync(navigationData);
            _eventAggregator.GetEventByType<ApplicationEvent>()
              .Where(_ => _.Type == ApplicationEventType.CredentialRemoved)
              .Subscribe(async _ => await LoadCredential());
            _eventAggregator.GetEventByType<ApplicationEvent>()
              .Where(_ => _.Type == ApplicationEventType.CredentialsUpdated)
              .Subscribe(async _ => await LoadCredential());
            await LoadCredential();
        }

        string _test = "Worked";
        public string Test
        {
            get => _test;
            set => this.RaiseAndSetIfChanged(ref _test, value);
        }

        private async Task LoadCredential()
        {
            try
            {
                IsRefreshing = true;
                var context = await _agentProvider.GetContextAsync();
                var credentialRecordsList = await _credentialService.ListAsync(context);
                if (credentialRecordsList != null)
                {
                    IList<CredentialViewModel> credentialVms = new List<CredentialViewModel>();
                    foreach (var record in credentialRecordsList)
                    {
                        //_listRecords.Add(item);
                        //_listProofRequest.Add(item);
                        var relatedConnection = await _connectionService.GetAsync(context, record.ConnectionId);
                        CredentialViewModel credViewModel = _scope.Resolve<CredentialViewModel>(new NamedParameter("credential", record));
                        credViewModel.RelatedConnection = relatedConnection;
                        if (credViewModel.RelatedConnection.Alias.Name != null)
                            credViewModel.OrganizeName = credViewModel.RelatedConnection.Alias.Name;
                        //credViewModel.CredentialRecord = item;
                        credentialVms.Add(credViewModel);
                    }
                    _credentialVm.Clear();
                    _credentialVm.InsertRange(credentialVms);
                }

                IsRefreshing = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                IsRefreshing = false;
            }
        }

        private RangeEnabledObservableCollection<CredentialViewModel> _credentialVm = new RangeEnabledObservableCollection<CredentialViewModel>();

        public RangeEnabledObservableCollection<CredentialViewModel> CredentialViewModels
        {
            get => _credentialVm;
            set => this.RaiseAndSetIfChanged(ref _credentialVm, value);
        }


        public ICommand RefreshCredentialsCommand => new Command(async () => await LoadCredential());

        private bool _isRefreshing = false;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => this.RaiseAndSetIfChanged(ref _isRefreshing, value);
        }

        public ICommand SelectCredentialCommand => new Command<CredentialViewModel>(async (credentialVm) =>
        {
            if (credentialVm != null)
            {
                await NavigationService.NavigateToAsync<CredentialViewModel>(credentialVm);
            }
        });

    }
}
