using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using NHazm;
using edu.stanford.nlp.ling;

namespace sentiment_analysis
{
    class Program
    {
        static List<string> tokenize(string text)
        {
            char[] delimiters = new char[] {
              ',', '"', ')', '(', ';', '.', '\n', '\r', '\t',
              '>', '<', ':', '=', '\'', '[', ']',' ','!','#',
              '؛','،','-','$','%','&','\'', '+','/','?','؟',
              '{','}','»','«','1','2','3','4','5','6','7','8',
              '9','0','١','۲', '۳','۴','۵','۶','۷','۸','۹','٠'};

            string[] ALLwords = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>();

            foreach (string word in ALLwords)
            {
                var myregex = new Regex(@"^[\u0600-\u06FF\uFB8A\u067E\u0686\u06AF\u200C\u200F ]+$");
                if (myregex.IsMatch(word))
                {
                    list.Add(word);
                }
            }

            return list;
        }

        static Dictionary<string, double> CalcWordCount(string[] words)
        {
            Dictionary<string, double> wordCount = new Dictionary<string, double>();

            foreach (string word in words)
            {

                if (wordCount.ContainsKey(word))
                    wordCount[word]++;
                else
                    wordCount.Add(word, 1);
            }

            return wordCount;
        }


        static double CalcNumberWord(string word)
        {
            string filePath = @"C:\Users\maryam\Documents\Visual Studio 2015\Projects\sentiment analysis\data\after telegram filtering\24may2018.txt";
            string text = File.ReadAllText(filePath);
            List<string> words = tokenize(text);
            Dictionary<string, double> WordCount = CalcWordCount(words.ToArray());
            double number = 0;
            if (WordCount.ContainsKey(word))
                 number = WordCount[word];
            return number;
        }

        static List<string> FindPhrase()
        {
            string filePath = @"C:\Users\maryam\Documents\Visual Studio 2015\Projects\sentiment analysis\data\after telegram filtering\24may2018.txt";
            string[] comments = File.ReadAllLines(filePath);
            POSTagger tagger = new POSTagger(@"C:\Users\maryam\Documents\Visual Studio 2015\Projects\sentiment analysis\NHazm-master\NHazm\Resources\persian.tagger");
            List<string> Phrases = new List<string>();

            foreach (string comment in comments)
            {
                List<string> st = tokenize(comment);
                List<TaggedWord> stTag = tagger.BatchTag(st);

                for (int j = 0; j < stTag.Count-1; j++)
                {
                    var wordTag = stTag[j];
                    if(wordTag.tag() == "N")
                    {   
                        var nextWordTag = stTag[j + 1];
                        if(nextWordTag.tag() == "N" || nextWordTag.tag() == "ADJ")
                        {
                            string phrase = st[j] + " " + st[j + 1];
                            Phrases.Add(phrase);
                        }
                    }
                }
            }

            return Phrases;
        }

        static Dictionary<string, double> hitPhraseWithWord(List<string> phrases , string word)
        {
            Dictionary<string, double> phraseNear = new Dictionary<string, double>();

            string filePath = @"C:\Users\maryam\Documents\Visual Studio 2015\Projects\sentiment analysis\data\after telegram filtering\24may2018.txt";
            string[] comments = File.ReadAllLines(filePath);

            foreach(string comment in comments)
            {
                foreach(string phrase in phrases)
                {
                    if(comment.Contains(phrase) && comment.Contains(word))
                    {
                        if(phraseNear.ContainsKey(phrase))
                        {
                            phraseNear[phrase] += 1;
                        }
                        else
                        {
                            phraseNear.Add(phrase, 1);
                        }

                    }
                }
            }
            return phraseNear;

        }

        static Dictionary<string, double> calcPhrasesPolarity(string posWord, string negWord)
        {
            double hitPosWord = CalcNumberWord(posWord);
            Console.WriteLine(hitPosWord);

            double hitNegWord = CalcNumberWord(negWord);
            Console.WriteLine(hitNegWord);

            List<string> phrases = FindPhrase();
            Console.WriteLine("phrases Found");

            Dictionary<string, double> hitPhrasesWithPos = hitPhraseWithWord(phrases, posWord);
            Console.WriteLine("phrases near positive found");

            Dictionary<string, double> hitPhrasesWithNeg = hitPhraseWithWord(phrases, negWord);
            Console.WriteLine("phrases near negative found");

            Dictionary<string, double> phrasesPolarity = new Dictionary<string, double>();

            foreach(string phrase in phrases)
            {
                if(hitPhrasesWithPos.ContainsKey(phrase) && hitPhrasesWithNeg.ContainsKey(phrase))
                {
                    double polarity = Math.Log((hitPhrasesWithPos[phrase] * hitNegWord) / (hitPhrasesWithNeg[phrase] * hitPosWord));
                    Console.WriteLine(polarity);
                    if(!phrasesPolarity.ContainsKey(phrase))
                        phrasesPolarity.Add(phrase, polarity);
                }             
            }
            Console.WriteLine("phrases polarity found");
            return phrasesPolarity;

        }

        static double calcPolarityText(string posWord , string negWord)
        {
            double polarity = 0;

            Dictionary<string, double> phrasesPolarity = calcPhrasesPolarity(posWord, negWord);
            foreach(var pair in phrasesPolarity)
            {
                polarity += pair.Value;
            }

            polarity = polarity / phrasesPolarity.Count;

            return polarity;

        }
        
        static void Main(string[] args)
        {
            double polarity = calcPolarityText("خوب", "بد");
            System.Diagnostics.Debug.WriteLine(polarity);
            Console.Write("\npolarity:");
            Console.Write(polarity);
            Console.Write("\nfinish");
            Console.Write("\nPress any key to continue... ");
            Console.ReadLine();

        }
    }
}
