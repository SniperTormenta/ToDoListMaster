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
    public partial class ArchiveWindow : Window
    {
        private ObservableCollection<TaskItem> _tasks;
        private ObservableCollection<TaskItem> _archiveTasks;
        private ObservableCollection<Category> _categories;
        private const string TasksFilePath = "tasks.json";
        private const string ArchiveFilePath = "archive.json";

        public ArchiveWindow(ObservableCollection<TaskItem> tasks, ObservableCollection<TaskItem> archiveTasks, ObservableCollection<Category> categories)
        {
            InitializeComponent();
            _tasks = tasks;
            _archiveTasks = archiveTasks;
            _categories = categories;
            ArchiveListBox.ItemsSource = _archiveTasks;
        }

        private void ArchiveListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
            {
                var selectedTask = listBox.SelectedItem as TaskItem;
                if (selectedTask != null)
                {
                    // Возвращаем задачу в активные
                    selectedTask.IsCompleted = false;
                    _archiveTasks.Remove(selectedTask);
                    _tasks.Add(selectedTask);
                    MessageBox.Show("Задача возвращена в активные.");
                    // Сохраняем изменения в файлы перед открытием MainWindow
                    SaveTasks();
                    SaveArchive();
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();

                }
            }
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
            // Сохраняем изменения перед открытием MainWindow
            SaveTasks();
            SaveArchive();

            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void OpenCalendarWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохраняем изменения перед открытием CalendarWindow
            SaveTasks();
            SaveArchive();

            var calendarWindow = new CalendarWindow(_tasks, _archiveTasks, _categories);
            calendarWindow.Show();
            Close();
        }

        private void OpenProfileWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохраняем изменения перед открытием ProfileWindow
            SaveTasks();
            SaveArchive();

            var profileWindow = new ProfileWindow(_tasks, _archiveTasks);
            profileWindow.Show();
            Close();
        }

        private void ArchiveMenuItem_Selected(object sender, RoutedEventArgs e)
        {
            // Мы уже в окне архива, ничего не делаем
        }

        private void BackToMainWindow_Click(object sender, RoutedEventArgs e)
        {
            var BackToMain = new MainWindow();
            BackToMain.Show();
            Close();
        }
    }
}