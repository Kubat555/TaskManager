using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Models;

namespace TaskManager.DTO
{
    public class ProjectCreateViewModel
    {
        public Project? Project { get; set; }
        public IEnumerable<SelectListItem>? Employees { get; set; }
        public int SelectedManagerId { get; set; }
    }
}
