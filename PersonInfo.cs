using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenaUbi
{
    class PersonInfo
    {
        private double x = 0;
        public double X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }
        private double y = 0;
        public double Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
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
