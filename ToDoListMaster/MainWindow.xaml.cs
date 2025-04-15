using Newtonsoft.Json;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;

namespace ToDoListMaster
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<TaskItem> _tasks;
        private ObservableCollection<TaskItem> _archiveTasks;
        private ObservableCollection<Category> _categories;
        public ICollectionView TaskView { get; set; }
        private const string TasksFilePath = "tasks.json";
        private const string ArchiveFilePath = "archive.json";
        private const string CategoriesFilePath = "categories.json";

        public MainWindow()
        {
            InitializeComponent();
            _tasks = LoadTasks();
            _archiveTasks = LoadArchive();
            _categories = LoadCategories();

            if (_categories == null || _categories.Count == 0)
            {
                _categories = new ObservableCollection<Category>
                {
                    new Category { Id = 1, Name = "ВСЕ" },
                    new Category { Id = 2, Name = "РАБОТА" },
                    new Category { Id = 3, Name = "УЧЕБА" }
                };
                SaveCategories();
            }

            CategoryListBox.ItemsSource = _categories;
            CategoryListBox.SelectedItem = _categories.FirstOrDefault(c => c.Name == "ВСЕ");
            UpdateTaskList();
        }

        private void UpdateTaskList()
        {
            var selectedCategory = CategoryListBox.SelectedItem as Category;
            if (selectedCategory == null || selectedCategory.Name == "ВСЕ")
            {
                TasksListBox.ItemsSource = _tasks;
            }
            else
            {
                TasksListBox.ItemsSource = _tasks.Where(t => t.Category?.Id == selectedCategory.Id).ToList();
            }
        }

        private ObservableCollection<TaskItem> LoadTasks()
        {
            try
            {
                if (File.Exists(TasksFilePath))
                {
                    string json = File.ReadAllText(TasksFilePath);
                    return JsonConvert.DeserializeObject<ObservableCollection<TaskItem>>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new ObservableCollection<TaskItem>();
        }

        private ObservableCollection<TaskItem> LoadArchive()
        {
            try
            {
                if (File.Exists(ArchiveFilePath))
                {
                    string json = File.ReadAllText(ArchiveFilePath);
                    return JsonConvert.DeserializeObject<ObservableCollection<TaskItem>>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading archive: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new ObservableCollection<TaskItem>();
        }

        private ObservableCollection<Category> LoadCategories()
        {
            try
            {
                if (File.Exists(CategoriesFilePath))
                {
                    string json = File.ReadAllText(CategoriesFilePath);
                    return JsonConvert.DeserializeObject<ObservableCollection<Category>>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new ObservableCollection<Category>();
        }

        private void SaveTasks()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_tasks, Formatting.Indented);
                File.WriteAllText(TasksFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveArchive()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_archiveTasks, Formatting.Indented);
                File.WriteAllText(ArchiveFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving archive: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCategories()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_categories, Formatting.Indented);
                File.WriteAllText(CategoriesFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAddTaskDrawerButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем нижнее меню
            MainDrawerHost.IsBottomDrawerOpen = true;
            // Закрываем левое меню, если оно открыто
            MainDrawerHost.IsLeftDrawerOpen = false;
        }

        private void SaveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitleTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (TaskDueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату выполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (TaskCategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите категорию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newTask = new TaskItem
            {
                Title = TaskTitleTextBox.Text,
                Category = TaskCategoryComboBox.SelectedItem as Category,
                DueDate = TaskDueDatePicker.SelectedDate.Value,
                CreatedDate = DateTime.Now,
                HasReminder = TaskHasReminderCheckBox.IsChecked ?? false,
                IsRepeatable = TaskIsRepeatableCheckBox.IsChecked ?? false,
                Notes = "Flags/BlueLents.png",
                Attachment = null,
                IsCompleted = false
            };

            _tasks.Add(newTask);
            SaveTasks();

            // Очищаем поля
            TaskTitleTextBox.Text = string.Empty;
            TaskCategoryComboBox.SelectedIndex = 0;
            TaskDueDatePicker.SelectedDate = null;
            TaskHasReminderCheckBox.IsChecked = false;
            TaskIsRepeatableCheckBox.IsChecked = false;

            // Обновляем список задач
            TaskView.Refresh();

        // Закрываем нижнее меню
#pragma warning disable CS0164 // Отсутствует ссылка на эту метку.
        materialDesign: DrawerHost.CloseDrawerCommand.Execute(null, MainDrawerHost);
#pragma warning restore CS0164 // Отсутствует ссылка на эту метку.
        }

        private void TasksListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
            {
                var selectedTask = listBox.SelectedItem as TaskItem;
                if (selectedTask != null)
                {
                    var taskDetailsWindow = new TaskDetailsWindow(selectedTask, _categories, _tasks, _archiveTasks);
                    taskDetailsWindow.ShowDialog();
                    UpdateTaskList();
                }
            }
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTaskList();
        }

        private void OpenCalendarWindowButton_Click(object sender, RoutedEventArgs e)
        {
            SaveTasks();
            SaveArchive();
            var calendarWindow = new CalendarWindow(_tasks, _archiveTasks, _categories);
            calendarWindow.Show();
            Close();
        }

        private void OpenProfileWindowButton_Click(object sender, RoutedEventArgs e)
        {
            SaveTasks();
            SaveArchive();
            var profileWindow = new ProfileWindow(_tasks, _archiveTasks);
            profileWindow.Show();
            Close();
        }

        private void Button_ClickCategory(object sender, RoutedEventArgs e)
        {
            var addcategoryWindow = new AddCategoryWindow(_categories);
            addcategoryWindow.Show();
            Close();
        }

        private void OpenLeftDrawerButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем левое меню
            MainDrawerHost.IsLeftDrawerOpen = true;
            // Закрываем нижнее меню, если оно открыто
            MainDrawerHost.IsBottomDrawerOpen = false;
        }
        //11




    }
}