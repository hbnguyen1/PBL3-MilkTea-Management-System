using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using PBL3.Service;
using PBL3.Interface;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace PBL3.GUI
{
    public partial class wThemNhanVien : Window
    {
        private readonly IStaffService _staffService;
        public wThemNhanVien()
        {
            InitializeComponent();
            _staffService = Program.ServiceProvider.GetRequiredService<IStaffService>();
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string pass = txtPassword.Password;
            string role = (cmbRole.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(pass))
            {
                System.Windows.MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(txtSalary.Text.Trim(), out double salary))
            {
                System.Windows.MessageBox.Show("Lương/giờ phải là số hợp lệ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            };
            bool isSuccess = _staffService.AddNewStaff(name, phone, pass, role, salary);

            if (isSuccess)
            {
                System.Windows.MessageBox.Show("Tạo tài khoản nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            else
            {
                System.Windows.MessageBox.Show("Số điện thoại này đã được sử dụng trong hệ thống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}