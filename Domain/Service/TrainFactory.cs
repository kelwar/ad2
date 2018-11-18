using Domain.Entitys;
using Domain.Entitys.Train;
using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Service
{
    public class TrainFactory
    {
        private ConcurrentSortedDict<string, TrainRecord> _dictionary;
    }
}
