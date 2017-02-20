#load "user.csx"

using Newtonsoft.Json;

var startWith = Args.FirstOrDefault() ?? "";

if (!string.IsNullOrWhiteSpace(startWith)) {
  Console.WriteLine($"Will show users with names staring with {startWith}");
} else {
  Console.WriteLine("Will show all users");
}

Console.WriteLine("Processing users!");

var rawJson = File.ReadAllText("users.json");
var users = JsonConvert.DeserializeObject<User[]>(rawJson);

var filteredUsers = users.Where(x => x.Name.StartsWith(startWith));

foreach(var user in filteredUsers) {
  Console.WriteLine($"Found: {user.Name} - {user.Email}.");
}
