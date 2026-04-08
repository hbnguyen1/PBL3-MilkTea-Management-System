using PBL3.Interface;

namespace PBL3.Manangers
{
    internal class CustomerManagers
    {
        public static bool Register(string name, string phoneNumber, string password)
        {
            CustomerService customerService = new CustomerService();
            // Hàm AddNewCustomer sẽ xử lý việc lưu vào Database và trả về true/false
            bool isRegistered = customerService.AddNewCustomer(name, phoneNumber, password);

            return isRegistered;
        }
    }
}