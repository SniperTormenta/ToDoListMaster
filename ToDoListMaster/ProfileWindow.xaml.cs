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
using System.Windows.Shapes;
using System.IO;

namespace ToDoListMaster
{
    public partial class ProfileWindow : Window
    {
        private ObservableCollection<TaskItem> _tasks;
        private ObservableCollection<TaskItem> _archiveTasks;
        private ObservableCollection<Category> _categories;
        private const string TasksFilePath = "tasks.json";
        private const string ArchiveFilePath = "archive.json";

        public ProfileWindow(ObservableCollection<TaskItem> tasks, ObservableCollection<TaskItem> archiveTasks)
        {
            InitializeComponent();
            _tasks = tasks;
            _archiveTasks = archiveTasks;
            _categories = LoadCategories();
            if (_categories == null || _categories.Count == 0)
            {
                _categories = new ObservableCollection<Category>
                {
                    new Category { Id = 1, Name = "ВСЕ" },
                    new Category { Id = 2, Name = "РАБОТА" },
                    new Category { Id = 3, Name = "УЧЕБА" }
                };
            }
            UpdateTaskCounts();
        }

        private void UpdateTaskCounts()
        {
            var completedTasks = _archiveTasks.Count;
            var incompleteTasks = _tasks.Count(t => !t.IsCompleted);

            CompletedTasksTextBlock.Text = completedTasks.ToString();
            IncompleteTasksTextBlock.Text = incompleteTasks.ToString();
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

        private void OpenMainWindowButton_Click(object sender, RoutedEventArgs e)
        {
            SaveTasks();
            SaveArchive();
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void OpenCalendarWindowButton_Click(object sender, RoutedEventArgs e)
        {
            SaveTasks();
            SaveArchive();
            var calendarWindow = new CalendarWindow(_tasks, _archiveTasks, _categories);
            calendarWindow.Show();
            Close();
        }

        private ObservableCollection<Category> LoadCategories()
        {
            try
            {
                if (System.IO.File.Exists("categories.json"))
                {
                    string json = System.IO.File.ReadAllText("categories.json");
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<Category>>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
    }
}