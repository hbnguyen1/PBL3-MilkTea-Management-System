using PBL3.Core;
using PBL3.Data;
using PBL3.Interface;
using PBL3.Models;

namespace PBL3.Manangers
{
    internal class AuthManager
    {
        public static object Login(string phoneNumber, string password)
        {
            UserAuthenticator authenticator = new UserAuthenticator();
            var currenuser = authenticator.Authenticate(phoneNumber, password);

            if (currenuser != null)
            {
                UserSession.CurrentUser = currenuser;
            }

            return currenuser;
        }
    }
}