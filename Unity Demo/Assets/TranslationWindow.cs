using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class TranslationWindow : EditorWindow
{
	private Vector2 _scrollPos = Vector2.zero;
	[MenuItem("Translationz/Get Translation", false, 10)]
	static void Init()
	{
		GetWindow(typeof(TranslationWindow), true, "Translationz");
	}

	public static string[] languagesArray;
	public List<string> Phrases;

	int sourceLanguageIndex = -1;
	int lastSourceLanguageIndex = -1;
	int destLanguageIndex = -1;
	int lastDestLanguageIndex = -1;
	string phrase;

	bool retrievingLanguages = false;
	bool languagesPresent = false;
	static bool languagesProcessed = false;

	bool hasPhrases = false;
	bool waitingOnTranslation = false;

	const string transSourceLangIDkey = "translationz_source_lang_index";
	const string transDestLangIDkey = "translationz_destination_lang_index";

	void OnEnable()
	{
		sourceLanguageIndex = EditorPrefs.GetInt(transSourceLangIDkey, 0);
		destLanguageIndex = EditorPrefs.GetInt(transDestLangIDkey, 0);

		languagesPresent = LanguagesFileExists();
		LanguageInitChecks();

		//default settings
		if (sourceLanguageIndex < 0 && destLanguageIndex < 0)
		{
			sourceLanguageIndex = System.Array.IndexOf(languagesArray, "en-us");
			destLanguageIndex = System.Array.IndexOf(languagesArray, "es-es");
			phrase = "seconds ago";
		}
	}

	void OnDisable()
	{
		EditorPrefs.SetInt(transSourceLangIDkey, sourceLanguageIndex);
		EditorPrefs.SetInt(transDestLangIDkey, destLanguageIndex);
	}

	void LanguageInitChecks()
	{
		if (!languagesPresent)
		{
			if (!retrievingLanguages)
			{
				retrievingLanguages = true;
				string exe = Directory.GetCurrentDirectory();
				exe += @"\Assets\Plugins\Translationz\GetTranslation.exe";
				System.Diagnostics.Process external = new System.Diagnostics.Process();
				external.StartInfo.FileName = exe;
				external.StartInfo.Arguments = ""; //argument
				external.StartInfo.UseShellExecute = false;
				external.Start();
			}
			else
			{
				languagesPresent = LanguagesFileExists();
				if (languagesPresent)
					retrievingLanguages = false;
			}
		}
		else if (!languagesProcessed)
		{
			string currentDir = Directory.GetCurrentDirectory();
			currentDir += @"\Assets\Plugins\Translationz\lang.txt";
			StreamReader input = new StreamReader(currentDir);

			List<string> availableLanguages = new List<string>();
			while (!input.EndOfStream)
			{
				string line = input.ReadLine();
				availableLanguages.Add(line);
			}

			languagesArray = availableLanguages.ToArray();
			languagesProcessed = true;

			input.Close();
		}
	}

	void OnGUI()
	{
		if (!languagesPresent || !languagesProcessed || retrievingLanguages)
		{
			GUILayout.BeginArea(new Rect(100, 100, 300, 300), "F");
			GUILayout.Label("Getting Languages...");
			GUILayout.EndArea();
			LanguageInitChecks();
			return;
		}

		EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb);
		EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
		EditorGUILayout.LabelField("Source Language", GUILayout.MaxWidth(160));
		lastSourceLanguageIndex = sourceLanguageIndex;
		sourceLanguageIndex = EditorGUILayout.Popup(sourceLanguageIndex, languagesArray, GUILayout.MaxWidth(100));
		if (lastSourceLanguageIndex != sourceLanguageIndex)
		{
			EditorPrefs.SetInt(transSourceLangIDkey, sourceLanguageIndex);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
		EditorGUILayout.LabelField("Destination Language", GUILayout.MaxWidth(160));
		lastDestLanguageIndex = destLanguageIndex;
		destLanguageIndex = EditorGUILayout.Popup(destLanguageIndex, languagesArray, GUILayout.MaxWidth(100));
		if (lastDestLanguageIndex != destLanguageIndex)
		{
			EditorPrefs.SetInt(transDestLangIDkey, destLanguageIndex);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
		phrase = EditorGUILayout.TextField(new GUIContent("Phrase", "Enter a phrase such as 'seconds ago' or 'Save to disk'"), phrase);
		EditorGUILayout.EndHorizontal();

		if (GUILayout.Button(new GUIContent("Go!", "This will retrieve translations for the phrase."), EditorStyles.toolbarButton, GUILayout.Width(160)))
		{
			hasPhrases = false;

			string currentDir = Directory.GetCurrentDirectory();
			currentDir += @"\Assets\Plugins\Translationz\trans.txt";
			File.Delete(currentDir);

			string argsVal = "\"" + phrase + "\" \"" + languagesArray[sourceLanguageIndex] + "\" \"" + languagesArray[destLanguageIndex] + "\"";
			string exe = Directory.GetCurrentDirectory();
			exe += @"\Assets\Plugins\Translationz\GetTranslation.exe";
			System.Diagnostics.Process external = new System.Diagnostics.Process();
			external.StartInfo.FileName = exe;
			external.StartInfo.Arguments = argsVal; //argument 
			external.StartInfo.UseShellExecute = false;
			external.Start();
			waitingOnTranslation = true;
		}
		EditorGUILayout.EndVertical();

		if (waitingOnTranslation && !hasPhrases && PhrasesFileExists())
		{
			ReadTranslatedPhrase();
		}

		if (hasPhrases)
		{
			EditorGUILayout.LabelField("Phrases:", GUILayout.MaxWidth(160));
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

			string val = "";
			for (int i = 0; i < Phrases.Count; ++i)
			{
				string[] pieces = Phrases[i].Split((char)14);

				string[] sourceAndProduct = pieces[0].Split((char)15);
				string[] translationAndLanguage = pieces[1].Split((char)15);

				val = sourceAndProduct[0] + "\t" + translationAndLanguage[0] + "\t\t\t" + sourceAndProduct[1] + " " + sourceAndProduct[2] + "\n";
				EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
				EditorGUILayout.TextField(sourceAndProduct[0]);
				EditorGUILayout.TextField(translationAndLanguage[0]);
				EditorGUILayout.TextField(sourceAndProduct[1] + " " + sourceAndProduct[2]);

				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
		}
	}

	bool LanguagesFileExists()
	{
		string currentDir = Directory.GetCurrentDirectory();
		currentDir += @"\Assets\Plugins\Translationz\lang.txt";
		return File.Exists(currentDir);
	}

	bool PhrasesFileExists()
	{
		string currentDir = Directory.GetCurrentDirectory();
		currentDir += @"\Assets\Plugins\Translationz\trans.txt";
		return File.Exists(currentDir);
	}

	void ReadTranslatedPhrase()
	{
		waitingOnTranslation = false;
		string currentDir = Directory.GetCurrentDirectory();
		currentDir += @"\Assets\Plugins\Translationz\trans.txt";
		StreamReader input = new StreamReader(currentDir);
		Phrases = new List<string>();

		while (!input.EndOfStream)
		{
			string line = input.ReadLine();
			Phrases.Add(line);
		}

		input.Close();
		hasPhrases = true;
	}
}
