using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace iad_test
{
    class Program
    {
        static void Main()
        {
            DebugMode dm = new DebugMode(@"log.txt");
            dm.stopwatch.Start();
            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------// 
            //------------------------------------------------------ DATA INPUT AND SERIALIZATION ----------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//

            List<string> requiredPlaces = new List<string> { "usa", "west-germany", "france", "uk", "canada", "japan" };
            List<Text> allTexts = new List<Text>();

            DirectoryInfo directory = new DirectoryInfo(@"C:\Users\Lenovo\Documents\Studia\SEMESTR VII\Inteligentna analiza danych\22_10_20\text");
            FileInfo[] files = directory.GetFiles("*.sgm");

            foreach (FileInfo file in files)
            {
                { 
                Console.WriteLine("Processing " + file.FullName + "...");
                const int bufferSize = 128;
                    using (var fileStream = File.OpenRead(file.FullName))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize))
                    {
                        string originalTextFile = "";
                        string line;
                        bool firstSkipped = false;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (!firstSkipped)
                            {
                                firstSkipped = true;
                                continue;
                            }
                            originalTextFile += line;
                        }
                        string originalFileTextWithoutHexadecimalSymbols = ReplaceHexadecimalSymbols(originalTextFile);

                        var rootedXml = "<root>" + originalFileTextWithoutHexadecimalSymbols + "</root>";

                        XmlSerializer serializer = new XmlSerializer(typeof(root));
                        root result;
                        using (TextReader reader = new StringReader(rootedXml))
                        {
                            result = (root)serializer.Deserialize(reader);
                        }

                        foreach (rootREUTERS rootREUTER in result.REUTERS)
                        {
                            if (rootREUTER.PLACES.Length != 1) continue;
                            else if (!requiredPlaces.Contains(rootREUTER.PLACES[0])) continue;
                            else
                            {
                                if (rootREUTER.TEXT.BODY != null) allTexts.Add(new Text(rootREUTER.PLACES[0], rootREUTER.TEXT.BODY));
                            }
                        }
                    }
                }
            }



            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------// 
            //------------------------------------------------------ BODY SPLITTING AND STEMMING -----------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//

            foreach (Text text in allTexts)
            {
                text.StemSplitBody();
            }

            Console.WriteLine();
            dm.WriteLogLine("Data split and stemmed", true);


            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //------------------------------------------------------ FEATURE WEIGHTING IMPLEMENTATION ------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//


            //----------------------ADDING EVERY WORD TO LIST----------------------//
            List<string> allWordList = new List<string>();
            Dictionary<string, int> allWordsDictionary = new Dictionary<string, int>();
            foreach (Text text in allTexts)
            {
                foreach (string word in text.splitBody)
                {
                    if (!allWordsDictionary.ContainsKey(word))
                    {
                        allWordsDictionary.Add(word, 1);
                    }
                    else
                    {
                        allWordsDictionary[word]++;
                    }
                }
            }


            //----------------------ADDING EVERY WORD WHICH OCCURED MORE THAN TEN TIMES IN ALL TEXTS----------------------//
            foreach (KeyValuePair<string, int> kvp in allWordsDictionary)
            {
                if (kvp.Value > 10) allWordList.Add(kvp.Key);
            }


            //------------------ADDING EVERY PREVIOUS SELECT WORD WITH ITS OCCURANCY IN EVERY EACH TEXT-------------------//
            foreach (Text text in allTexts)
            {
                foreach (string word in allWordList)
                {
                    if (text.splitBody.Contains(word))
                    {
                        int counter = 0;
                        for (int i = 0; i < text.splitBody.Length; i++)
                        {
                            if (text.splitBody[i] == word) counter++;
                        }
                        //text.wordOccurancy.Add(word, counter);
                        text.wordOccurancyInt.Add(counter);
                        double tfidf = counter * (Math.Log(allTexts.Count() / allWordsDictionary[word]));
                        text.wordTFIDFDouble.Add(counter);
                    }
                    else
                    {
                        //text.wordOccurancy.Add(word, 0);
                        text.wordOccurancyInt.Add(0);
                        text.wordTFIDFDouble.Add(0);
                    }
                }
            }


            dm.WriteLogLine("Word frequency per text calculated", true);

            //---------------------------------------TF - IDF WEIGHTING---------------------------------------//
            /*
            foreach (Text text in allTexts)
            {
                foreach (KeyValuePair<string, int> wordOccurancyKvp in text.wordOccurancy)
                {
                    if (wordOccurancyKvp.Value != 0)
                    {
                        double tfidfWeight = wordOccurancyKvp.Value * (Math.Log(allTexts.Count() / allWordsDictionary[wordOccurancyKvp.Key]));
                        text.wordTFIDFDouble.Add(tfidfWeight);
                    }
                    else
                    {
                        text.wordTFIDFDouble.Add(0);
                    }
                }
            }
            */
            dm.WriteLogLine("Word frequency per text normalized", true);



            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //------------------------------------------------------ K NEAREST NEIGHBOURS ALGORITHM --------------------------------------------------------------//
            //--------------------------------------------------------------- CALCULATIONS -----------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            //----------------------------------------------------------------------------------------------------------------------------------------------------//
            Console.WriteLine();

            KNearestNeighbours(5, allTexts, 5, requiredPlaces, 1);
            dm.WriteLogLine(" ", true);
            KNearestNeighbours(5, allTexts, 5, requiredPlaces, 2);
            dm.WriteLogLine(" ", true);
            KNearestNeighbours(5, allTexts, 5, requiredPlaces, 3);
            dm.WriteLogLine(" ", true);
            KNearestNeighbours(5, allTexts, 5, requiredPlaces, 4);
            dm.WriteLogLine(" ", true);

            dm.stopwatch.Stop();
            dm.streamWriter.Close();
            Console.ReadLine();
        }








        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //---------------------------------------------------- REMOVING HEXADECIMAL SYMBOLS METHOD -----------------------------------------------------------//
        //------------------------------------------------------------- IMPLEMENTATION -----------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//

        static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }








        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //------------------------------------------------------ K NEAREST NEIGHBOURS ALGORITHM --------------------------------------------------------------//
        //-------------------------------------------------------------- IMPLEMENTATION ----------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//

        static void KNearestNeighbours(int kNumber, List<Text> texts, int testCasesPropotion, List<string> allPlaces, int distanceMethod)
        {
            //----------------------PICKING LEARNING DATASET AND TEACHING DATASET----------------------//
            List<Text> testCases = new List<Text>();
            List<Text> learnFromCases = new List<Text>();
            int count = 0;
            int deleteUsa = 0;
            foreach (Text text in texts)
            {
                if (text.place == "usa") deleteUsa++;

                if ((deleteUsa == 10) || (!text.place.Equals("usa")))
                {
                    if (count <= testCasesPropotion)
                    {
                        learnFromCases.Add(text);
                        count++;
                    }
                    else if (count > testCasesPropotion && count < 10)
                    {
                        testCases.Add(text);
                        count++;
                    }
                    else
                    {
                        testCases.Add(text);
                        count = 0;
                    }
                    deleteUsa = 0;
                }
            }


            //--------------------------------------------------------------------------//
            //----------------------K NEAREST NEIGHBOURS ALGORITHM----------------------//
            //--------------------------------------------------------------------------//

            //----------------------CALCULATING DISTANCES BETWEEN TEXTS BASED ON FEATURES AND SORTING THEM----------------------//
            foreach (Text testText in testCases)
            {
                //----------------------DISTANCE CALCULATING DISTANCE CHOOSING----------------------//
                List<NeighbourDistance> neighbourDistances = new List<NeighbourDistance>();
                switch (distanceMethod) 
                {
                    case 1:
                        //-----------------------EUCLIDEAN DISTANCE-----------------------//

                        foreach (Text learnFromText in learnFromCases)
                        {
                            neighbourDistances.Add(new NeighbourDistance(EuclideanDistance(testText, learnFromText, 1), learnFromText.place));
                        }
                        neighbourDistances.Sort((x, y) => x.distance.CompareTo(y.distance));
                        break;
                  
                    case 2:
                        //-----------------------MANHATTAN DISTANCE-----------------------//

                        foreach (Text learnFromText in learnFromCases)
                        {
                            int buffor = ManhattanDistance(testText, learnFromText);
                            neighbourDistances.Add(new NeighbourDistance((double)buffor, learnFromText.place));
                        }
                        neighbourDistances.Sort((x, y) => x.distance.CompareTo(y.distance));
                        break;

                    case 3:
                        //-----------------------CHEBYSHEV DISTANCE-----------------------//

                        foreach (Text learnFromText in learnFromCases)
                        {
                            int buffor = ChebyshevDistance(testText, learnFromText);
                            neighbourDistances.Add(new NeighbourDistance((double)buffor, learnFromText.place));
                        }
                        neighbourDistances.Sort((x, y) => x.distance.CompareTo(y.distance));
                        break;

                    case 4:
                        //-----------------------EUCLIDEAN DISTANCE WITH TFIDF-----------------------//

                        foreach (Text learnFromText in learnFromCases)
                        {
                            neighbourDistances.Add(new NeighbourDistance(EuclideanDistance(testText, learnFromText, 2), learnFromText.place));
                        }
                        neighbourDistances.Sort((x, y) => x.distance.CompareTo(y.distance));
                        break;
                }


                //----------------------CALCULATING PLACES OCCURANCY FOR K NEAREST NEIGHBOURS----------------------//
                Dictionary<string, int> placesOccurancy = new Dictionary<string, int>();
                foreach (string place in allPlaces)
                { 
                    placesOccurancy.Add(place, 0);
                }

                for (int i = 0; i < kNumber; i++)
                {
                    foreach (KeyValuePair<string, int> kvp in placesOccurancy.ToList())
                    {
                        if (neighbourDistances[i].place == kvp.Key) placesOccurancy[kvp.Key]++;
                    }
                }


                //-------------------------------ASSIGNING MOST OCCURANT PLACE TAG TO LEARNING TEXT TAG-------------------------------//
                
                int maxValue = 0;
                foreach (KeyValuePair<string, int> kvp in placesOccurancy)
                {
                    if (kvp.Value > maxValue) maxValue = kvp.Value;
                }

                foreach (KeyValuePair<string, int> kvp in placesOccurancy)
                {
                    if (kvp.Value.Equals(maxValue)) testText.assignedPlace = kvp.Key;
                }
            }


            //-------------------------------CALCULATING ACCURACY, PRECISION AND RECALL-------------------------------//
            Console.WriteLine();
            Console.WriteLine("--------------------------------");

            if (distanceMethod == 1)
            {
                Console.WriteLine("KNN using Euclidean distnace:");
                Console.WriteLine();
            }
            if (distanceMethod == 2)
            {
                Console.WriteLine("KNN using Manhattan distnace:");
                Console.WriteLine();
            }
            if (distanceMethod == 3)
            {
                Console.WriteLine("KNN using Chebyshev distnace:");
                Console.WriteLine();

            }
            if (distanceMethod == 4)
            {
                Console.WriteLine("KNN using Euclidean distnace and TF-IDF weighting:");
                Console.WriteLine();

            }
            Accuracy(testCases);
            PrecisionAndRecall(testCases, allPlaces);
            
            Console.WriteLine("--------------------------------");
            Console.WriteLine();
        }









        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //------------------------------------------------------ DISTANCES CALCULATION METHODS ---------------------------------------------------------------//
        //-------------------------------------------------------------- IMPLEMENTATION ----------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//

        //----------------------------------------------------------------//
        //-----------------------EUCLIDEAN DISTANCE-----------------------//
        //----------------------------------------------------------------//
        static double EuclideanDistance(Text verifying, Text testing, int wieghtPicker)
        {
            double result = 0;

            if (wieghtPicker == 1)
            {
                for (int i = 0; i < verifying.wordOccurancyInt.Count(); i++)
                {
                    result += Math.Pow((verifying.wordOccurancyInt[i] - testing.wordOccurancyInt[i]), 2);
                }
            }
            else
            {
                for (int i = 0; i < verifying.wordOccurancyInt.Count(); i++)
                {
                    result += Math.Pow((verifying.wordTFIDFDouble[i] - testing.wordTFIDFDouble[i]), 2);
                }
            }

            return Math.Sqrt(result);
        }

        //----------------------------------------------------------------//
        //-----------------------MANHATTAN DISTANCE-----------------------//
        //----------------------------------------------------------------//
        static int ManhattanDistance(Text verifying, Text testing)
        {
            int result = 0;

            for (int i = 0; i < verifying.wordOccurancyInt.Count(); i++)
            {
                result += Math.Abs(verifying.wordOccurancyInt[i] - testing.wordOccurancyInt[i]);
            }

            return result;
        }

        //----------------------------------------------------------------//
        //-----------------------CHEBYSHEV DISTANCE-----------------------//
        //----------------------------------------------------------------//
        static int ChebyshevDistance(Text verifying, Text testing)
        {
            int result = 0;

            for (int i = 0; i < verifying.wordOccurancyInt.Count(); i++)
            {
                int distance = 0;
                distance = Math.Abs(verifying.wordOccurancyInt[i] - testing.wordOccurancyInt[i]);
                if (result < distance) result = distance;
            }

            return result;
        }








        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------------- ACCURACY, PRECISION, RECALL ---------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------------------------------------//

        //----------------------------------------------------------------//
        //-------------------------ACCURACY METHOD------------------------//
        //----------------------------------------------------------------//
        static void Accuracy (List<Text> textList)
        {
            //-------------------------CALCULATING ACCURACY-------------------------//
            int numerator = 0;
            int denominator = textList.Count();

            foreach(Text text in textList)
            {
                if (text.place.Equals(text.assignedPlace)) numerator++;
            }

            double result = (double) numerator / denominator;

            //-------------------------RESULTS PRESENTATION-------------------------//
            Console.WriteLine("Accuracy: {0}", result);
        }


        //-------------------------------------------------------------------------//
        //-----------------------PRECISION AND RECALL METHOD-----------------------//
        //-------------------------------------------------------------------------//
        static void PrecisionAndRecall (List<Text> textList, List<string> allPlaces)
        {
            double precision = 0;
            double recall = 0;

            //-------------------------CONFUSION MATRIX PLOTTING-------------------------//
            List<ConfusionMatrixRow> confusionMatrix = new List<ConfusionMatrixRow>();
            
            foreach (string place in allPlaces)
            {
                confusionMatrix.Add(new ConfusionMatrixRow(place));
            }
            
            foreach (Text text in textList)
            {
                foreach (ConfusionMatrixRow confusionMatrixRow in confusionMatrix)
                {
                    if (text.place.Equals(confusionMatrixRow.label))
                    {
                        if (text.place.Equals(text.assignedPlace))
                        {
                            confusionMatrixRow.columns[text.place]++;
                        }
                        else
                        {
                            if (text.assignedPlace.Equals("usa"))
                            {
                                confusionMatrixRow.columns["usa"]++;
                            }
                            if (text.assignedPlace.Equals("west-germany"))
                            {
                                confusionMatrixRow.columns["west-germany"]++;
                            }
                            if (text.assignedPlace.Equals("france"))
                            {
                                confusionMatrixRow.columns["france"]++;
                            }
                            if (text.assignedPlace.Equals("uk"))
                            {
                                confusionMatrixRow.columns["uk"]++;
                            }
                            if (text.assignedPlace.Equals("canada"))
                            {
                                confusionMatrixRow.columns["canada"]++;
                            }
                            if (text.assignedPlace.Equals("japan"))
                            {
                                confusionMatrixRow.columns["japan"]++;
                            }
                        }
                    }
                }
            }
            
            //-------------------------CALCULATING RECALL-------------------------//
            foreach (ConfusionMatrixRow confusionMatrixRow in confusionMatrix)
            {
                confusionMatrixRow.truePostive = confusionMatrixRow.columns[confusionMatrixRow.label];

                foreach (KeyValuePair<string, int> column in confusionMatrixRow.columns)
                {
                    confusionMatrixRow.falseNegative += column.Value;
                }
                recall += (double)confusionMatrixRow.truePostive / (double)confusionMatrixRow.falseNegative;
            }
            
            recall = recall / confusionMatrix.Count();

            //-------------------------CALCULATING PRECISION-------------------------//
            int falsePositiveUsa = 0;
            int falsePositiveWestGermany = 0;
            int falsePositiveFrance = 0;
            int falsePositiveUk = 0;
            int falsePositiveCanada = 0;
            int falsePositiveJapan = 0;

            foreach (ConfusionMatrixRow confusionMatrixRow in confusionMatrix)
            {
                foreach (KeyValuePair<string, int> column in confusionMatrixRow.columns)
                {
                    if (column.Key.Equals("usa")) falsePositiveUsa += column.Value;
                    if (column.Key.Equals("west-germany")) falsePositiveWestGermany += column.Value;
                    if (column.Key.Equals("france")) falsePositiveFrance += column.Value;
                    if (column.Key.Equals("uk")) falsePositiveUk += column.Value;
                    if (column.Key.Equals("canada")) falsePositiveCanada += column.Value;
                    if (column.Key.Equals("japan")) falsePositiveJapan += column.Value;
                }
            }

            foreach (ConfusionMatrixRow confusionMatrixRow in confusionMatrix)
            {
                if (confusionMatrixRow.label.Equals("usa") && !falsePositiveUsa.Equals(0))
                    confusionMatrixRow.classPrecision = (double)confusionMatrixRow.truePostive / falsePositiveUsa;

                if (confusionMatrixRow.label.Equals("west-germany") && !falsePositiveWestGermany.Equals(0))
                    confusionMatrixRow.classPrecision = (double)confusionMatrixRow.truePostive / falsePositiveWestGermany;

                if (confusionMatrixRow.label.Equals("france") && !falsePositiveFrance.Equals(0))
                    confusionMatrixRow.classPrecision = (double)confusionMatrixRow.truePostive / falsePositiveFrance;

                if (confusionMatrixRow.label.Equals("uk") && !falsePositiveUk.Equals(0))
                    confusionMatrixRow.classPrecision = (double)confusionMatrixRow.truePostive / falsePositiveUk;

                if (confusionMatrixRow.label.Equals("canada") && !falsePositiveCanada.Equals(0))
                    confusionMatrixRow.classPrecision = (double)confusionMatrixRow.truePostive / falsePositiveCanada;

                if (confusionMatrixRow.label.Equals("japan") && !falsePositiveJapan.Equals(0))
                    confusionMatrixRow.classPrecision = (double)confusionMatrixRow.truePostive / falsePositiveJapan;
            }

            foreach (ConfusionMatrixRow confusionMatrixRow in confusionMatrix)
            {
                precision += confusionMatrixRow.classPrecision;
            }
            
            precision = precision / confusionMatrix.Count();

            //-------------------------RESULTS PRESENTATION-------------------------//
            Console.WriteLine("Precision: {0}", precision);
            Console.WriteLine("Recall: {0}", recall);
        }
    }
}
