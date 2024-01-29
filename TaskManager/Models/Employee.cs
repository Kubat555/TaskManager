namespace TaskManager.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }

        // Навигационное свойство для связи с проектами, на которых работает сотрудник
        public ICollection<EmployeeProject>? Projects { get; set; }
    }
}
