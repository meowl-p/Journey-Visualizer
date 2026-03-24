using Journey.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journey.Core.Logic
{
    public static class ProgressCalculator
    {
        public static double CalculateTotalProgress(Project project)
        {
            if (project == null || !project.Milestones.Any()) return 0;

            // Собираем абсолютно все задачи из всех вех в один плоский список
            var allTasks = project.Milestones.SelectMany(m => m.Tasks).ToList();
            if (!allTasks.Any()) return 0;

            double completedCount = allTasks.Count(t => t.IsCompleted);
            return completedCount / allTasks.Count;
        }
    }
}
