using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel; // Thêm thư viện này

namespace PBL3.GUI
{
    public partial class wGioHang : Window
    {
        public wGioHang()
        {
            InitializeComponent();
        }
    }

    public class CartItem : INotifyPropertyChanged
    {
        public int ItemID { get; set; } 
        public string? Size { get; set; }
        public string? TenMon { get; set; }
        public string? MoTa { get; set; }
        public int GiaGoc { get; set; } // Lưu giá gốc của 1 ly

        private int _soLuong = 1;
        public int SoLuong
        {
            get { return _soLuong; }
            set
            {
                _soLuong = value;
                OnPropertyChanged(nameof(SoLuong));
                OnPropertyChanged(nameof(ThanhTienStr));
            }
        }

        public int ThanhTien => GiaGoc * SoLuong;
        public string ThanhTienStr => $"{ThanhTien:N0}đ";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class CartManager
    {
        public static ObservableCollection<CartItem> GioHang { get; set; } = new ObservableCollection<CartItem>();
    }
}