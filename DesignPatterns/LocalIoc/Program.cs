namespace ConsoleApp2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Program
    {
        public static void Main()
        {
            var person = new Person { Age = 21, Names = new string[] { "Pesho" } };

            Assert(expected: true, actual: person.HasAny(p => p.Names).And.HasNo(p => p.Children));
            Assert(expected: false, actual: person.HasAny(p => p.Names).And.HasAny(p => p.Children));
            Assert(expected: false, actual: person.HasNo(p => p.Names).And.HasNo(p => p.Children));
            Assert(expected: false, actual: person.HasNo(p => p.Names).And.HasAny(p => p.Children));
            Assert(expected: false, actual: person.HasNo(p => p.Names).Or.HasAny(p => p.Children));
            Assert(expected: true, actual: person.HasNo(p => p.Names).Or.HasNo(p => p.Children));
            Assert(expected: true, actual: person.HasAny(p => p.Names).Or.HasAny(p => p.Children));
            Assert(expected: true, actual: person.HasAny(p => p.Names).Or.HasNo(p => p.Children));
        }

        private static void Assert(bool expected, bool actual)
        {
            Console.WriteLine($"Expected: {expected}    Actual: {actual}");
        }
    }

    public enum BoolResultType { None, And, Or };

    public class BoolResult<TSelf>
    {
        public BoolResult(bool result, TSelf self, BoolResultType type = BoolResultType.None)
        {
            Result = result;
            Self = self;
            Type = type;
        }

        public bool Result { get; }

        public TSelf Self { get; }
        public BoolResultType Type { get; }

        public BoolResult<TSelf> And
        {
            get => new BoolResult<TSelf>(Result, Self, BoolResultType.And);
        }

        public BoolResult<TSelf> Or
        {
            get => new BoolResult<TSelf>(Result, Self, BoolResultType.Or);
        }

        public BoolResult<TSelf> Combine(bool result)
        {
            switch (this.Type)
            {
                case BoolResultType.None:
                    return new BoolResult<TSelf>(result, Self, BoolResultType.None);

                case BoolResultType.And:
                    return new BoolResult<TSelf>(result && Result, Self, BoolResultType.None);

                case BoolResultType.Or:
                    return new BoolResult<TSelf>(result || Result, Self, BoolResultType.None);

                default: throw new ArgumentException();
            }
        }

        public override string ToString()
        {
            return Result.ToString();
        }

        public static implicit operator bool(BoolResult<TSelf> result)
        {
            return result.Result;
        }
    }

    public static class ObjectExtensions
    {
        public static BoolResult<TSelf> HasAny<TSelf, TCollection>(this TSelf self, Func<TSelf, IEnumerable<TCollection>> selector)
        {
            var collection = selector.Invoke(self) ?? new List<TCollection>();

            return new BoolResult<TSelf>(collection.Any(), self);
        }

        public static BoolResult<TSelf> HasNo<TSelf, TCollection>(this TSelf self, Func<TSelf, IEnumerable<TCollection>> selector)
        {
            var collection = selector.Invoke(self) ?? new List<TCollection>();

            return new BoolResult<TSelf>(!collection.Any(), self);
        }

        public static BoolResult<TSelf> HasNo<TSelf, TCollection>(this BoolResult<TSelf> boolResult, Func<TSelf, IEnumerable<TCollection>> selector)
        {
            var collection = selector.Invoke(boolResult.Self) ?? new List<TCollection>();

            return boolResult.Combine(!collection.Any());
        }

        public static BoolResult<TSelf> HasAny<TSelf, TCollection>(this BoolResult<TSelf> boolResult, Func<TSelf, IEnumerable<TCollection>> selector)
        {
            var collection = selector.Invoke(boolResult.Self) ?? new List<TCollection>();

            return boolResult.Combine(collection.Any());
        }
    }

    class Person
    {
        public string[] Names { get; set; }

        public string[] Children { get; set; }

        public int Age { get; set; }
    }
}