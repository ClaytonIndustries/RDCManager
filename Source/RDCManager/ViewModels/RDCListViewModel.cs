﻿using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using RDCManager.Messages;
using RDCManager.Models;

namespace RDCManager.ViewModels
{
    public class RDCListViewModel : Screen, IHandle<NewRDCConnectionMessage>
    {
        private ObservableCollection<RDCConnection> _rdcCConnections;

        public ObservableCollection<RDCConnection> RDCConnections
        {
            get { return _rdcCConnections; }
            private set { _rdcCConnections = value; NotifyOfPropertyChange(() => RDCConnections); }
        }

        private RDCConnection _selectedRDCConnection;

        public RDCConnection SelectedRDCConnection
        {
            get { return _selectedRDCConnection; }
            set { _selectedRDCConnection = value; NotifyOfPropertyChange(() => SelectedRDCConnection); }
        }

        private readonly IEventAggregator _events;

        private readonly IRDCStarter _rdcStarter;

        private readonly IFileAccess _fileAccess;

        public RDCListViewModel(IEventAggregator events, IRDCStarter rdcStarter, IFileAccess fileAccess)
        {
            _events = events;
            _events.Subscribe(this);

            _rdcStarter = rdcStarter;

            _fileAccess = fileAccess;
        }

        protected override void OnActivate()
        {
            try
            {
                RDCConnections = _fileAccess.Read<ObservableCollection<RDCConnection>>(GetSaveLocation());

                if (RDCConnections.Count > 0)
                {
                    SelectedRDCConnection = RDCConnections.First();
                }
            }
            catch
            {
                RDCConnections = new ObservableCollection<RDCConnection>();
            }
        }

        private string GetSaveLocation()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory + "RDCConnections.xml";
        }

        public void New()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 250;
            settings.Title = "New RDC Connection";

            new WindowManager().ShowDialog(new NewRDCConnectionViewModel(_events), null, settings);
        }

        public void Delete()
        {
            if (SelectedRDCConnection != null)
            {
                RDCConnections.Remove(SelectedRDCConnection);

                if (RDCConnections.Count > 0)
                {
                    SelectedRDCConnection = RDCConnections.First();
                }

                SaveChanges();
            }
        }

        public void Start()
        {
            if (SelectedRDCConnection != null)
            {
                _rdcStarter.StartRDCSession(SelectedRDCConnection.MachineName);
            }
        }

        public void Handle(NewRDCConnectionMessage message)
        {
            RDCConnections.Add(new RDCConnection(message.DisplayName, message.MachineName));

            ListRDCConnectionsAlphabetically();

            if (RDCConnections.Count == 1)
            {
                SelectedRDCConnection = RDCConnections.First();
            }

            SaveChanges();
        }

        private void ListRDCConnectionsAlphabetically()
        {
            RDCConnections = new ObservableCollection<RDCConnection>(RDCConnections.OrderBy(x => x.DisplayName));
        }

        private void SaveChanges()
        {
            try
            {
                _fileAccess.Write(GetSaveLocation(), RDCConnections);
            }
            catch
            {
            }
        }
    }
}
