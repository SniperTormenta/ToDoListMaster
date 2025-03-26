using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ToDoListMaster
{
    public partial class CalendarWindow : Window
    {
        private ObservableCollection<TaskItem> _tasks;
        private ObservableCollection<Category> _categories;
        private DateTime _selectedDate;

        public CalendarWindow(ObservableCollection<TaskItem> tasks, ObservableCollection<Category> categories)
        {
            InitializeComponent();
            _tasks = tasks;
            _categories = categories;
            _selectedDate = DateTime.Today;
            CalendarControl.SelectedDate = _selectedDate;
            UpdateTaskList();
        }

        private void CalendarControl_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalendarControl.SelectedDate.HasValue)
            {
                _selectedDate = CalendarControl.SelectedDate.Value;
                UpdateTaskList();
            }
        }

        private void UpdateTaskList()
        {
            var tasksForDate = _tasks.Where(t => t.DueDate.Date == _selectedDate.Date).ToList();
            TasksListBox.ItemsSource = tasksForDate;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CalendarControl.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var addTaskWindow = new AddTaskWindow(_categories, _tasks, CalendarControl.SelectedDate.Value);
            addTaskWindow.ShowDialog();
            UpdateTaskList();
        }

        private void OpenMainWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}