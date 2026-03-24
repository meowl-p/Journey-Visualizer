using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journey.Core.Models
{
    public class Milestone
    {
        public string Title { get; set; } = string.Empty;
        public List<ProjectTask> Tasks { get; set; } = new();

        // Веха завершена, если в ней есть задачи и все они выполнены
        // TODO: add m.GetProgress()

        public bool IsCompleted => Tasks.Any() && Tasks.All(t => t.IsCompleted);
    }
}
