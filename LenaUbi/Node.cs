using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMLWriter {
    class Node {
        private double size;
        private int count = 1;
        private String label;
        private String id;
        private String fill;
        private Dictionary<String, double> edges = new Dictionary<string, double>();

        public Node(String id, String label, Double size, String fill) {
            this.id = id;
            this.label = label;
            this.size = size;
            this.fill = fill;
        }

        public void addSize(double val) {
            size += val;
            count++;
        }
        public void addEdge(String key, double val) {
            if (edges.ContainsKey(key)) {
                edges[key] += val;
            }
            else {
                edges.Add(key, val);
            }
        }
        public void average(int num) {
            size /= num;
            List<String> keys = edges.Keys.ToList();
            foreach (String key in keys) {
                edges[key] /= num;
            }
        }
        public void average() {
            size /= count;
            List<String> keys = edges.Keys.ToList();
            foreach (String key in keys) {
                edges[key] /= count;
            }
        }
        public double getSize() {
            return size;
        }
        public String getLabel() {
            return label;
        }
        public String getID() {
            return id;
        }
        public String getFill() {
            return fill;
        }
        public Dictionary<String, double> getEdges() {
            return edges;
        }
    }
}
