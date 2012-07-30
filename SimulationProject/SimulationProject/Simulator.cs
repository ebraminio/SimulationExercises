using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationProject
{
    abstract public class Simulator<T> : IEnumerable<T> where T : Entity
    {
        public abstract IEnumerator<T> GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    abstract public class Entity
    {
        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            Entity o = obj as Entity;
            if (obj == null || obj.ToString() == "{DisconnectedItem}")
                return false;


            foreach (var property in this.GetType().GetProperties())
            {
                var x = property.GetValue(this, null) as IComparable;
                var y = property.GetValue(obj, null) as IComparable;

                if (x == null || y == null)
                {
                    throw new NotSupportedException();
                }

                if (!x.Equals(y))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash = hash * 23 + base.GetHashCode();
            hash = hash * 23 + Id.GetHashCode();
            return hash;
        }
    }

    public class ItemPicker<T> : IEnumerable<T>
    {
        private IEnumerator<double> _mantissaEnumerator;
        public IDictionary<T, double> _possiblities { private set; get; }
        private double _sum = 0;
        public ItemPicker(IEnumerator<double> mantissaYielder)
        {
            _possiblities = new Dictionary<T, double>();
            _mantissaEnumerator = mantissaYielder;
        }

        public void AddEntityPossibilty(T entity, double possibilty)
        {
            _possiblities.Add(entity, possibilty);
            _sum += possibilty;
        }

        private T Yield()
        {
            var mantissa = _mantissaEnumerator.Current * _sum;

            if (_mantissaEnumerator.Current == .88)
                mantissa = .88;
            foreach (var kv in _possiblities)
            {
                if (Math.Round(mantissa, 4) == 0)
                {
                    return kv.Key;
                }
                mantissa -= kv.Value;
                if (Math.Round(mantissa, 4) <= 0)
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
