using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    public partial class TaskDetailsWindow : Window
    {
        private TaskItem _task;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<TaskItem> _tasks;
        private ObservableCollection<TaskItem> _archiveTasks;
        private Timer _reminderTimer;
        private const string TasksFilePath = "tasks.json";
        private const string ArchiveFilePath = "archive.json";

        public TaskDetailsWindow(TaskItem task, ObservableCollection<Category> categories,
            ObservableCollection<TaskItem> tasks, ObservableCollection<TaskItem> archiveTasks)
        {
            InitializeComponent();
            _task = task;
            _categories = categories;
            _tasks = tasks;
            _archiveTasks = archiveTasks;
            DataContext = _task;

            // Привязываем категории к ComboBox
            CategoryComboBox.ItemsSource = _categories;
            CategoryComboBox.SelectedItem = _task.Category;

            // Инициализируем таймер
            _reminderTimer = new Timer();
            _reminderTimer.Elapsed += ReminderTimer_Elapsed;

            // Если напоминание уже установлено, запускаем таймер
            if (_task.HasReminder && _task.ReminderTime.HasValue && _task.ReminderTime > DateTime.Now)
            {
                var timeSpan = _task.ReminderTime.Value - DateTime.Now;
                _reminderTimer.Interval = timeSpan.TotalMilliseconds;
                _reminderTimer.Start();
            }
            else
            {
                ReminderSetPanel.Visibility = Visibility.Visible;
                ReminderDisplayPanel.Visibility = Visibility.Collapsed;
            }

            // Подписываемся на изменение чекбокса
            IsCompletedCheckBox.Checked += IsCompletedCheckBox_Checked;
            IsCompletedCheckBox.Unchecked += IsCompletedCheckBox_Unchecked;
        }

        private void IsCompletedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_task.IsCompleted)
            {
                // Переносим задачу в архив
                _tasks.Remove(_task);
                _archiveTasks.Add(_task);
                MessageBox.Show("Задача завершена и перемещена в архив.");

                // Сохраняем изменения
                SaveTasks();
                SaveArchive();

                Close();
            }
        }

        private void IsCompletedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Ничего не делаем, так как задача уже в активных задачах
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _reminderTimer.Stop();
            Close();
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить задачу?", "Подтверждение удаления",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _tasks.Remove(_task);
                _reminderTimer.Stop();
                SaveTasks();
                Close();
            }
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is Category selectedCategory)
            {
                _task.Category = selectedCategory;
            }
        }

        private void AddSubTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new Window
            {
                Title = "Добавить подзадачу",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var textBox = new TextBox { Width = 200, Margin = new Thickness(0, 0, 0, 10) };
            var addButton = new Button { Content = "Добавить", Width = 100 };
            addButton.Click += (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    _task.SubTasks.Add(textBox.Text);
                    inputDialog.Close();
                }
            };
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(addButton);
            inputDialog.Content = stackPanel;
            inputDialog.ShowDialog();
        }

        private void SubTasksListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
            {
                var selectedSubTask = listBox.SelectedItem as string;
                if (selectedSubTask != null)
                {
                    _task.SubTasks.Remove(selectedSubTask);
                }
            }
        }

        private void ReminderTimeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ReminderTimeTextBox.Text == "HH:mm")
            {
                ReminderTimeTextBox.Text = "";
                ReminderTimeTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void ReminderTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReminderTimeTextBox.Text))
            {
                ReminderTimeTextBox.Text = "HH:mm";
                ReminderTimeTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void SetReminderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReminderDatePicker.SelectedDate == null || ReminderTimeTextBox.Text == "HH:mm")
            {
                MessageBox.Show("Пожалуйста, выберите дату и время для напоминания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var date = ReminderDatePicker.SelectedDate.Value;
                var timeParts = ReminderTimeTextBox.Text.Split(':');
                int hours = int.Parse(timeParts[0]);
                int minutes = int.Parse(timeParts[1]);
                var reminderTime = new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0);

                if (reminderTime <= DateTime.Now)
                {
                    MessageBox.Show("Время напоминания должно быть в будущем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _task.HasReminder = true;
                _task.ReminderTime = reminderTime;

                var timeSpan = reminderTime - DateTime.Now;
                _reminderTimer.Interval = timeSpan.TotalMilliseconds;
                _reminderTimer.Start();

                ReminderSetPanel.Visibility = Visibility.Collapsed;
                ReminderDisplayPanel.Visibility = Visibility.Visible;
            }
            catch
            {
                MessageBox.Show("Пожалуйста, введите время в формате HH:mm.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditReminderButton_Click(object sender, RoutedEventArgs e)
        {
            _reminderTimer.Stop();
            _task.ReminderTime = null;
            ReminderSetPanel.Visibility = Visibility.Visible;
            ReminderDisplayPanel.Visibility = Visibility.Collapsed;
        }

        private void ReminderTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _reminderTimer.Stop();
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Напоминание: {_task.Title}", "Напоминание о задаче", MessageBoxButton.OK, MessageBoxImage.Information);
                _task.HasReminder = false;
                _task.ReminderTime = null;
                ReminderSetPanel.Visibility = Visibility.Visible;
                ReminderDisplayPanel.Visibility = Visibility.Collapsed;
            });
        }

        private void NotesTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = new ContextMenu();
            var flags = new[]
            {
                "Flags/YellowLents.png",
                "Flags/GreenLents.png",
                "Flags/BlueLents.png",
                "Flags/PurpleLents.png",
                "Flags/RedLents.png"
            };

            foreach (var flag in flags)
            {
                var menuItem = new MenuItem { Header = flag };
                var flagPath = flag;
                menuItem.Click += (s, args) =>
                {
                    _task.Notes = flagPath;
                };
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
        }

        private void AttachmentTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*",
                Title = "Выберите изображение"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _task.Attachment = openFileDialog.FileName;
            }
        }
    }
}