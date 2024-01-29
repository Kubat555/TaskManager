using TaskManager.Models;

namespace TaskManager.DTO
{
    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; }
        public Employee ProjectManager { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
