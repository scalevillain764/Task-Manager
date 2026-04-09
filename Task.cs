using _user;
using System.Runtime.CompilerServices;
using _hash_table;
namespace _Task
{
    public enum Status { ToDo = 1, InProgress = 2, Done = 3};
    public class _task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Status status { get; set; }
        public int AssignedUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ToString(HashTable<User> us)
        {
            string temp = string.Empty;
            if (AssignedUserId == -1) temp = "не определен";
            else temp = $"Id: {us.find(AssignedUserId).Id.ToString()} Имя: {us.find(AssignedUserId).Name.ToString()}";
            return $"ID: {Id}, Название: {Title}\nОписание: {Description}\nСтатус: {status.ToString()}\nПользователь: {temp}\nДата создания: {CreatedAt.ToString()}";
        }

        public _task()
        {
            Title = string.Empty;
            Description = string.Empty;
            status = Status.Done;
            AssignedUserId = -1;
            CreatedAt = DateTime.Now;
        }

        public _task(string title, string description, Status status, int AssignedUserId, DateTime CreatedAt)
        {
            /*Id = -1;*/
            Title = title;
            Description = description;
            this.status = status;
            this.AssignedUserId = AssignedUserId;
            this.CreatedAt = CreatedAt;
        }
    }
}