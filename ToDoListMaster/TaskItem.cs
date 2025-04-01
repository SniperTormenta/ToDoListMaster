using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListMaster
{
    public class TaskItem
    {
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public Category Category { get; set; }
        public bool HasReminder { get; set; }
        public DateTime? ReminderTime { get; set; }
        public bool IsRepeatable { get; set; }
        public string Notes { get; set; }
        public string Attachment { get; set; }
        public bool IsCompleted { get; set; }
        public List<string> SubTasks { get; set; } = new List<string>();
    }
}