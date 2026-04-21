using PBL3.Interface;

namespace PBL3.Manangers
{
    internal class CustomerManagers
    {
        public static bool Register(string name, string phoneNumber, string password)
        {
            CustomerService customerService = new CustomerService();
            bool isRegistered = customerService.AddNewCustomer(name, phoneNumber, password);

            return isRegistered;
        }
    }
}