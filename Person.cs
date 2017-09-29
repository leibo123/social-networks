using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenaUbi
{
    class Person
    {
        private String type = "";
        public String Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
        private double interactions = 0;
        public double Interactions
        {
            get
            {
                return this.interactions;
            }
            set
            {
                this.interactions = value;
            }
        }


    }
}
