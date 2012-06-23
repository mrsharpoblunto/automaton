using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomatonAgent
{
    class Tasks
    {
        public static Tasks Current { get; set; }

        private readonly List<Task>  _tasks;

        public Tasks(List<Task> tasks)
        {
            _tasks = tasks;
        }

        public List<Task> LoadedTasks { 
            get
            {
                return _tasks;
            }
        }

        public Task GetTask(Guid id)
        {
            return _tasks.SingleOrDefault(t => t.Id == id);
        }
    }
}
