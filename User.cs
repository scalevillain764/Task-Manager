namespace _user
{
    public class User
    {
        private static int lastId = LoadStatic(); // хранит последний использованный Id
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }

        public User(string name, string surname, string email, int age)
        {
            Id = lastId++;
            Name = name;
            Surname = surname;
            Email = email;
            Age = age;
            SaveStatic();
        }
        private static int LoadStatic()
        {
            if (!File.Exists("User.txt"))
            {
                File.WriteAllText("User.txt", "1");
                return 1;
            }

            int rez = 0;
            using (StreamReader sr = new StreamReader("User.txt"))
            {
                string inp = sr.ReadLine();

                if (int.TryParse(inp, out rez)) return rez;
            }
            throw new FormatException("User Stream Reader error");
        }

        private void SaveStatic()
        {
            File.WriteAllText("User.txt", lastId.ToString());
        }
        public override bool Equals(object? obj)
        {
            if (obj is not User other) return false;
            return Id == other.Id && Name == other.Name && Surname == other.Surname && Email == other.Email && Age == other.Age;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Surname, Email, Age);
        }

        public override string ToString()
        {
            return $@"
╔════════════════════════════════════════════════════════════════╗
║                        USER PROFILE                            ║
╠════════════════════════════════════════════════════════════════╣
║ {"ID:",-15}{Id,-45}║
║ {"Name:",-15}{Name,-45}║
║ {"Surname:",-15}{Surname,-45}║
║ {"Age:",-15}{Age,-45}║
║ {"Email:",-15}{Email,-45}║
╚════════════════════════════════════════════════════════════════╝
";
        }

        public void print()
        {
            Console.WriteLine($"[User #{Id}] {Name} {Surname}, Возраст: {Age}, Email: {Email}");
        }

        public User()
        {
            Id = lastId++;
            Name = String.Empty;
            Surname = String.Empty;
            Email = String.Empty;
            Age = -1;
        }
    }
}