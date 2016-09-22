using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslationServiceHelperTester.TerminologyService;


namespace TranslationServiceHelperTester
{
    class Program
    {
        static TerminologyClient tc;
        static TranslationSources ts;
        static bool waitForResults = false;

        static void Main(string[] args)
        {
            tc = new TerminologyClient();
            ts = new TranslationSources() { TranslationSource.Terms, TranslationSource.UiStrings };

            SearchStringComparison ssc = (args.Length > 3) && args[3] == "usecase" ? SearchStringComparison.CaseSensitive : SearchStringComparison.CaseInsensitive;

            tc.GetTranslationsCompleted += tc_GetTranslationsCompleted;
            tc.GetLanguagesCompleted += tc_GetLanguagesCompleted;

            if (args.Length < 3)
            {
                tc.GetLanguagesAsync();
                waitForResults = true;
                while (waitForResults)
                {
                    Console.WriteLine("Waiting for languages retrieval results...\n");
                }

                string exeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                Console.WriteLine("\n\n\n\nUsage: " + exeName + " \"phrase\"" + " \"source lang\"" + " \"dest lang\"" + " \"");

                //Console.WriteLine("\n\n\n\nPress any key to exit...");
                //Console.ReadKey();
                return;
            }

            Console.WriteLine("Querying Translation Service...\n");

            tc.GetTranslationsAsync(args[0],
                        args[1],
                        args[2],
                        ssc,
                        SearchOperator.Contains,
                        ts,
                        false,
                        20,
                        true,
                        null);

            waitForResults = true;
            while (waitForResults)
            {
                Console.WriteLine("Waiting for translation results...\n");
            }

            //Console.WriteLine("\n\n\n\nPress any key to exit...\n");
            //Console.ReadKey();
        }

        static void tc_GetLanguagesCompleted(object sender, GetLanguagesCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != null)
            {
                Console.Clear();
                Console.WriteLine("Got Languages Complete!\n");

                Languages ls = e.Result;

                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string[] vals = path.Split('\\');
                path = path.Remove(path.IndexOf(vals[vals.Length - 1]));
                path += "lang.txt";

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
                {
                    foreach (Language l in ls)
                    {
                        file.WriteLine("{0}", l.Code);
                    }
                    waitForResults = false;
                }
                return;
            }
            else
            {
                Console.WriteLine("ERROR: Failed On Translation Service!\n");
                waitForResults = false;
            }
        }

        static void tc_GetTranslationsCompleted(object sender, GetTranslationsCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != null)
            {
                Console.Clear();
                Console.WriteLine("Translation Complete!\n");
                Matches m = e.Result;

                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;


                string[] vals = path.Split('\\');
                path = path.Remove(path.IndexOf(vals[vals.Length - 1]));
                path += "trans.txt";


                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
                {
                    foreach (Match match in m)
                    {
                        file.Write("{0}"+ (char)15 + "{1}" + (char)15 + "{2}", match.OriginalText, match.Product + " " + match.ProductVersion, match.ConfidenceLevel);
                        foreach (Translation t in match.Translations)
                        {
                            file.Write((char)14 + "{0}" + (char)15 +"{1}", t.TranslatedText, t.Language);
                        }
                        file.WriteLine("");
                    }
                }

                waitForResults = false;
                return;
            }
            else
            {
                Console.WriteLine("ERROR: Failed On Translation Service!\n");
                waitForResults = false;
            }
        }
    }
}
