using Nancy;
using Nancy.ModelBinding;
using Nancy.TinyIoc;
using System.Linq;
using SessionsControllerServer.Models;
using SessionsControllerServer.ViewModels;

namespace SessionsControllerServer.Helpers
{
    public class ApiModule : NancyModule
    {
        private readonly MainViewModel _mainWindowViewModel;

        public ApiModule(MainViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            Get["/"] = _ => "Api is working!";

            Get["/users"] = _ => _mainWindowViewModel.UserSessions?.ToList();

            Post["/users"] = _ =>
                {
                    var user = this.Bind<UserSession>();
                    _mainWindowViewModel.AddUser(new UserSessionViewModel(user));
                    return HttpStatusCode.OK;
                };

            Delete["/users/{sessionId}"] = parameters =>
                {
                    var sessionId = parameters.sessionId;
                    _mainWindowViewModel.RemoveUser(sessionId);
                    return HttpStatusCode.OK;
                };

            Get["/sessions/{computerName}"] = parameters =>
                {
                    var computerName = parameters.computerName;
                    return _mainWindowViewModel.UserSessions?
                                                       .Where(user => user.ComputerName == computerName)
                                                       .ToList();
                };
        }
    }
}
