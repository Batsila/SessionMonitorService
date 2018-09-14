using System.Linq;
using SessionsControllerServer.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SessionsControllerServer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainViewModel()
        {
            UserSessions = new ObservableCollection<UserSessionViewModel>();

            LogoutUserCommand = new RelayCommand(obj =>
            {
                if (obj is UserSessionViewModel user)
                {
                    var logoutUser = UserSessions.FirstOrDefault(u => u.UserName == user.UserName);
                    if (logoutUser != null)
                        logoutUser.Status = "Logoff";
                }
            });

            LockUserCommand = new RelayCommand(obj =>
            {
                if (obj is UserSessionViewModel user)
                {
                    var logoutUser = UserSessions.FirstOrDefault(u => u.UserName == user.UserName);
                    if (logoutUser != null)
                        logoutUser.Status = "Lock";
                }
            });
        }

        public ICommand LogoutUserCommand { get; private set; }
        public ICommand LockUserCommand { get; private set; }

        private UserSessionViewModel _selectedUser;
        public UserSessionViewModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                NotifyPropertyChanged(nameof(SelectedUser));
            }
        }

        public ObservableCollection<UserSessionViewModel> UserSessions { get; set; }
        
        public void AddUser(UserSessionViewModel user)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var currentUser = UserSessions.FirstOrDefault(u => u.SessionId == user.SessionId);
                if (currentUser != null)
                {
                    UserSessions.Remove(currentUser);
                }
                UserSessions.Add(user);
            }));
        }

        public void RemoveUser(int sessionId)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (UserSessions.Any(user => user.SessionId == sessionId))
                {
                    UserSessions.Remove(UserSessions.First(user => user.SessionId == sessionId));
                }
            }));
        }
    }
}
