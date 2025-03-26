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
    public partial class AddTaskWindow : Window
    {
        private ObservableCollection<TaskItem> _tasks;
        private ObservableCollection<Category> _categories;
        private DateTime? _preselectedDate;

        public AddTaskWindow(ObservableCollection<Category> categories, ObservableCollection<TaskItem> tasks, DateTime? preselectedDate = null)
        {
            InitializeComponent();
            _tasks = tasks;
            _categories = categories;
            _preselectedDate = preselectedDate;
            DataContext = this;
            CategoryComboBox.ItemsSource = _categories;

            if (_preselectedDate.HasValue)
            {
                DueDatePicker.SelectedDate = _preselectedDate.Value;
                DueDatePicker.IsEnabled = false;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitleTextBox.Text) || TaskTitleTextBox.Text == "ВВЕДИТЕ НОВУЮ ЗАДАЧУ")
            {
                MessageBox.Show("Пожалуйста, введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newTask = new TaskItem
            {
                Title = TaskTitleTextBox.Text,
                Category = CategoryComboBox.SelectedItem as Category,
                DueDate = DueDatePicker.SelectedDate ?? DateTime.Now,
                CreatedDate = DateTime.Now,
                HasReminder = false,
                IsRepeatable = false,
                Notes = "Flags/BlueLents.png",
                Attachment = null
            };

            _tasks.Add(newTask);
            Close();
        }

        private void TaskTitleTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskTitleTextBox.Text == "ВВЕДИТЕ НОВУЮ ЗАДАЧУ")
            {
                TaskTitleTextBox.Text = "";
                TaskTitleTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TaskTitleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitleTextBox.Text))
            {
                TaskTitleTextBox.Text = "ВВЕДИТЕ НОВУЮ ЗАДАЧУ";
                TaskTitleTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryComboBox.Visibility = CategoryComboBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void DateButton_Click(object sender, RoutedEventArgs e)
        {
            DueDatePicker.Visibility = DueDatePicker.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}