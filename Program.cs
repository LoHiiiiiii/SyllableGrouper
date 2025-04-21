var projectDirectory = Directory.GetCurrentDirectory();
var filename = "wordlist.txt";

var path = Path.Combine(projectDirectory, filename);

if (!File.Exists(path)) {
	Console.WriteLine($"{path} not found!");
	Console.ReadKey();
}
string[] words = File.ReadAllLines(path);
Console.WriteLine($"Loaded {words.Length} words.");

var excludeFile = "excludedSyllables.txt";
var excludedSyllables = new HashSet<string>();
path = Path.Combine(projectDirectory, excludeFile);

var minSyllableLength = 2;
var maxSyllableLength = 3;
var groupSize = 4;
var uniqueSetCount = 2;
var skipUniques = false;
var excludeSubstringStarts = false;

if (File.Exists(path)) {
	excludedSyllables = File.ReadAllLines(path).ToHashSet();
	if (excludedSyllables.Count > 0) {
		Console.WriteLine($"Excluding {excludedSyllables.Count} {(excludeSubstringStarts ? "syllable substrings" : "syllables")}.");
	}
}


var startSyllables = new Dictionary<string, HashSet<string>>();
var endSyllables = new Dictionary<string, HashSet<string>>();


if (excludeSubstringStarts) {
	var substrings = new HashSet<string>();
	foreach (var syl in excludedSyllables) {
		substrings.Add(syl.Substring(0, minSyllableLength));
	}
	excludedSyllables = substrings;
}

for (int i = 0; i < words.Length; ++i) {
	for (int j = minSyllableLength; j <= maxSyllableLength; ++j) {
		if (words[i].Length < j + minSyllableLength || words[i].Length > j + maxSyllableLength) {
			continue;
		}

		var startSyl = words[i][..j];
		var endSyl = words[i][j..];

		if (excludeSubstringStarts) {
			if (excludedSyllables.Contains(startSyl[..minSyllableLength])) continue;
			if (excludedSyllables.Contains(endSyl[..minSyllableLength])) continue;
		} else if (excludedSyllables.Contains(startSyl) || excludedSyllables.Contains(endSyl)) continue;

		SyllableLogic.AddSyllable(startSyllables, startSyl, endSyl);
		SyllableLogic.AddSyllable(endSyllables, endSyl, startSyl);
	}

	Console.Write("\r" + "Splitting into syllables: " + (startSyllables.Count));
}
var eliminatedStarts = new HashSet<string>();
var eliminatedEnds = new HashSet<string>();


var startOk = false;
var endOk = false;
var startSize = startSyllables.Count;

Console.WriteLine();
while (!startOk && !endOk) {
	startOk = SyllableLogic.EliminateSyllables(startSyllables, eliminatedEnds, eliminatedStarts, groupSize);
	if (!startOk || !endOk) {
		endOk = SyllableLogic.EliminateSyllables(endSyllables, eliminatedStarts, eliminatedEnds, groupSize);
	}

	Console.Write($"\rEliminating ungroupable syllables: {startSize  - startSyllables.Count}");
}

Console.WriteLine();
var groups = SyllableLogic.GroupSyllables(startSyllables, groupSize);
if (groups.Count == 0) {
	Console.Write("No groups found.");
	Console.ReadKey();
	return;
}
Console.Write($"{groups.Count} groups found.");
Console.WriteLine();

var uniqueSets = SyllableLogic.CreateUniqueSets(groups, uniqueSetCount, (excludeSubstringStarts) ? minSyllableLength : -1, skipUniques);
Console.WriteLine();

var resultSyllables = new List<string>();
var resultWords = new List<string>();
var resultUnique = new HashSet<string>();

if (uniqueSets.Count == 0) {
	Console.Write("No unique sets found.");
	Console.ReadKey();
	return;
}

var wordSetCount = new Dictionary<string, int>();

foreach (var result in uniqueSets) {
	var writeSyllables = new List<string>();
	var writeWords = new List<string>();
	foreach (var set in result) {
		writeSyllables.Add($"{{{string.Join(',', set[0])} | {string.Join(',', set[1])}}}");
		foreach(var start in set[0]) {
			foreach (var end in set[1]) {
				var word = start + end;
				writeWords.Add(word); 
				resultUnique.Add(word);
				int count = 0;
				wordSetCount.TryGetValue(word, out count);
				wordSetCount[word] = count + 1;
			}
		}
	}
	resultSyllables.Add(string.Join(", ", writeSyllables));
	resultWords.Add(string.Join(";", writeWords));
}

var resultNecessary = new List<string>();

foreach (var word in wordSetCount.Keys) {
	if (wordSetCount[word] == resultWords.Count) { 
		resultNecessary.Add(word);
	}
}


Console.WriteLine($"{resultSyllables.Count} sets found with {resultUnique.Count} unique words and {resultNecessary.Count} that are in each set.");

File.WriteAllLines(Path.Combine(projectDirectory, "resultSyllables.txt"), resultSyllables);
File.WriteAllLines(Path.Combine(projectDirectory, "resultWords.txt"), resultWords);
File.WriteAllLines(Path.Combine(projectDirectory, "resultUniques.txt"), resultUnique);
File.WriteAllLines(Path.Combine(projectDirectory, "resultNecessary.txt"), resultNecessary);

Console.ReadKey();