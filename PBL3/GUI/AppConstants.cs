namespace PBL3.GUI
{
    /// <summary>
    /// Chứa các hằng số được sử dụng toàn bộ ứng dụng GUI
    /// </summary>
    public static class AppConstants
    {
        // Danh mục sản phẩm
        public const string CATEGORY_ALL = "Tất cả";
        public const string CATEGORY_MILK_TEA = "Trà Sữa";
        public const string CATEGORY_FRUIT_TEA = "Trà Trái Cây";
        public const string CATEGORY_OTHER = "Món khác";

        // Kiểu item trong database
        public const string ITEM_TYPE_MILK_TEA = "Milk Tea";
        public const string ITEM_TYPE_FRUIT_TEA = "Fruit Tea";

        // Trạng thái đơn hàng
        public const string ORDER_STATUS_PENDING = "Pending";
        public const string ORDER_STATUS_COMPLETED = "Completed";
        public const string ORDER_STATUS_CANCELLED = "Cancelled";

        // Hình ảnh mặc định
        public const string DEFAULT_IMAGE_PATH = "/Images/default.jpg";
        public const string DEFAULT_IMAGE_PACK_URI = "pack://application:,,,/Images/default.jpg";

        // Size sản phẩm
        public const string SIZE_MEDIUM = "M";
        public const string SIZE_LARGE = "L";

        // Công thức
        public const string RECIPE_SIZE_MEDIUM = "M";

        // Thông báo
        public const string MSG_BADGE_AVAILABLE = "SẴN SÀNG";
        public const string MSG_BADGE_OUT_OF_STOCK = "TẠM HẾT";
        public const string MSG_NO_PRODUCTS_FOUND = "Không tìm thấy món";
        public const string MSG_NO_PRODUCTS_FOUND_DESC = "Không có sản phẩm nào phù hợp";

        // Giá trị mặc định
        public const double DEFAULT_SUGAR_PERCENTAGE = 100;
        public const double DEFAULT_ICE_PERCENTAGE = 50;

        // Validation
        public const int MIN_PHONE_LENGTH = 10;
        public const int MIN_PASSWORD_LENGTH = 6;

        // Icon emoji
        public const string ICON_ERROR = "❌";
        public const string ICON_WARNING = "⚠";
        public const string ICON_SUCCESS = "✔";

        // Đơn vị tiền tệ
        public const string CURRENCY_SYMBOL = "đ";
    }
}
