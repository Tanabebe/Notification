using System;
using System.Collections;
using System.Collections.Generic;
using tanabebe.tech.function;
using HatebNotification.Service;
using Xunit;

namespace HatebNotification.Tests
{
    public class CompareToBlogDayTestDataClass : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new DateTime(2008, 6, 1, 7, 47, 0) , DateTime.Now };
            yield return new object[] { DateTime.Now.AddYears(-1), DateTime.Now };
            yield return new object[] { DateTime.Now.AddMonths(-1), DateTime.Now };
            yield return new object[] { DateTime.Now, DateTime.Now };
            yield return new object[] { DateTime.Now.AddHours(1), DateTime.Now };
            yield return new object[] { DateTime.Now.AddDays(2), DateTime.Now };
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class TimerBlogNotificationTest
    {
        [Theory]
        [ClassData(typeof(CompareToBlogDayTestDataClass))]
        public void CompareToBlogDayTest(DateTime date, DateTime now)
        {
            var published = MessageService.CompareToBlogDay(date, now);
            Assert.NotNull(published);
        }
    }
}
