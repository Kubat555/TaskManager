using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.DTO
{
    public class AddEmployeeViewModel
    {
        [Required]
        public int ProjectId { get; set; }

        public string? ProjectName { get; set; }

        [Required(ErrorMessage = "Выберите сотрудника")]
        [Display(Name = "Сотрудник")]
        public int SelectedEmployeeId { get; set; }

        public List<SelectListItem>? AvailableEmployees { get; set; }
    }

}
