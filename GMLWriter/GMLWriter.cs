
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GMLWriter
{
    /// <summary>
    /// Writes a GML file using a CSV. CSV must be in the following format:
    ///    | ID | ID | ID | ... | SUM | etc |
    /// ID |    | ## | ## | ... | ### | ... |
    /// ID | ## |    | ## | ... | ### | ... |
    /// ID | ## | ## |    | ... | ### | ... |
    /// ID | ## | ## | ## | ... | ### | ... |
    /// 
    /// </summary>
    class GMLWriter {
        private String filename;
        private String[] ids;
        private double average;
        private List<String> edges = new List<string>();
        private List<String> nodes = new List<string>();
        static Dictionary<String, String> colorMap = new Dictionary<string, string>();

        public GMLWriter(String file) {
            filename = file;
        }

        private static String writeNode(int root_index, int id, double x, double y, double h, double w, String fill, 
                                        String outline, String outline_width, String label) {
            return "node\n\t[\n\t\troot_index\t" + root_index + "\n\t\tid\t" + id
                             + "\n\t\tgraphics\n\t\t[\n\t\t\tx\t" + x + "\n\t\t\ty\t" + y
                             + "\n\t\t\th\t" + h + "\n\t\t\tw\t" + w + "\n\t\t\tfill\t\""
                             + fill + "\"\n\t\t\ttype\t\"ellipse\"\n\t\t\toutline\t\"" + outline
                             + "\"\n\t\t\toutline_width\t" + outline_width + "\n\t\t]\n\t\tlabel\t\""
                             + label + "\"\n\t]";
        }
        private static String writeEdge(int source, int target, double width) {
            return "edge\n\t[\n\t\tsource\t" + source + "\n\t\ttarget\t" + target 
                + "\n\t\tlabel\t\" \"\n\t\tgraphics [\n\t\t\twidth\t" + width + "\n\t\t]\n\t]";
        }
        public static void readMapping(String filelocation) {
            int IDColumn = 3;
            int HearingColumn = 14;
            String typical = "#e41a1c"; //red
            String atypical = "#377eb8"; //blue
            using (StreamReader sr = new StreamReader(filelocation)) {
                if(!sr.EndOfStream) {
                    sr.ReadLine();
                }
                while(!sr.EndOfStream) {
                    String[] line = sr.ReadLine().Split(',');
                    if (!colorMap.ContainsKey(line[IDColumn])) {
                        if (line[HearingColumn].Equals("")) 
                            colorMap.Add(line[IDColumn], typical);
                        else
                            colorMap.Add(line[IDColumn], atypical);
                    }
                }
            }
        }
        public void findAverage(Boolean includeTeachers) {
            double triangularNumber(int num) {
                if(num == 0) {
                    return 0;
                }
                return num + triangularNumber(num - 1);
            }
            using (StreamReader sr = new StreamReader(filename)) {
                double sum = 0.0;
                int numPeople = 0;
                if (!sr.EndOfStream) {
                    String[] line = sr.ReadLine().Split(',');
                    for (int i = 0; i < line.Length; i++) {
                        if (includeTeachers) {
                            if (!line[i].Contains("SUM") && !line[i].Equals("")) {
                                numPeople++;
                            }
                        }
                        else {
                            if (!line[i].Contains("T") && !line[i].Contains("Lab") && !line[i].Contains("SUM") 
                                && !line[i].Equals("")) {
                                numPeople++;
                            }
                        }
                    }
                }
                int rowNum = 1;
                while (!sr.EndOfStream) {
                    String[] line = sr.ReadLine().Split(',');
                    if (includeTeachers) {
                        sum += Convert.ToDouble(line[line.Length - 2]);
                    }
                    else {
                        if (rowNum <= numPeople) {
                            sum += Convert.ToDouble(line[line.Length - 1]);
                        }
                        rowNum++;
                    }
                }
                average = sum / (triangularNumber(numPeople - 1) * 2);
            }  
        }

        /// <summary>
        /// Reads in a CSV file to process it into a GML
        /// </summary>
        /// <param name="includeTeachers"> parameter for whether or not to include teachers </param>
        public void readCSV(Boolean includeTeachers) {
            String outline = "#000000";
            String outline_width = "2.0";
            String fill = "";
            double xVal = 0;
            double yVal = 0;
            using (StreamReader sr = new StreamReader(filename)) {
                if (!sr.EndOfStream) {
                    String[] line = sr.ReadLine().Split(',');
                    //assume last two columns are reserved for sums and not IDs
                    int count = 0;
                    for (int i = 0; i < line.Length; i++) {
                        if (includeTeachers)
                        {
                            if (!line[i].Contains("SUM") && !line[i].Equals("")) {
                                count++;
                            }
                        }
                        else {
                            if (!line[i].Contains("T") && !line[i].Contains("Lab") && !line[i].Contains("SUM") && !line[i].Equals(""))
                            {
                                count++;
                            }
                        }
                    }
                    ids = new String[count];
                    for (int i = 0; i < ids.Count(); i++) {
                        ids[i] = line[i + 1];
                    }
                }
                int rowCount = 1;
                while (!sr.EndOfStream) {
                    String[] line = sr.ReadLine().Split(',');
                    for (int i = rowCount + 1; i <= ids.Count(); i++) {
                        if (Convert.ToDouble(line[i]) >= average) {
                            if (!includeTeachers) {
                                if (rowCount <= ids.Count()) {
                                    String e = GMLWriter.writeEdge(-rowCount, -i, (Convert.ToDouble(line[i]) * 200));
                                    edges.Add(e);
                                }
                            }
                            else {
                                String e = GMLWriter.writeEdge(-rowCount, -i, (Convert.ToDouble(line[i]) * 200));
                                edges.Add(e);
                            }
                        }
                    }
                    if (rowCount <= ids.Length) {
                        double nodeSize = 0.0;
                        nodeSize = includeTeachers ? Convert.ToDouble(line[line.Length - 2]) : Convert.ToDouble(line[line.Length - 1]);
                        nodeSize *= 300;
                        fill = colorMap[ids[rowCount - 1]];
                        String n = GMLWriter.writeNode(-rowCount, -rowCount, xVal, yVal, nodeSize, nodeSize, fill, outline, 
                                                       outline_width, ids[rowCount - 1]);
                        nodes.Add(n);
                    }
                    rowCount++;
                }
            }
        }
        public void writeGML() {
            String header = "Creator \"Y\"\nVersion 1.0\ngraph\n[";
            using (TextWriter sw = new StreamWriter(filename.Replace(".CSV", ".gml"))) {
                sw.WriteLine(header);
                foreach (String node in nodes) {
                    sw.WriteLine("\t" + node);
                }
                foreach (String edge in edges) {
                    sw.WriteLine("\t" + edge);
                }
                sw.WriteLine("]");
            }
        }

        /// <summary>
        /// takes in a List of files and produces one GML with aggregate values of node and edge size
        /// </summary>
        public static void sumGML(String outputname, List<String> files, Boolean penalizeAbsent, Boolean pruneEdges) {
            Dictionary<String, Node> nodeDict = new Dictionary<string, Node>();
            double edgesum = 0.0;
            int edgecount = 0;
            foreach (String file in files) {
                List<String> rawNodes = new List<String>();
                List<String> rawEdges = new List<String>();
                using (StreamReader sr = new StreamReader(file)) {
                    string[] splitNode = new string[] { "node" };
                    string[] splitEdge = new string[] { "edge" };
                    String[] line = sr.ReadToEnd().Split(splitNode, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < line.Length - 1; i++) {
                        rawNodes.Add(line[i]);
                    }
                    String rest = line[line.Length - 1];
                    String[] line2 = rest.Split(splitEdge, StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine(line2[0]);
                    rawNodes.Add(line2[0]);
                    for(int i = 1; i< line2.Length; i++) {
                        rawEdges.Add(line2[i]);
                    }
                }
                foreach (String nodeString in rawNodes) {
                    int id_index_start = nodeString.IndexOf("id");
                    int id_index_end = nodeString.IndexOf("graphics");
                    int size_index_start = nodeString.IndexOf("w");
                    int fill_index_start = nodeString.IndexOf("fill");
                    int fill_index_end = nodeString.IndexOf("type");
                    int label_index_start = nodeString.IndexOf("label");
                    String id = nodeString.Substring(id_index_start + 2, id_index_end - id_index_start - 2).Trim();
                    String size = nodeString.Substring(size_index_start + 1, fill_index_start - size_index_start - 1).Trim();
                    if(nodeDict.ContainsKey(id)) {
                        nodeDict[id].addSize(Convert.ToDouble(size));
                    }
                    else {
                        String fill = nodeString.Substring(fill_index_start + 4, fill_index_end - fill_index_start - 4).Trim();
                        fill = fill.Substring(1, fill.Length - 2);
                        String label = nodeString.Substring(label_index_start + 5).Trim();
                        label = label.Substring(0, label.Length - 1).Trim();
                        label = label.Substring(1, label.Length - 2);
                        Node node = new Node(id, label, Convert.ToDouble(size), fill);
                        nodeDict.Add(id, node);
                    }
                }
                foreach(String edgeString in rawEdges) {
                    int source_index_start = edgeString.IndexOf("source");
                    int target_index_start = edgeString.IndexOf("target");
                    int target_index_end = edgeString.IndexOf("label");
                    int width_index_start = edgeString.IndexOf("width");
                    String source = edgeString.Substring(source_index_start + 6, target_index_start - source_index_start - 7).Trim();
                    String target = edgeString.Substring(target_index_start + 6, target_index_end - target_index_start - 6).Trim();
                    String width = edgeString.Substring(width_index_start + 5).Trim();
                    width = width.Split(']')[0];
                    width = width.Split(']')[0];
                    nodeDict[source].addEdge(target, Convert.ToDouble(width));
                }
            }
            using (TextWriter sw = new StreamWriter(outputname)) {
                String header = "Creator \"Y\"\nVersion 1.0\ngraph\n[";
                sw.WriteLine(header);
                foreach (String key in nodeDict.Keys) {
                    if (penalizeAbsent) {
                        nodeDict[key].average(files.Count);
                    }
                    else {
                        nodeDict[key].average();
                    }
                    int id = int.Parse(nodeDict[key].getID());
                    String label = nodeDict[key].getLabel();
                    String fill = nodeDict[key].getFill();
                    double size = nodeDict[key].getSize();
                    String nodeprint = GMLWriter.writeNode(id, id, 0, 0, size, size, fill, "#000000", "2.0", label);
                    sw.WriteLine("\t" + nodeprint);
                }
                foreach(String key in nodeDict.Keys) {
                    Dictionary<string, double> edgeDict = nodeDict[key].getEdges();
                    foreach (String s in nodeDict.Keys) {
                        foreach (String edge in nodeDict[s].getEdges().Keys) {
                            edgesum += nodeDict[s].getEdges()[edge];
                            edgecount++;
                        }
                    }
                    double averagewidth = edgesum / edgecount;
                    foreach (String s in edgeDict.Keys {
                        if (pruneEdges) {
                            if (edgeDict[s] >= averagewidth) {
                                String edgeprint = GMLWriter.writeEdge(int.Parse(key), int.Parse(s), edgeDict[s]);
                                sw.WriteLine("\t" + edgeprint);
                            }
                        }
                        else {
                            String edgeprint = GMLWriter.writeEdge(int.Parse(key), int.Parse(s), edgeDict[s]);
                            sw.WriteLine("\t" + edgeprint);
                        }
                    }
                }
                sw.WriteLine("]");
            }
        }

        public static void writeBasicGML(String outputFileName, List<Tuple<int, double>> nodes, List<Tuple<int, int, double>> edges) {
            List<String> nodeStrings = new List<String>();
            List<String> edgeStrings = new List<String>();
            foreach(Tuple<int, double> node in nodes) {
                String color = "#377eb8"; //blue (atypical)
                if (node.Item1 == -4 || node.Item1 == -8 || node.Item1 == -9) {
                    color = "#e41a1c"; //red (typical)
                }
                String nodestring = writeNode(node.Item1, node.Item1, 0, 0, node.Item2, node.Item2, color, "#000000", "2.0",
                                              "B" + -node.Item1);
                nodeStrings.Add(nodestring);
            }
            foreach(Tuple<int,int,double> edge in edges) {
                String edgestring = writeEdge(edge.Item1, edge.Item2, edge.Item3);
                edgeStrings.Add(edgestring);
            }
            using (TextWriter sw = new StreamWriter(outputFileName)) {
                String header = "Creator \"Y\"\nVersion 1.0\ngraph\n[";
                sw.WriteLine(header);
                foreach (String n in nodeStrings) {
                    sw.WriteLine("\t" + n);
                }
                foreach (String e in edgeStrings) {
                    sw.WriteLine("\t" + e);
                }
                sw.WriteLine("]");
            }
        }
        /// <summary>
        /// convert a summary data csv to a gml
        /// </summary>
        /// <param name="filename">location of file</param>
        public static void summaryCSV2GML(String filename) {
            Dictionary<int, Node> nodes = new Dictionary<int, Node>();
            using (StreamReader sr = new StreamReader(filename)) {
                if (!sr.EndOfStream) {
                    sr.ReadLine();
                }
                while (!sr.EndOfStream) {
                    String[] line = sr.ReadLine().Split(',');
                    int subject = Convert.ToInt32(line[1].Substring(0, line[1].Length - 1));
                    String partner = line[2].Substring(0, line[2].Length - 1);
                    double proximity = Convert.ToDouble(line[6]) / Convert.ToDouble(line[8]);
                    if (!nodes.ContainsKey(subject)) {
                        nodes.Add(subject, new Node(subject.ToString(), subject.ToString(), proximity, colorMap[subject + "B"]));
                        nodes[subject].addEdge(partner, proximity);
                    }
                    else {
                        nodes[subject].addSize(proximity);
                        nodes[subject].addEdge(partner, proximity);
                    }
                }
            }
            List<Tuple<int, double>> nodelist = new List<Tuple<int, double>>();
            double edgesum = 0;
            int counter = 0; 
            foreach(int n in nodes.Keys) {
                Dictionary<String, double> edges = nodes[n].getEdges();
                foreach(string e in edges.Keys) {
                    edgesum += edges[e];
                    counter++;
                }
            }
            double edgeavg = edgesum / counter;
            foreach (int n in nodes.Keys) {
                nodelist.Add(new Tuple<int, double>(-n, nodes[n].getSize() * 20));
            }
            List<Tuple<int, int, double>> edgelist = new List<Tuple<int, int, double>>();
            for(int i = 1; i < 11; i++) {
                if (nodes.ContainsKey(i)) {
                    Dictionary<String, double> edges = nodes[i].getEdges();
                    for (int j = i; j < 11; j++) {
                        if (edges.ContainsKey(j.ToString())) {
                            double edgeweight = edges[j.ToString()];
                            if(edgeweight >= edgeavg) {
                                edgelist.Add(new Tuple<int, int, double>(-i, -j, edgeweight * 10));
                            }                            
                        }
                    }
                }
            }
            writeBasicGML(filename.Replace(".CSV", ".gml"),nodelist, edgelist);
        }
        static void Main(string[] args) {
            String path = "C://Users/leibo/Documents/UM_REU/Data Vis/GMLWriter/GMLWriter/data/";
            String mappingfile = "MAPPINGNEW.CSV";
            String file3_3_normalized = "3_3_interactionsv2_normalized.CSV";
            String file3_10_normalized = "3_10_interactionsv2_normalized.CSV";
            String file3_17_normalized = "3_17_interactionsv2_normalized.CSV";
            String file3_31_normalized = "3_31_interactionsv3_normalized.CSV";
            String file4_7_normalized = "4_7_interactionsv3_normalized.CSV";
            String file4_21_normalized = "4_21_interactionsv3_normalized.CSV";
            String file4_28_normalized = "4_28_interactionsv2_normalized.CSV";
            List<String> files = new List<String> { file3_3_normalized,
                file3_10_normalized, file3_17_normalized, file3_31_normalized,
                file4_7_normalized, file4_21_normalized, file4_28_normalized };
            
            foreach (String file in files) {
                GMLWriter gml = new GMLWriter(path + file);
                GMLWriter.readMapping(path + mappingfile);
                gml.findAverage(false);
                gml.readCSV(false);
                gml.writeGML();
            }
            
            //replace with files to use
            String gml3_3 = "cotalk/3_3_cotalk.gml";
            String gml3_10 = "cotalk/3_10_cotalk.gml";
            String gml3_17 = "cotalk/3_17_cotalk.gml";
            String gml3_31 = "cotalk/3_31_cotalk.gml";
            String gml4_7 = "cotalk/4_7_cotalk.gml";
            String gml4_21 = "cotalk/4_21_cotalk.gml";
            String gml4_28 = "cotalk/4_28_cotalk.gml";
            List<String> gmls = new List<String>() { path + gml3_3, path + gml3_10, path + gml3_17, path + gml3_31, path + gml4_7, path + gml4_21, path + gml4_28 };
            GMLWriter.sumGML("cotalk/cotalk_pruned_aggregate.gml", gmls, false, true);
            
            //use the below to manually create edge and node lists
            //takes in 10 doubles in order representing the sizes of B1 to B10 and returns a list of those nodes.
            List<Tuple<int, double>> makeNodeList(List<double> nodewidths) {
                List<Tuple<int, double>> nodelist = new List<Tuple<int, double>>();
                for (int i = -1; i >= -nodewidths.Count; i--) {
                    if (!nodewidths[-i - 1].Equals(0)) {
                        nodelist.Add(new Tuple<int, double>(i, nodewidths[-i - 1]));
                    }
                }
                return nodelist;
            }
            Tuple<int, int, double> edge(int source, int target, double width) {
                return new Tuple<int, int, double>(source, target, width);
            }
            
            GMLWriter.summaryCSV2GML(path + "sortedsync/summary3_3.CSV");
            BidirectionalGraph g = new BidirectionalGraph();
            g.readGML(path + "cotalk/3_3_cotalk.gml");
        }
    }
}
