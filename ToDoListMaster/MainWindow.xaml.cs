﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace ToDoListMaster
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TaskItem> Tasks { get; set; }
        public ObservableCollection<TaskItem> ArchiveTasks { get; set; }
        public ObservableCollection<Category> Categories { get; set; }
        public ICollectionView TaskView { get; set; }
        private const string TasksFilePath = "tasks.json";
        private const string ArchiveFilePath = "archive.json";
        private const string CategoriesFilePath = "categories.json";

        public MainWindow()
        {
            InitializeComponent();

            // Загружаем категории
            Categories = LoadCategories();
            if (Categories == null || Categories.Count == 0)
            {
                Categories = new ObservableCollection<Category>
                {
                    new Category { Id = 1, Name = "ВСЕ" },
                    new Category { Id = 2, Name = "РАБОТА" },
                    new Category { Id = 3, Name = "УЧЕБА" }
                };
            }

            // Загружаем задачи
            Tasks = LoadTasks();
            if (Tasks == null)
            {
                Tasks = new ObservableCollection<TaskItem>
                {
                    new TaskItem
                    {
                        Title = "Сдать долги по учебе",
                        DueDate = new DateTime(2024, 10, 15),
                        CreatedDate = DateTime.Now,
                        Category = Categories[2],
                        HasReminder = false,
                        IsRepeatable = false,
                        Notes = "Flags/BlueLents.png",
                        Attachment = null,
                        IsCompleted = true
                    },
                    new TaskItem
                    {
                        Title = "Сдать проект",
                        DueDate = new DateTime(2024, 10, 20),
                        CreatedDate = DateTime.Now,
                        Category = Categories[1],
                        HasReminder = false,
                        IsRepeatable = false,
                        Notes = "Flags/BlueLents.png",
                        Attachment = null,
                        IsCompleted = false
                    }
                };
            }

            // Загружаем архив
            ArchiveTasks = LoadArchive();
            if (ArchiveTasks == null)
            {
                ArchiveTasks = new ObservableCollection<TaskItem>();
            }

            // Переносим завершённые задачи в архив при загрузке
            var completedTasks = Tasks.Where(t => t.IsCompleted).ToList();
            foreach (var task in completedTasks)
            {
                Tasks.Remove(task);
                ArchiveTasks.Add(task);
            }

            // Создаем ICollectionView для фильтрации задач
            TaskView = CollectionViewSource.GetDefaultView(Tasks);
            TaskView.Filter = FilterTasks;

            // Устанавливаем контекст данных
            DataContext = this;

            // Подписываемся на событие закрытия окна
            Closing += MainWindow_Closing;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var addTaskWindow = new AddTaskWindow(Categories, Tasks);
            addTaskWindow.ShowDialog();
            TaskView.Refresh();
        }

        private void TaskListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
            {
                var selectedTask = listBox.SelectedItem as TaskItem;
                if (selectedTask != null)
                {
                    var taskDetailsWindow = new TaskDetailsWindow(selectedTask, Categories, Tasks, ArchiveTasks);
                    taskDetailsWindow.ShowDialog();
                }
            }
        }

        private bool FilterTasks(object item)
        {
            var task = item as TaskItem;
            if (task == null) return false;

            var selectedCategory = CategoryFilterComboBox.SelectedItem as Category;
            if (selectedCategory == null) return true;

            if (selectedCategory.Name == "ВСЕ") return true;

            return task.Category != null && task.Category.Id == selectedCategory.Id;
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TaskView.Refresh();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveTasks();
            SaveArchive();
            SaveCategories();
        }

        private void SaveTasks()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Tasks, Formatting.Indented);
                File.WriteAllText(TasksFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            return null;
        }

        private void SaveArchive()
        {
            try
            {
                string json = JsonConvert.SerializeObject(ArchiveTasks, Formatting.Indented);
                File.WriteAllText(ArchiveFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving archive: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            return null;
        }

        private void SaveCategories()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Categories, Formatting.Indented);
                File.WriteAllText(CategoriesFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            return null;
        }

        private void OpenCalendarWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var calendarWindow = new CalendarWindow(Tasks, ArchiveTasks, Categories);
            calendarWindow.Show();
            Close();
        }

        private void OpenProfileWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow(Tasks, ArchiveTasks);
            profileWindow.Show();
            Close();
        }

        private void ArchiveMenuItem_Selected(object sender, RoutedEventArgs e)
        {
            var archiveWindow = new ArchiveWindow(Tasks, ArchiveTasks, Categories);
            archiveWindow.Show();
            Close();
        }
    }

    public class TaskItem
    {
        public string Title { get; set; }
        public Category Category { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool HasReminder { get; set; }
        public DateTime? ReminderTime { get; set; }
        public bool IsRepeatable { get; set; }
        public string Notes { get; set; } = "Flags/BlueLents.png";
        public string Attachment { get; set; }
        public ObservableCollection<string> SubTasks { get; set; } = new ObservableCollection<string>();
        public bool IsCompleted { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}