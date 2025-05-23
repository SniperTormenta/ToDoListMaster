﻿using Newtonsoft.Json;
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
    public partial class CalendarWindow : Window
    {
        private ObservableCollection<TaskItem> _tasks;
        private ObservableCollection<TaskItem> _archiveTasks;
        private ObservableCollection<Category> _categories;
        private DateTime _selectedDate;
        private const string TasksFilePath = "tasks.json";
        private const string ArchiveFilePath = "archive.json";

        public CalendarWindow(ObservableCollection<TaskItem> tasks, ObservableCollection<TaskItem> archiveTasks, ObservableCollection<Category> categories)
        {
            InitializeComponent();
            _tasks = tasks;
            _archiveTasks = archiveTasks;
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

        private void OpenProfileWindowButton_Click(object sender, RoutedEventArgs e)
        {
            SaveTasks();
            SaveArchive();
            var profileWindow = new ProfileWindow(_tasks, _archiveTasks);
            profileWindow.Show();
            Close();
        }
    }
}