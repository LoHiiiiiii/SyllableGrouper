using System.Security;

var projectDirectory = Directory.GetCurrentDirectory();
var filename = "wordlist.txt";

var path = Path.Combine(projectDirectory, filename);

if (!File.Exists(path)) {
	Console.WriteLine($"{path} not found!");
	Console.ReadKey();
}
string[] words = File.ReadAllLines(path);

var excludeFile = "excludedSyllables.txt";
var excludedSyllables = new HashSet<string>();
path = Path.Combine(projectDirectory, excludeFile);

if (File.Exists(path)) {
	excludedSyllables = File.ReadAllLines(path).ToHashSet();
	if (excludedSyllables.Count > 0) {
		Console.WriteLine($"Excluding {excludedSyllables.Count} syllables.");
	}
}

var minSyllableLength = 2;
var maxSyllableLength = 3;

var startSyllables = new Dictionary<string, HashSet<string>>();
var endSyllables = new Dictionary<string, HashSet<string>>();

for (int i = 0; i < words.Length; ++i) {
	for (int j = minSyllableLength; j <= maxSyllableLength; ++j) {
		if (words[i].Length < j + minSyllableLength || words[i].Length > j + maxSyllableLength) {
			continue;
		}

		var startSyl = words[i][..j];
		var endSyl = words[i][j..];

		if (excludedSyllables.Contains(startSyl) || excludedSyllables.Contains(endSyl)) continue;

		SyllableLogic.AddSyllable(startSyllables, startSyl, endSyl);
		SyllableLogic.AddSyllable(endSyllables, endSyl, startSyl);
	}

	Console.Write("\r" + "Splitting syllables: " + (i+1));
}
var eliminatedStarts = new HashSet<string>();
var eliminatedEnds = new HashSet<string>();

var setCount = 4;

var startOk = false;
var endOk = false;


var whiteSpace = "";
for (int i = 0; i < startSyllables.Count.ToString().Length + endSyllables.Count.ToString().Length; ++i) {
	whiteSpace += " ";
}

Console.WriteLine();
while (!startOk && !endOk) {
	startOk = SyllableLogic.EliminateSyllables(startSyllables, eliminatedEnds, eliminatedStarts, setCount);
	if (!startOk || !endOk) {
		endOk = SyllableLogic.EliminateSyllables(endSyllables, eliminatedStarts, eliminatedEnds, setCount);
	}

	Console.Write("\r" + "Eliminating syllables: " + (startSyllables.Count + endSyllables.Count) + whiteSpace);
}

var uniqueSetCount = 2;

Console.WriteLine();
var groups = SyllableLogic.GroupSyllables(startSyllables, setCount);
Console.WriteLine();
var skip = false;
var uniqueSets = SyllableLogic.CreateUniqueSets(groups, uniqueSetCount, skip);
Console.WriteLine();

var resultSyllables = new List<string>();
var resultWords = new List<string>();
var resultUnique = new HashSet<string>();

if (uniqueSets.Count == 0) {
	Console.Write("No results found.");
	Console.ReadKey();
	return;
}

foreach (var result in uniqueSets) {
	var writeSyllables = new List<string>();
	var writeWords = new List<string>();
	foreach (var set in result) {
		writeSyllables.Add($"{{{string.Join(',', set[0])} | {string.Join(',', set[1])}}}");
		foreach(var start in set[0]) {
			foreach (var end in set[1]) {
				writeWords.Add(start + end); 
				resultUnique.Add(start + end);
			}
		}
	}
	resultSyllables.Add(string.Join(", ", writeSyllables));
	resultWords.Add(string.Join(";", writeWords));
}

Console.WriteLine($"{resultSyllables.Count} results found with {resultUnique.Count} unique words.");

File.WriteAllLines(Path.Combine(projectDirectory, "resultSyllables.txt"), resultSyllables);
File.WriteAllLines(Path.Combine(projectDirectory, "resultWords.txt"), resultWords);
File.WriteAllLines(Path.Combine(projectDirectory, "resultUniques.txt"), resultUnique);

Console.ReadKey();