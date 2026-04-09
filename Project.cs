using _hash_table;
using _Task;
using _user;
using m_list;
namespace _project
{
    public class Project {
        public int LastTaskID { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public List<_task> tasks { get; set; } = new List<_task>();
        public HashSet<string> t_names { get; set; } = new HashSet<string>();

        public Project(string name)
        {
            Id = -1;
            Name = name;
            tasks = new List<_task>();
            t_names = new HashSet<string>();
        }
       
        public string ToString(HashTable<User> us)
        {
            
            string temp = $"_______________________________\n\nId: {Id}, Название: {Name}\nЗадачи: ";
            if (tasks.Count == 0) temp += "отсутствуют\n";
            else
            {
                temp += '\n';
                for(int i = 0; i < tasks.Count; i++)
                {
                    temp = temp + (i + 1) + "." + tasks[i].ToString(us) + '\n';
                }
            }
            temp += "_______________________________\n";
            return temp;
        }

        public bool AddTask(_task t)
        {
            if (t_names.Contains(t.Title)) return false;
            t.Id = ++LastTaskID;
            tasks.Add(t);
            t_names.Add(t.Title);
            return true;
        }

        public void RemoveTask(_task task)
        {
            if (tasks.Count == 0) return;
            tasks.Remove(task);
            t_names.Remove(task.Title);
            if (tasks.Count == 0) LastTaskID = 0;
        }
    }
}