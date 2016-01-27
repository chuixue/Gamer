using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace wuhun
{
    class mPublic
    {

        public double Vac(List<double> dt)
        {
            double avg = dt.Average();
            double tp=0.0;
            for (var i = 0; i < dt.Count; i++)
            {
                tp += (dt[i] - avg) * (dt[i] - avg);
            }
            return tp / dt.Count;
        }
    }
    
}
