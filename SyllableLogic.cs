
public static class SyllableLogic {
	public static void AddSyllable(Dictionary<string, HashSet<string>> dict, string key, string value) {
		if (key == value) return;
		if (!dict.ContainsKey(key)) {
			dict.Add(key, new HashSet<string>());
		}
		dict[key].Add(value);
	}

	public static bool EliminateSyllables(Dictionary<string, HashSet<string>> dict, HashSet<string> eliminatedInternal, HashSet<string> eliminatedExternal, int minSetCount) {

		var newDict = new Dictionary<string, HashSet<string>>();
		var newSet = new HashSet<string>();
		var unchanged = true;

		foreach (var key in dict.Keys) {
			foreach (var value in dict[key]) {
				if (!eliminatedInternal.Contains(value)) {
					newSet.Add(value);
				}
			}

			if (newSet.Count() >= minSetCount) {
				newDict[key] = [.. newSet];
			} else {
				unchanged = false;
				eliminatedExternal.Add(key);
			}

			newSet.Clear();
		}

		eliminatedInternal.Clear();
		dict.Clear();

		foreach (var key in newDict.Keys) {
			dict.Add(key, newDict[key]);
		}

		return unchanged;
	}

	public static List<List<HashSet<string>[]>> GroupUniques(Dictionary<string, HashSet<string>> startDict, int setCount, int uniqueCount) {

		var startKeys = startDict.Keys.ToArray();
		string[][] searchArrays;
		List<string> potentialResult = new List<string>();
		var results = new List<HashSet<string>[]>();

		string print = "";
		int maxSymbol = 0;

		for (int i = 0; i < startKeys.Length - setCount; ++i) {

			searchArrays = CreateCombinations(startDict[startKeys[i]].ToArray(), setCount);

			for (int j = 0; j < searchArrays.Length; ++j) {
				potentialResult.Clear();
				for (int k = i + 1; k < startKeys.Length; ++k) {
					var containsAll = true;
					foreach (var end in searchArrays[j]) {
						if (!startDict[startKeys[k]].Contains(end)) {
							containsAll = false;
							break;
						}
					}
					if (containsAll) {
						potentialResult.Add(startKeys[k]);
					}
				}

				print = "\r" + "Creating groups: " + (i + 1) + " / " + (startKeys.Length - setCount) + " | " + (j + 1) + " / " + searchArrays.Length;
				maxSymbol = Math.Max(maxSymbol, print.Length);
				var loop = maxSymbol - print.Length;

				for (int k = 0; k < loop; ++k) {
					print += " ";
				}
				Console.Write(print);

				if (potentialResult.Count < setCount - 1) {
					continue;
				}

				var resultCombinations = CreateCombinations(potentialResult.ToArray(), setCount - 1);
				foreach (var combination in resultCombinations) {
					var set = combination.ToHashSet();
					set.Add(startKeys[i]);
					results.Add([set, searchArrays[j].ToHashSet()]);
				}
			}
		}
		Console.WriteLine();

		var uniqueResults = new List<List<HashSet<string>[]>>();
		var potentialUnique = new List<HashSet<string>[]>();
		var usedSyllables = new HashSet<string>();
		var usedWords = new HashSet<string>();

		if (results.Count < uniqueCount) {
			Console.Write("Selecting Uniques: 0 / 0 | 0 / 0");
		}

		for (int i = 0; i < results.Count - uniqueCount + 1; ++i) {
			potentialUnique.Add(results[i]);
			foreach (var start in results[i][0]) {
				usedSyllables.Add(start);
				foreach (var end in results[i][1]) {
					usedSyllables.Add(end);
					usedWords.Add(start + end);
				}
			}
			for (int j = i + 1; j < results.Count - uniqueCount + 1; ++j) {
				var containsSame = false;

				foreach (var start in results[j][0]) {
					if (usedSyllables.Contains(start)) {
						containsSame = true; 
						break;
					}
					foreach (var end in results[j][1]) {
						if (usedSyllables.Contains(end)) {
							containsSame = true;
							break;
						}
						if (usedWords.Contains(start + end)) {
							containsSame = true;
							break;
						}
					}
					if (containsSame) break;
				}

				print = "\r" + "Selecting Uniques: " + (i + 1) + " / " + (results.Count - uniqueCount) + " | " + (j - i + 1) + " / " + (results.Count - uniqueCount + 1 - i);
				maxSymbol = Math.Max(maxSymbol, print.Length);
				var loop = maxSymbol - print.Length;

				for (int k = 0; k < loop; ++k) {
					print += " ";
				}

				Console.Write(print);

				if (containsSame) continue;
				potentialUnique.Add(results[j]);

				foreach (var start in results[j][0]) {
					usedSyllables.Add(start);
					foreach (var end in results[j][1]) {
						usedSyllables.Add(end);
						usedWords.Add(start + end);
					}
				}
			}

			if (potentialUnique.Count >= uniqueCount) {
				var subArray = new HashSet<string>[potentialUnique.Count - 1][];
				Array.Copy(potentialUnique.ToArray(), 1, subArray, 0, subArray.Length);
				var uniqueCombinations = CreateCombinations(subArray, uniqueCount - 1);
				foreach (var combination in uniqueCombinations) {
					var list = combination.ToList();
					list.Add(results[i]);
					uniqueResults.Add(list);
				}
			}
			potentialUnique.Clear();
			usedSyllables.Clear();
			usedWords.Clear();
		}

		return uniqueResults;
	}


	public static T[][] CreateCombinations<T>(T[] array, int combinationSize) {
		if (combinationSize > array.Length) {
			return [];
		}
		if (combinationSize == array.Length) {
			return [array];
		}

		var resultList = new List<T[]>();
		var data = new T[combinationSize];

		return CombinationRecursion(0, array.Length - 1, 0).ToArray();

		List<T[]> CombinationRecursion(int start, int end, int index) {
			if (index == combinationSize) {
				resultList.Add([.. data]);
				return resultList;
			}

			for (int i = start; i <= end && end - i + 1 >= combinationSize - index; ++i) {
				data[index] = array[i];
				resultList = CombinationRecursion(i + 1, end, index + 1);
			}

			return resultList;
		}
	}

}
