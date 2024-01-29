using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }

        public string CustomerCompany { get; set; }

        public string ExecutorCompany { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Priority { get; set; }

        // Внешний ключ для руководителя проекта
        public int? ProjectManagerId { get; set; }
        public Employee? ProjectManager { get; set; }


        public ICollection<EmployeeProject>? Employees { get; set; }
        public ICollection<ProjectTask>? ProjectTasks { get; set; }
    }
}

