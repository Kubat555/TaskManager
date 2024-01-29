namespace TaskManager.Models
{
    public class ProjectTask
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string Comment { get; set; }
        public int Priority { get; set; }
        public TaskStatus Status { get; set; }

        public int AuthorId { get; set; }
        public Employee? Author { get; set; }

        public int? ExecutorId { get; set; }
        public Employee? Executor { get; set; }

        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Done
    }
}
