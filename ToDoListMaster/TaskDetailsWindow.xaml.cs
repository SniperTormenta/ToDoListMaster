using Microsoft.Win32;
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

namespace ToDoListMaster
{
    public partial class TaskDetailsWindow : Window
    {
        private TaskItem _task;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<TaskItem> _tasks; // Добавляем коллекцию задач
        private Timer _reminderTimer;

        public TaskDetailsWindow(TaskItem task, ObservableCollection<Category> categories, ObservableCollection<TaskItem> tasks)
        {
            InitializeComponent();
            _task = task;
            _categories = categories;
            _tasks = tasks; // Сохраняем коллекцию задач
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
                // Если напоминание не установлено, показываем панель установки
                ReminderSetPanel.Visibility = Visibility.Visible;
                ReminderDisplayPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _reminderTimer.Stop();
            Close();
        }

        // Удаление задачи
        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            // Показываем диалог подтверждения
            var result = MessageBox.Show("Вы уверены, что хотите удалить задачу?", "Подтверждение удаления",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _tasks.Remove(_task); // Удаляем задачу из коллекции
                _reminderTimer.Stop(); // Останавливаем таймер, если он активен
                Close(); // Закрываем окно
            }
        }

        // Изменение категории
        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is Category selectedCategory)
            {
                _task.Category = selectedCategory;
            }
        }

        // Добавление подзадачи
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

        // Удаление подзадачи при двойном клике
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

        // Placeholder для времени напоминания
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

        // Установка напоминания
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

                // Настраиваем таймер
                var timeSpan = reminderTime - DateTime.Now;
                _reminderTimer.Interval = timeSpan.TotalMilliseconds;
                _reminderTimer.Start();

                // Обновляем отображение
                ReminderSetPanel.Visibility = Visibility.Collapsed;
                ReminderDisplayPanel.Visibility = Visibility.Visible;
                DataContext = null;
                DataContext = _task;
            }
            catch
            {
                MessageBox.Show("Пожалуйста, введите время в формате HH:mm.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Изменение времени напоминания
        private void EditReminderButton_Click(object sender, RoutedEventArgs e)
        {
            _reminderTimer.Stop();
            _task.ReminderTime = null;
            ReminderSetPanel.Visibility = Visibility.Visible;
            ReminderDisplayPanel.Visibility = Visibility.Collapsed;
            DataContext = null;
            DataContext = _task;
        }

        // Событие срабатывания таймера
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
                DataContext = null;
                DataContext = _task;
            });
        }

        // Выбор флажка для заметок
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
                var flagPath = flag; // Сохраняем путь для замыкания
                menuItem.Click += (s, args) =>
                {
                    _task.Notes = flagPath;
                    DataContext = null;
                    DataContext = _task;
                };
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
        }

        // Загрузка изображения для вложения
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
                DataContext = null;
                DataContext = _task;
            }
        }
    }
}