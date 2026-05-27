using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PBL3.src.Application.Interface;
using PBL3.src.Domain.Models;

namespace PBL3.UI.Views
{
    public partial class wThemNhanVien : Window
    {
        private readonly IStaffService _staffService;

        private Staff _editingStaff = null;
        private bool _isEditMode = false;

        public wThemNhanVien()
        {
            InitializeComponent();
            _staffService = Program.ServiceProvider.GetRequiredService<IStaffService>();
        }

        public void SetEditMode(Staff staff)
        {
            _editingStaff = staff;
            _isEditMode = true;

            Title = "Sửa Thông Tin Nhân Viên";
            txtName.Text = staff.Name;
            txtPhone.Text = staff.Phone;
            txtSalary.Text = staff.salaryPerHour.ToString();

            lblPassword.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Collapsed;
            cmbRole.IsEnabled = false;

            btnLuu.Content = "CẬP NHẬT";
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                System.Windows.MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(txtSalary.Text.Trim(), out double salary))
            {
                System.Windows.MessageBox.Show("Lương/giờ phải là số hợp lệ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEditMode)
            {
                bool isSuccess = _staffService.UpdateStaff(_editingStaff.userID, name, phone, salary);

                if (isSuccess)
                {
                    System.Windows.MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; 
                }
                else
                {
                    System.Windows.MessageBox.Show("Cập nhật thất bại! (Có thể số điện thoại này đã được sử dụng)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                string pass = txtPassword.Password;
                if (string.IsNullOrEmpty(pass))
                {
                    System.Windows.MessageBox.Show("Vui lòng nhập mật khẩu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string role = (cmbRole.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
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
}