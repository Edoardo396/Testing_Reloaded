namespace SharedLibrary.Models {
    public class User {
        public User(string name, string surname, string pcHostname) {
            Name = name;
            Surname = surname;
            PCHostname = pcHostname;
        }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string PCHostname { get; set; }

        public override string ToString() {
            return $"{Name} {Surname}";
        }
    }
}