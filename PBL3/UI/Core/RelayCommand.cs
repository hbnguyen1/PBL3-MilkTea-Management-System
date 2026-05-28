using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PBL3.UI.Core
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Đăng ký sự kiện thay đổi trạng thái có được thực hiện hay không
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value;}
        }
        // Constuctor nhận vào 2 hàm, 1 hàm để chạy, 1 hàm để kiểm tra điều kiện
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Kiểm tra xem 1 nút bấm có thể bấm hay không
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
