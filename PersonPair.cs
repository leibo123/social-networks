using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenaUbi {
    class PersonPair {
        private double interactions = 0;
        public double Interactions {
            get {
                return this.interactions;
            }
            set {
                this.interactions = value;
            }
        }
        private Person person1 = new Person();
        public Person Person1 {
            get {
                return this.person1;
            }
            set {
                this.person1 = value;
            }
        }
        private Person person2 = new Person();
        public Person Person2 {
            get {
                return this.person2;
            }
            set {
                this.person2 = value;
            }
        }
    }
}
