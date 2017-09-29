using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LenaUbi {
    class LenaUbi {
        private String ubilenaFileName;
        private String outputFileSuffix = "_OUTPUT_300SEC.CSV";
        private int timeIntervalSecs;
        private double metersToBeClose = 1.5;
        private int childIdFileColumn = 0;
        private int xColumn = 7;
        private int yColumn = 8;
        private int vocColumn = 33;
        private int timeColumn = 1;
        private String mappingFile = "C://Users/leibo/Documents/UM_REU/LENAUBI/lenaubi_data/data/MAPPINGNEW.CSV";
        private int mappingIdColumn = 3;
        private int mappingUbiIdColumn = 4;
        public Dictionary<String, double> pairStats = new Dictionary<String, double>();
        public Dictionary<String, int> personPos = new Dictionary<String, int>();
        public Dictionary<String, double> persons = new Dictionary<String, double>();

        /// <summary>
        /// Constructs a LenaUbi object.
        /// </summary>
        /// <param name="filepath"> path of the CSV file to process </param>
        /// <param name="interval"> time interval for processes in seconds </param>
        public LenaUbi(string filepath, int interval) {
            ubilenaFileName = filepath;
            timeIntervalSecs = interval;
        }

        public List<String> makePairs(List<String> list, List<String> newList) {
        // Takes in a list of strings 'list' and an empty list 'newList' and returns a
        // list of string pairs from the members of 'list'
            if (list.Count == 0) {
                return newList;
            }
            String first = list[0];
            List<String> rest = list.Skip(1).ToList();
            foreach (String str in rest) {
                newList.Add(first + "-" + str);
                if(!pairStats.ContainsKey(first + "-" + str))
                pairStats.Add(first + "-" + str,0);
            }
            return makePairs(rest, newList);
        }
         
        public void createPairsV2() {
            try {
                using (StreamReader sr = new StreamReader(mappingFile)) {
                    sr.ReadLine();
                    List<String> personsBefore = new List<string>();
                    while (!sr.EndOfStream) {
                        String[] line = sr.ReadLine().Split(',');
                        if (line.Length > 8 && line[mappingIdColumn].Trim() != "") {
                            String personId = line[mappingIdColumn].Trim();
                            if(!persons.ContainsKey(personId))
                            persons.Add(personId,0);
                            //String personUbiId = line[mappingUbiIdColumn];
                            //personLenaId.Add(personUbiId,personId);
                            //personLenaId
                            foreach (String person in personsBefore) {
                                String key = person + "-" + personId;
                                pairStats.Add(key, 0);
                            }
                            personsBefore.Add(personId);
                        }
                    }
                }
            }
            catch (Exception ) {
            }
        }
 
        public Boolean areTheyClose(PersonInfo p1, PersonInfo p2) {
            double dX = p1.X - p2.X;
            double dY = p1.Y - p2.Y;
            return (dX * dX + dY * dY <= (metersToBeClose * metersToBeClose));
        }
        public void processTimeInterval(Dictionary<string, List<PersonInfo>> personInformation) {
            foreach (String pId in personInformation.Keys) {
                foreach (PersonInfo tpi in personInformation[pId]) {
                    if (tpi.Interactions > 0) {
                        Dictionary<String, double> talkedToPersons = new Dictionary<string, double>();
                        foreach (String otherId in personInformation.Keys) {
                            if (otherId != pId) {
                                foreach (PersonInfo otherTpi in personInformation[otherId]) {
                                    if (otherTpi.Interactions > 0 && areTheyClose(tpi, otherTpi) && 
                                        (!talkedToPersons.ContainsKey(otherId))) {
                                        String key1 = pId + "-" + otherId;
                                        String key2 = otherId + "-" + pId;
                                        if (pairStats.ContainsKey(key1)) {
                                            pairStats[key1] += tpi.Interactions;
                                        }
                                        else if (pairStats.ContainsKey(key2)) {
                                            pairStats[key2] += tpi.Interactions;
                                        }
                                        else {
                                            String err = "Aaaaa";
                                        }
                                        talkedToPersons.Add(otherId, tpi.Interactions);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void readCSV() {
            using (StreamReader sr = new StreamReader(ubilenaFileName)) {
                if (!sr.EndOfStream)
                    sr.ReadLine();
                Dictionary<String, List<PersonInfo>> personInfo = new Dictionary<string, List<PersonInfo>>();
                Dictionary<DateTime, int> processedTimes = new Dictionary<DateTime, int>();
                DateTime timeCutOff=new DateTime(1700,01, 01);
                while (!sr.EndOfStream) {
                    String[] line = sr.ReadLine().Split(',');
                    if(line.Length>vocColumn && line[vocColumn].Trim()!="") {
                        DateTime lineTime = Convert.ToDateTime(line[timeColumn]);
                        String personId = line[childIdFileColumn];
                        if (timeCutOff.CompareTo(new DateTime(1700, 01, 01)) == 0) {
                            timeCutOff = lineTime.AddSeconds(timeIntervalSecs);
                        }
                        else if(lineTime.CompareTo(timeCutOff) >=0) {
                            timeCutOff = timeCutOff.AddSeconds(timeIntervalSecs);                           
                            processTimeInterval(personInfo);
                            personInfo = new Dictionary<string, List<PersonInfo>>();           
                        }
                        else {
                            if (line[xColumn].Trim() != "") {
                                PersonInfo pi = new PersonInfo();
                                pi.X = Convert.ToDouble(line[xColumn]);
                                pi.Y = Convert.ToDouble(line[yColumn]);
                                pi.Interactions = line.Length >= vocColumn ? Convert.ToDouble(line[vocColumn]) : 0;
                                if (!personInfo.ContainsKey(personId)) {
                                    personInfo.Add(personId, new List<PersonInfo>());
                                }
                                personInfo[personId].Add(pi);
                            }
                        }
                    }
                }
            }
        }
        public void writeCSV() {
            String outputFileName = this.ubilenaFileName.Replace(".CSV", "_OUTPUT_"+ this.timeIntervalSecs + "_SECS.CSV");
            using (TextWriter sw = new StreamWriter(outputFileName)) {
                sw.Write(",");
                foreach (String ps in persons.Keys) {
                    sw.Write(ps  + ",");
                }
                foreach (String ops in persons.Keys) {
                    sw.WriteLine("");
                    sw.Write(ops + ",");
                    foreach (String oops in persons.Keys) {
                        if(ops!=oops) {
                            String key1 = ops + "-" + oops;
                            String key2 = oops + "-" + ops;
                            if (pairStats.ContainsKey(key1)) {
                                sw.Write(pairStats[key1] + ",");
                            }
                            else if (pairStats.ContainsKey(key2)) {
                                sw.Write(pairStats[key2] + ",");
                            }
                        }
                        else
                            sw.Write(",");
                    }
                }
            }
        }
        static void Main(string[] args) {
            String path = "C://Users/leibo/Documents/UM_REU/LENAUBI/lenaubi_data/data";
            String file_3_3 = "/SYNCHEDANDFILTERSV4_3_3_2017_3_4_2017.CSV";
            String file_3_10 = "/SYNCHEDANDFILTERSV4_3_10_2017_3_11_2017.CSV";
            String file_3_17 = "/SYNCHEDANDFILTERSV4_3_17_2017_3_18_2017.CSV";
            String file_3_31 = "/SYNCHEDANDFILTERSV4_3_31_2017_4_1_2017.CSV";
            String file_4_7 = "/SYNCHEDANDFILTERSV4_4_7_2017_4_8_2017.CSV";
            String file_4_21 = "/SYNCHEDANDFILTERSV4_4_21_2017_4_22_2017.CSV";
            String file_4_28 = "/SYNCHEDANDFILTERSV4_4_28_2017_4_29_2017.CSV";
            void makeCSVFiles(int interval) {
                List<String> files = new List<string> { file_3_3, file_3_10, file_3_17, file_3_31, file_4_7, file_4_21, file_4_28 };
                List<LenaUbi> objects = new List<LenaUbi>();
                foreach (String file in files) {
                    String filename = path + file;
                    LenaUbi obj = new LenaUbi(filename, interval);
                    objects.Add(obj);
                }
                foreach (LenaUbi lu in objects) {
                    lu.createPairsV2();
                    lu.readCSV();
                    lu.writeCSV();
                }
            }
            makeCSVFiles(30);
            makeCSVFiles(60);
            makeCSVFiles(300);
            //List<String> t = obj.makePairs(obj.getIds(), new List<String>());
        }
        /*
        public List<String> getIds()
        {
            List<String> idList = new List<string>();
            try {
                using (StreamReader sr = new StreamReader(mappingFile)) {
                    sr.ReadLine();
                    List<String> personsBefore = new List<string>();
                    while (!sr.EndOfStream) {
                        String[] line = sr.ReadLine().Split(',');
                        if (line.Length > 8 && line[mappingIdColumn].Trim() != "") {
                            String personId = line[mappingIdColumn];
                            idList.Add(personId);
                        }
                    }
                }
            }
            catch (Exception) {
            }
            return idList;
        }*/
    }
}
