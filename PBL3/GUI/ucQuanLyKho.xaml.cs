using System;
using System.Windows;
using System.Windows.Controls;
using PBL3.Manangers;

namespace PBL3.GUI
{
    public partial class ucQuanLyKho : System.Windows.Controls.UserControl
    {
        private IngredientManager _manager = new IngredientManager();

        public ucQuanLyKho()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            dgKho.ItemsSource = _manager.GetAllIngredients();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txtTen.Text.Trim();
                string unit = cmbDonVi.Text;
                int count = int.Parse(txtSoLuong.Text);
                int price = int.Parse(txtGia.Text);

                if (_manager.AddIngredient(name, price, unit, count))
                {
                    System.Windows.MessageBox.Show("Thêm nguyên liệu thành công!", "Thông báo");
                    LoadData();

                    txtTen.Clear();
                    txtSoLuong.Clear();
                    txtGia.Clear();
                    cmbDonVi.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Vui lòng kiểm tra lại dữ liệu nhập vào!", "Lỗi");
            }
        }

        private void btnCanhBao_Click(object sender, RoutedEventArgs e)
        {
            dgKho.ItemsSource = _manager.GetLowStockIngredients();
        }
    }
}