using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Daimler.Providence.Tests
{
    [ExcludeFromCodeCoverage]
    public class Awaiter
    {
        private const int Steps = 10;
        private TimeSpan _stepTime;

        public const string Seconds = "SECONDS";
        public const string Milliseconds = "MILLISECONDS";
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public static Awaiter Await()
        {
            return new Awaiter();
        }

        /// <summary>
        /// Sets the time to wait in seconds.
        /// </summary>
        public Awaiter AtMost(int seconds)
        {
            _stepTime = TimeSpan.FromSeconds(seconds / Steps);
            return this;
        }

        /// <summary>
        /// Sets the time to wait according to the unit.
        /// </summary>
        public Awaiter AtMost(int duration, string unit)
        {
            if (Seconds.Equals(unit))
            {
                _stepTime = TimeSpan.FromSeconds(duration / Steps);
            }
            else if (Milliseconds.Equals(unit))
            {
                _stepTime = TimeSpan.FromMilliseconds(duration/ Steps);
            }
            return this;
        }

        /// <summary>
        /// Waits until the property has the value.
        /// </summary>
        public void UntilProperty(string property, object o, long expectedValue)
        {
            var magicType = o.GetType();
            for (var i = 0; i < Steps; i++)
            {
                Task.Delay(_stepTime).Wait();
                var magicValue = magicType.GetProperty(property).GetValue(o);
                var myValue = (int)magicValue;
                if (myValue == expectedValue)
                {
                    return;
                }
            }
            Assert.Fail("TimeOut");
        }

        /// <summary>
        /// Waits until the property has the value.
        /// </summary>
        public void UntilMinimumProperty(string property, object o, long expectedValue)
        {
            var magicType = o.GetType();
            for (var i = 0; i < Steps; i++)
            {
                Task.Delay(_stepTime).Wait();
                var magicValue = magicType.GetProperty(property).GetValue(o);
                var myValue = (int)magicValue;
                if (myValue >= expectedValue)
                {
                    return;
                }
            }
            Assert.Fail("TimeOut");
        }

        /// <summary>
        /// Waits until the property is equals to the given object.
        /// </summary>
        public void UntilProperty(string property, object o, object expectedObject)
        {
            var magicType = o.GetType();
            for (var i = 0; i < Steps; i++)
            {
                Task.Delay(_stepTime).Wait();
                var magicValue = magicType.GetProperty(property).GetValue(o);
                if (magicValue.Equals(expectedObject))
                {
                    return;
                }
            }
            Assert.Fail("TimeOut");
        }
        
        /// <summary>
        /// Checks that the property does not change the value.
        /// </summary>
        public void KeepProperty(string property, object o, long expectedValue)
        {
            var magicType = o.GetType();
            for (var i = 0; i < Steps; i++)
            {
                Task.Delay(_stepTime).Wait();
                var magicValue = magicType.GetProperty(property).GetValue(o);
                var myValue = (int)magicValue;
                if (myValue != expectedValue)
                {
                    Assert.Fail("Value changed");
                }
            }
        }

        /// <summary>
        /// Wait until the method returns the specified value.
        /// </summary>
        public void UntilMethod(string methodName, object o, long expectedValue)
        {
            var magicType = o.GetType();
            var magicMethod = magicType.GetMethod(methodName);
            for (var i = 0; i < Steps; i++)
            {
                Task.Delay(_stepTime).Wait();
                var magicValue = magicMethod.Invoke(o, null);
                var myValue = (int)magicValue;
                if (myValue == expectedValue)
                {
                    return;
                }
            }
            Assert.Fail("TimeOut");
        }
    }
}
