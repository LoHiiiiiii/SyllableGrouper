
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

	public static List<HashSet<string>[]> GroupSyllables(Dictionary<string, HashSet<string>> startDict, int groupSize, int maxCombinations = -1) {

		var startKeys = startDict.Keys.ToArray();
		string[][] searchArrays;
		List<string> potentialResult = new List<string>();
		var results = new List<HashSet<string>[]>();

		if (maxCombinations == 0) {
			return results;
		}

		string print = "";
		int maxSymbol = 0;

		for (int i = 0; i < startKeys.Length - groupSize + 1; ++i) {

			searchArrays = CreateCombinations(startDict[startKeys[i]].ToArray(), groupSize);

			if (maxCombinations > 0 && searchArrays.Length > maxCombinations) {
				var indices = new List<int>();
				var tempResults = new List<string[]>();
				for (int k = 0; k < maxCombinations; ++k) {
					int random = Random.Shared.Next(searchArrays.Length - indices.Count);
					foreach (int r in indices) {
						if (random >= r) {
							random++;
						}
					}
					indices.Add(random);
					tempResults.Add(searchArrays[random]);
				}

				searchArrays = tempResults.ToArray();
			}

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

				print = "\r" + "Creating groups: " + (i + 1) + " / " + (startKeys.Length - groupSize + 1) + " | " + (j + 1) + " / " + searchArrays.Length;
				maxSymbol = Math.Max(maxSymbol, print.Length);
				var loop = maxSymbol - print.Length;

				for (int k = 0; k < loop; ++k) {
					print += " ";
				}
				Console.Write(print);

				if (potentialResult.Count < groupSize - 1) {
					continue;
				}

				var resultCombinations = CreateCombinations(potentialResult.ToArray(), groupSize - 1);

				foreach (var combination in resultCombinations) {
					var set = combination.ToHashSet();
					set.Add(startKeys[i]);
					results.Add([set, searchArrays[j].ToHashSet()]);
				}
			}
		}
		return results;
	}


	public static List<List<HashSet<string>[]>> CreateUniqueSets(List<HashSet<string>[]> groups, int uniqueCount, int minSyllable = -1, bool skip = false) {
		if (minSyllable == 0) {
			Console.Write($"Selecting unique sets: {groups.Count} / {groups.Count}");
			return [];
		}

		skip = skip || uniqueCount <= 1 || groups.Count < uniqueCount;

		var filterSubstrings = minSyllable > 0;


		if (!skip && (groups.Count.ToString().Length - 1) * uniqueCount > 8) {
			Console.Write("Over hundred million combinations, skipping unique sets.");
			Console.WriteLine();
			skip = true;
		}

		var uniqueResults = new List<List<HashSet<string>[]>>();

		if (skip) {
			foreach (var group in groups) {
				uniqueResults.Add([group]);
			}
			Console.Write($"Selecting unique sets: {groups.Count} / {groups.Count}");
			return uniqueResults;
		}


		var potentialUnique = new List<HashSet<string>[]>();
		var usedSyllables = new List<HashSet<string>>();
		var usedWords = new List<HashSet<string>>();
		var usedSubstrings = new List<HashSet<string>>();


		double maxCount = 1;

		for (int i = groups.Count - uniqueCount + 1; i <= groups.Count; ++i) {
			maxCount *= i;
			maxCount /= groups.Count - i + 1;
		}

		maxCount = Math.Round(maxCount);
		var counter = 0;

		UniqueRecursion(0);

		return uniqueResults;

		void UniqueRecursion(int startIndex) {
			bool containsSame;
			for (int i = startIndex; i < groups.Count - uniqueCount + potentialUnique.Count + 1; ++i) {
				containsSame = false;

				if (potentialUnique.Count > 0) {
					foreach (var start in groups[i][0]) {
						var endsFilled = false;
						for (int j = 0; j < usedSyllables.Count; ++j) {
							if (filterSubstrings) {
								if (usedSyllables[j].Contains(start[..minSyllable])) {
									containsSame = true;
									break;
								}
							} else if (usedSyllables[j].Contains(start)) {
								containsSame = true;
								break;
							}

							foreach (var end in groups[i][1]) {
								if (usedWords[j].Contains(start + end)) {
									containsSame = true;
									break;
								}
								if (endsFilled) continue;
								if (filterSubstrings) {
									if (usedSyllables[j].Contains(end[..minSyllable])) {
										containsSame = true;
										break;
									}
								} else if (usedSyllables[j].Contains(end)) {
									containsSame = true;
									break;
								}
							}
							if (containsSame) break;
						}
						endsFilled = true;
						if (containsSame) break;
					}
				}

				if (containsSame) {
					counter += (int)Math.Pow(groups.Count - uniqueCount - i + potentialUnique.Count + 1, uniqueCount - 1 - potentialUnique.Count);
					Console.Write($"\rSelecting unique sets: {counter} / {maxCount}");
					continue;
				}

				usedWords.Add([]);
				usedSyllables.Add([]);
				potentialUnique.Add(groups[i]);
				FillUsed(groups[i], usedSyllables.Last(), usedWords.Last());

				if (potentialUnique.Count == uniqueCount) {
					uniqueResults.Add([.. potentialUnique]);
					counter++;
				} else {
					UniqueRecursion(i + 1);
				}

				potentialUnique.RemoveAt(potentialUnique.Count - 1);
				usedSyllables.RemoveAt(usedSyllables.Count - 1);
				usedWords.RemoveAt(usedWords.Count - 1);

				Console.Write($"\rSelecting unique sets: {counter} / {maxCount}");
			}
		}

		void FillUsed(HashSet<string>[] group, HashSet<string> syllables, HashSet<string> words) {
			var endsFilled = false;
			foreach (var start in group[0]) {

				if (filterSubstrings) {
					syllables.Add(start[..minSyllable]);
				} else {
					syllables.Add(start);
				}

				foreach (var end in group[1]) {
					words.Add(start + end);
					if (endsFilled) continue;
					if (filterSubstrings) {
						syllables.Add(end[..minSyllable]);
					} else {
						syllables.Add(end);
					}
				}
			}
		}
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
