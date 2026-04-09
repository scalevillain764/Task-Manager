using _hash_table;
using _project;
using _Task;
using _user;
using m_list;
using System.Globalization;
using System.Linq;
using System.Net.Cache;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
namespace _manager
{
    public class DataModel 
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Project> Projects { get; set; } = new List<Project>();
        public DataModel()
        {
            Users = new List<User>();
            Projects = new List<Project>();
        }
    }

    public sealed class Manager // singleton 
    {
        private int LastUserId = 0;
        private int LastProjectId = 0;

        private static Manager _instance;
        public HashTable<User> users { get; private set; }
        public HashTable<Project> projects { get; private set; }
        private HashSet<string> project_names { get; set; }

        private void Save()
        {
            File.WriteAllText("User.txt", LastUserId.ToString());
            File.WriteAllText("Project.txt", LastProjectId.ToString());

            var data = new DataModel();

            data.Users = users.GetAll();
            data.Projects = projects.GetAll();

            string json = JsonSerializer.Serialize(data);
            File.WriteAllText("data.json", json);
        }

        private void Load()
        {
            if(!File.Exists("User.txt")) File.WriteAllText("User.txt", "1");
            else
            {
                string outp = File.ReadAllText("User.txt");
                if (!int.TryParse(outp, out LastUserId))
                {
                    RedWrite("Непредвиденная ошибка в чтении [User.txt] #10");
                    LastUserId = 0;
                }
            }

            if (!File.Exists("Project.txt")) File.WriteAllText("Project.txt", "1");
            else
            {
                string outp = File.ReadAllText("Project.txt");
                if (!int.TryParse(outp, out LastProjectId))
                {
                    RedWrite("Непредвиденная ошибка в чтении [User.txt] #10");
                    LastProjectId = 0;
                }
            }

            if (!File.Exists("data.json") || new FileInfo("data.json").Length == 0) return;

            string json = File.ReadAllText("data.json");

            var data = JsonSerializer.Deserialize<DataModel>(json);

            if(data != null)
            {
                for (int i = 0; i < data.Users.Count; i++)
                {
                    users.Add(data.Users[i].Id, data.Users[i]);
                }

                for (int i = 0; i < data.Projects.Count; i++)
                {
                    projects.Add(data.Projects[i].Id, data.Projects[i]);
                    data.Projects[i].LastTaskID = data.Projects[i].tasks.Count == 0 ? 0 : data.Projects[i].tasks.Max(t => t.Id);
                }

                for (int i = 0; i < data.Projects.Count; i++)
                {
                    if (data.Projects[i].tasks == null) Console.WriteLine($"Project: {data.Projects[i].Id} | TaskCount: null");
                    else Console.WriteLine($"Project: {data.Projects[i].Id} | TaskCount: {data.Projects[i].tasks.Count}");
                }
            }
        }

        private Manager()
        {
            users = new HashTable<User>();
            projects = new HashTable<Project>();
            project_names = new HashSet<string>(); 
            Load();
        }

        static public Manager Instance
        {
            get
            {
                if(_instance == null)
                     _instance = new Manager();
                return _instance;
            }
        }

        // CRUD
        private void GreenWrite(string str)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        private void RedWrite(string str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        // User
        public void AddUser(User new_user)
        {
            new_user.Id = ++LastUserId;
            users.Add(new_user.Id, new_user);          
        }

        public void RemoveUser(int id)
        {
            var user = users.find(id);
            if(user != null)
            {
                var all_projects = projects.GetAll();
                for(int i = 0; i < all_projects.Count; i++)
                {
                    var task = all_projects[i].tasks;
                    for(int j = 0; j < task.Count; j++)
                    {
                        if (task[j].AssignedUserId == user.Id)
                        {
                            task.Remove(task[j]);
                            j--;
                        }
                    }
                }
                users.Remove(id);
            }
        }

        public User FindUser(int id)
        {
            return users.find(id);
        }

        // Project
        public void AddProject(Project new_project)
        {
            new_project.Id = ++LastProjectId;
            projects.Add(new_project.Id, new_project);
            project_names.Add(new_project.Name);
        }
        public bool RemoveProject(int id)
        {
            var prjc = projects.find(id);
            if (prjc == null) return false;
            projects.Remove(id);
            project_names.Remove(prjc.Name);
            return true;
        }
        public Project FindProject(int id)
        {
            return projects.find(id);
        }
        // Crud

        private Project MakeProject()
        {
            Console.Clear();
            Console.WriteLine("Введите название проекта: ");
            string name = Console.ReadLine();
            Console.Clear();
            return new Project(name);
        }
        private User MakeUser()
        {
            Console.Clear();
            Console.WriteLine("Введите имя пользователя: ");
            string name = Console.ReadLine();
            Console.Clear();

            Console.WriteLine("Введите фамилию: ");
            string surname = Console.ReadLine();
            Console.Clear();

            int age = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Введите возраст: ");
                string age_str = Console.ReadLine();

                if (int.TryParse(age_str, out age))
                {
                    if (age > 0)
                    {
                        Console.Clear();
                        break;
                    }
                    else age = 0;
                }

                RedWrite("Ошибка");
                Console.ReadKey(true);
            }

            Console.WriteLine("Введите email: ");
            string email = Console.ReadLine();

            GreenWrite("Пользотватель успешно добавлен");;

            return new User(name, surname, email, age);
        }
        private _task MakeTask()
        {
            Console.Clear();
            Console.WriteLine("Введите название задания: ");
            string new_t = Console.ReadLine();

            Console.Clear();
            Console.WriteLine("Введите описание: ");
            string new_d = Console.ReadLine();

            Console.Clear();
            Console.WriteLine("Выберете статус: ");
            Console.WriteLine("[1] Сделать задание\n[2] В процессе\n[3] Выполнено");
            var k = Console.ReadKey(true);

            Status status = Status.ToDo;
            switch (k.Key)
            {
                case ConsoleKey.D1:
                    status = Status.ToDo;
                    break;
                case ConsoleKey.D2:
                    status = Status.InProgress;
                    break;
                case ConsoleKey.D3:
                    status = Status.Done;
                    break;
            }

            int id = -1;
            User? user = null;
            bool exit = false;
            while(!exit)
            {
                Console.Clear();
                Console.WriteLine("[1] Добавить пользователя\n[2] Пропустить");
                var opt = Console.ReadKey(true);
                if (opt.Key == ConsoleKey.D1)
                {
                    while (true)
                    {
                        while (true)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите ID нового пользователя: ");
                            string input = Console.ReadLine();
                            if (int.TryParse(input, out id))
                            {
                                if (id > 0) break;
                            }
                            else RedWrite("Ошибка");
                        }

                        try
                        {
                            user = users.find(id);
                            exit = true;
                        }
                        catch (KeyNotFoundException e)
                        {
                            RedWrite("Такого пользователя не существует");
                            Console.ReadKey(true);
                            id = -1;
                        }
                        break;
                    }
                }
                else if (opt.Key == ConsoleKey.D2) break;
            }
            
            return new _task(new_t, new_d, status, id, DateTime.Now);
        }
        
        // Menu
        public void Menu()
        {
            while (true)
            {
                Console.WriteLine("[1] Пользователи\n[2] Проекты\n[ESC] Выход");
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        {
                            UsersMenu();
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            ProjectMenu();
                            break;
                        }
                    case ConsoleKey.Escape: return;
                }
            }
        }
        private void UsersMenu()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("[1] Настройки пользователя\n[2] Добавить пользователя\n[3] Удалить пользователя\n[4] Показать всех пользователей\n[ESC] Выход");
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        {
                            if(users.empty())
                            {
                                Console.Clear();
                                Console.WriteLine("Тут пусто\nНажмите любую кнопку, чтобы выйти.");
                                Console.ReadKey(true);
                                Console.Clear();
                                break;
                            }

                            int ID = 0;
                            while (true)
                            {
                                Console.Clear();
                                Console.Write("Введите ID пользователя: ");
                                if (int.TryParse(Console.ReadLine(), out ID)) break;
                                RedWrite("Ошибка");
                                Console.ReadKey(true);
                            }

                            User? temp = null;
                            try
                            {
                                temp = FindUser(ID);
                            }
                            catch (KeyNotFoundException e)
                            {
                                RedWrite("Такого пользователя не существует");
                            }

                            if (temp != null)
                            {
                                CurUSERmenu(temp);
                                Console.Clear();                                
                                break;
                            }

                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти.");
                            Console.ReadKey(true);
                            Console.Clear();
                            Save();
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            AddUser(MakeUser());

                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти.");
                            Console.ReadKey(true);
                            Console.Clear();
                            Save();
                            break;
                        }
                    case ConsoleKey.D3:
                        {
                            if (users.empty())
                            {
                                Console.Clear();
                                Console.WriteLine("Тут пусто\nНажмите любую кнопку, чтобы выйти.");
                                Console.ReadKey(true);
                                Console.Clear();
                                break;
                            }

                            int ID = 0;
                            while (true)
                            {
                                Console.Clear();
                                Console.WriteLine("Введите ID для удаления");
                                string ID_str = Console.ReadLine();
                                if (int.TryParse(ID_str, out ID)) break;
                                RedWrite("Ошибка");
                                Console.ReadKey(true);
                            }

                            try
                            {
                                FindUser(ID);
                                RemoveUser(ID);
                                GreenWrite("Успешно удалено");
                            }
                            catch (KeyNotFoundException e)
                            {
                                RedWrite(e.Message);
                            }

                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти.");
                            Console.ReadKey(true);
                            Console.Clear();
                            Save();
                            break;
                        }
                    case ConsoleKey.D4:
                        {
                            Console.Clear();
                            if (users.empty())
                            {
                                Console.WriteLine("Тут пусто\nНажмите любую кнопку, чтобы выйти.");
                                Console.ReadKey(true);
                                Console.Clear();
                                break;
                            }

                            int counter = 1;
                            for (int i = 0; i < users.capacity; i++)
                            {
                                Node<User> cur = users[i];
                                while (cur != null)
                                {
                                   Console.Write(counter + ".");
                                   cur.value.print();
                                   cur = cur.next;
                                   counter++;
                                }
                            }

                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти.");
                            Console.ReadKey(true);
                            Console.Clear();
                            Save();
                            break;
                        }
                    case ConsoleKey.Escape: Console.Clear(); return;
                }
            }
        }
        public void ProjectMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("[1] Изменить проект\n[2] Добавить проект\n[3] Удалить проект\n[4] Показать все проекты\n[ESC] - Выход");
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        {
                            if (projects.empty())
                            {
                                Console.Clear();
                                Console.WriteLine("Список проектов пока пуст");
                                Console.ReadKey(true);
                                break;
                            }

                            Project? cur = null;
                            int ID = 0;
                            while (true)
                            {
                                Console.Clear();
                                Console.WriteLine("Введите ID проекта: ");
                                string input = Console.ReadLine();
                                if (int.TryParse(input, out ID)) break;
                                RedWrite("Такого курса нет");
                                Console.ReadKey(true);
                            }

                            try
                            {
                                cur = projects.find(ID);
                            }
                            catch (KeyNotFoundException e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            if(cur != null)
                            {
                                CurPROJECTmenu(cur);
                                Console.Clear();
                                break;
                            }

                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                            Console.ReadKey(true);
                            Save();
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            Project? project = null;
                            while (true)
                            {
                                Console.Clear();
                                project = MakeProject();

                                if (project_names.Contains(project.Name))
                                {
                                    RedWrite("Такой проект уже существует");
                                    project = null;
                                }
                                else
                                {
                                    AddProject(project);
                                    GreenWrite("Проект успешно добавлен");
                                    Console.ReadKey(true);
                                    break;
                                }
                            }

                            bool exit = false;
                            while (!exit)
                            {
                                Console.Clear();
                                Console.WriteLine("[1] Добавить задачу\n[ESCAPE] - Выйти");
                                var option = Console.ReadKey(true);
                                switch (option.Key)
                                {
                                    case ConsoleKey.D1:
                                        {
                                            var task = MakeTask();
                                            if (task != null)
                                            {
                                                if (project.AddTask(task)) GreenWrite("Задание добавлено успешно");

                                            }
                                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                                            Console.ResetColor();
                                            Save();
                                            break;
                                        }
                                    case ConsoleKey.Escape: 
                                        exit = true;
                                        break;
                                }
                            }
                            Save();
                            break;
                        }
                    case ConsoleKey.D3:
                        {
                            if (projects.empty())
                            {
                                Console.Clear();
                                Console.WriteLine("Список проектов пока пуст");
                                Console.ReadKey(true);
                                break;
                            }

                            int ID = 0;
                            while (true)
                            {
                                Console.Clear();
                                Console.WriteLine("Введите ID для удаления");
                                string ID_str = Console.ReadLine();
                                if (int.TryParse(ID_str, out ID)) break;
                                RedWrite("Ошибка");
                                Console.ReadKey(true);
                            }
                            try
                            {
                                FindProject(ID);
                                if (RemoveProject(ID)) GreenWrite("Успешно удалено");
                                else RedWrite("Что-то пошло не так");
                            }
                            catch (KeyNotFoundException e)
                            {
                                RedWrite(e.Message);
                            }

                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти.");
                            Console.ReadKey(true);
                            Console.Clear();
                            Save();
                            break;
                        }
                    case ConsoleKey.D4:

                        if (projects.empty())
                        {
                            Console.Clear();
                            Console.WriteLine("Список проектов пока пуст");
                            Console.ReadKey(true);
                            break;
                        }

                        Console.Clear();
                        List<Project> projs = projects.GetAll();
                        for (int i = 0; i < projs.Count; i++)
                        {
                            Console.WriteLine($"{(i + 1)}.{projs[i].ToString(users)}");
                        }

                        Console.WriteLine("Нажмите любую кнопку, чтобы выйти.");
                        Console.ReadKey(true);
                        Console.Clear();
                        Save();
                        break;
                    case ConsoleKey.Escape:
                        {
                            Console.Clear();
                            return;
                        }
                }
            }
        }
        // Menu

        private void TaskMenu(Project project, _task t, HashTable<User> user)
        {
            Console.Clear();
            Console.WriteLine(t.ToString(user));         
            Console.WriteLine("[1] Изменить название\n[2] Изменить описание\n[3] Изменить статус\n[4] Изменить пользователя\n[ESC] - Выход");
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.D1:
                    {
                        while (true)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите новое название: ");
                            string input = Console.ReadLine();
                            if(project.t_names.Contains(input))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Такое название уже есть в данном проекте");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Название успешно обновлено");
                                Console.ResetColor();
                                t.Title = input;
                                break;
                            }
                        }
                        Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                        Console.ReadKey(true);
                        break;
                    }
                case ConsoleKey.D2:
                    {
                        Console.Clear();
                        Console.WriteLine("Введите новое описание: ");
                        string input = Console.ReadLine();
                        t.Description = input;
                        Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                        Console.ReadKey(true);
                        break;
                    }
                case ConsoleKey.D3:
                    {        
                        Console.Clear();
                        Console.WriteLine("[1] Сделать задание\n[2] В процессе\n[3] Выполнено");
                        var k = Console.ReadKey(true);
                        switch(k.Key)
                        {
                            case ConsoleKey.D1:
                                t.status = Status.ToDo;
                                break;
                            case ConsoleKey.D2:
                                t.status = Status.InProgress;
                                break;

                            case ConsoleKey.D3:
                                t.status = Status.Done;
                                break;
                        }
                        Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                        Console.ReadKey(true);
                        break;
                    }
                case ConsoleKey.D4:
                    {
                        int id = 0;
                        while(true)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите ID нового пользователя: ");
                            string input = Console.ReadLine();

                            if (int.TryParse(input, out id))
                            {
                                GreenWrite("Пользователь успешно обновлен");
                                break;
                            }
                            else Console.WriteLine("Ошибка");
                        }
                        var usr = users.find(id);
                        if (usr == null) RedWrite("Такого пользователя не существует");
                        
                        Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                        Console.ReadKey(true);
                        break;
                    }
                case ConsoleKey.Escape: return;
            }
            Save();
        }

        private void CurPROJECTmenu(Project project)
        {      
            while(true)
            {
                Console.Clear();
                Console.WriteLine(project.ToString(users));
                Console.WriteLine("[1] Изменить название\n[2] Добавить задание\n[3] Удалить задание\n[4] Изменить задание\n[ESC] - Выход");

                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        {
                            while (true)
                            {
                                Console.Clear();
                                Console.WriteLine("Введите новое название: ");
                                string input = Console.ReadLine();
                                if (!project_names.Contains(input))
                                {
                                    GreenWrite("Название проекта успешно изменено");
                                    project.Name = input;
                                    break;
                                }
                                else RedWrite("Такое название уже существует");
                            }
                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                            Console.ReadKey(true);
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            var t = MakeTask();
                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                            project.tasks.Add(t);
                            Console.ReadKey(true);
                            break;
                        }
                    case ConsoleKey.D3:
                        {
                            if (project.tasks.Count == 0)
                            {
                                Console.Clear();
                                Console.WriteLine("Заданий пока нет");
                                break;
                            }

                            int id = 0;
                            while(true)
                            {
                                Console.Clear();
                                Console.WriteLine(project.ToString(users));
                                Console.WriteLine("Введите нужный ID: ");
                                string input = Console.ReadLine();
                                if (int.TryParse(input, out id)) break;
                                RedWrite("Ошибка");
                            }

                            _task? t = null;
                            foreach (var task in project.tasks)
                            {
                                if (task.Id == id)
                                {
                                    t = task;
                                    break;
                                }
                            }

                            if (t != null)
                            {
                                project.RemoveTask(t);
                                GreenWrite("Задача удалена успешно");
                            }
                            else RedWrite("Такой задачи не существует");

                            Console.WriteLine("Нажмите любую кнопку, чтобы выйти");
                            Console.ReadKey(true);
                            break;
                        }
                    case ConsoleKey.D4:
                        {
                            if(project.tasks.Count == 0)
                            {
                                Console.Clear();
                                Console.WriteLine("Заданий пока нет");
                                break;
                            }

                            int ID = 0;
                            while (true)
                            {
                                Console.Clear();
                                for (int i = 0; i < project.tasks.Count; i++)
                                {
                                    Console.WriteLine(project.tasks[i].ToString(users));
                                }

                                Console.WriteLine("Введите ID нужной задачи: ");

                                string input = Console.ReadLine();
                                if (int.TryParse(input, out ID))
                                {
                                    Console.Clear();
                                    break;
                                }
                                Console.WriteLine("Ошибка");
                            }

                            _task? t = null;
                            for(int i = 0; i < project.tasks.Count; i++)
                            {
                                if (project.tasks[i].Id == ID)
                                {
                                    t = project.tasks[i];
                                    break;
                                }
                            }

                            if (t != null) TaskMenu(project, t, users);
                            else RedWrite("Такого задания не существует");
                            break;
                        }
                    case ConsoleKey.Escape: 
                        Save(); 
                        return;
                }
            }
        }
        private void CurUSERmenu(User user)
        {
            
            while (true)
            {
                Console.WriteLine(user.ToString());
                Console.WriteLine("[1] Изменить имя\n[2] Изменить фамилию\n[3] Изменить возраст\n[4] Изменить email\n[ESC] Выход");
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        {
                            Console.Clear();
                            Console.WriteLine("Введите новое имя: ");
                            string old_name = user.Name;

                            user.Name = Console.ReadLine();
                            GreenWrite("Имя успешно изменено");

                            Console.Write("Нажмите любую кнопку, чтобы выйти");
                            Console.ReadKey(true);
                            Console.Clear();
                            break;
                        }                   
                    case ConsoleKey.D3:
                       {                    
                            int new_age = 0;

                            while (true)
                            {
                                Console.Clear();
                                Console.WriteLine("Введите новый возраст: ");
                                string input = Console.ReadLine();

                                if (int.TryParse(input, out new_age)) break;

                                RedWrite("Попробуйте еще раз");
                                Console.ReadKey(true);                              
                            }

                            GreenWrite("Возраст успешно изменен");
                            user.Age = new_age;                       

                            Console.Write("Нажмите любую кнопку, чтобы выйти");
                            Console.ReadKey(true);
                            Console.Clear();
                            break;
                       }
                    case ConsoleKey.D2:
                        {
                            Console.Clear();
                            Console.WriteLine("Введите новую фамилию: ");
                            string old_sname = user.Surname;

                            user.Surname = Console.ReadLine();                            
                            GreenWrite("Фамилия успешно изменена");

                            Console.Write("Нажмите любую кнопку, чтобы выйти");
                            Console.ReadKey(true);
                            Console.Clear();
                            break;
                        }
                    case ConsoleKey.D4:
                        {
                            Console.Clear();
                            Console.WriteLine("Введите новый email: ");
                            string old_email = user.Email;

                            user.Email = Console.ReadLine();
                            GreenWrite("Email успешно изменен");
                            
                            Console.Write("Нажмите любую кнопку, чтобы выйти");
                            Console.ReadKey(true);
                            Console.Clear();
                            break;
                        }
                    case ConsoleKey.Escape:
                        Save();
                        return;
                }
            }
        }

    }
}