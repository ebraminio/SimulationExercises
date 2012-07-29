using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationProject
{
    abstract public class Simulator<T> : IEnumerable<T>
    {
        public abstract IEnumerator<T> GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ItemPicker<T> : IEnumerable<T>
    {
        private IEnumerator<double> _mantissaEnumerator;
        public IDictionary<T, double> _possiblities { private set; get; }
        private double _sum = 0;
        public ItemPicker(IEnumerable<double> mantissaYielder)
        {
            _possiblities = new Dictionary<T, double>();
            _mantissaEnumerator = mantissaYielder.GetEnumerator();
        }

        public void AddEntityPossibilty(T entity, double possibilty)
        {
            _possiblities.Add(entity, possibilty);
            _sum += possibilty;
        }

        private T Yield()
        {
            var rand = _mantissaEnumerator.Current * _sum;

            foreach (var kv in _possiblities)
            {
                if (rand == 0)
                {
                    return kv.Key;
                }
                rand -= kv.Value;
                if (rand <= 0)
                {
                    return kv.Key;
                }
            }
            return default(T);
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (_mantissaEnumerator.MoveNext())
            {
                yield return Yield();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RandomMantissa : IEnumerable<double>
    {
        private Random _random = new Random();
        public IEnumerator<double> GetEnumerator()
        {
            while (true)
            {
                yield return _random.NextDouble();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class FlipFlopMantissa : IEnumerable<double>
    {
        public IEnumerator<double> GetEnumerator()
        {
            bool _flag = false;
            while (true)
            {
                _flag = !_flag;
                yield return _flag ? 0 : 0.99;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
