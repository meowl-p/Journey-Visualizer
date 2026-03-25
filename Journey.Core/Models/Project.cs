using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journey.Core.Models
{
    public class Project
    {
        public string Name { get; set; } = string.Empty;
        public List<Milestone> Milestones { get; set; } = new();

        public string Goal { get; set; } = ""; //Цель
        public string Motivation { get; set; } = ""; // Чтоо будет, если я сделаю?
        public string FailureCost { get; set; } = ""; // Что будет, если брошу?
        public DateTime TargetDate { get; set; } // Целевая дата

        public DateTime LastVisitDate { get; set; } = DateTime.Now;
        public double LastProgressValue { get; set; } = 0;
    }
}
