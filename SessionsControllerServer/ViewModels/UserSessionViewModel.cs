using System;
using System.ComponentModel;
using SessionsControllerServer.Models;

namespace SessionsControllerServer.ViewModels
{
    public class UserSessionViewModel : INotifyPropertyChanged
    {
        private UserSession _user;

        public string Status
        {
            get { return _user.Status; }
            set
            {
                _user.Status = value;
                NotifyPropertyChanged(nameof(Status));
            }
        }

        public string UserName
        {
            get { return _user.UserName; }
            set
            {
                _user.UserName = value;
                NotifyPropertyChanged(nameof(UserName));
            }
        }

        public string ComputerName
        {
            get { return _user.ComputerName; }
            set
            {
                _user.ComputerName = value;
                NotifyPropertyChanged(nameof(ComputerName));
            }
        }

        public int SessionId
        {
            get { return _user.SessionId; }
            set
            {
                _user.SessionId = value;
                NotifyPropertyChanged(nameof(SessionId));
            }
        }

        public UserSessionViewModel(UserSession user)
        {
            this._user = user;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
