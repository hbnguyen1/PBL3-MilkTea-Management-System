using PBL3.src.Domain.Models;

namespace PBL3.src.Application
{
    public static class UserSession
    {
        private static readonly object _sessionLock = new object();
        private static Users? _currentUser;

        public static Users? CurrentUser
        {
            get
            {
                lock (_sessionLock) { return _currentUser; }
            }
            set
            {
                lock (_sessionLock) { _currentUser = value; }
            }
        }
    }
}