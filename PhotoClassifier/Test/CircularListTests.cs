using System.Collections.Generic;
using NUnit.Framework;

namespace PhotoClassifier.Test
{
    [TestFixture]
    internal class CircularListTests
    {
        private CircularList<int> GetTestList(int numberOfElements)
        {
            var list = new List<int>();
            for (var i = 0; i < numberOfElements; i++)
            {
                list.Add(i);
            }
            return new CircularList<int>(list);
        }

        [Test]
        public void Current_AfterMoveNext_ReturnsIdenticalElement()
        {
            var list = GetTestList(3);
            var returnedItem = list.MoveNext();

            var testItem = list.Current;

            Assert.That(testItem, Is.EqualTo(returnedItem));
        }

        [Test]
        public void Current_AfterMovePrevious_ReturnsIdenticalElement()
        {
            var list = GetTestList(3);
            var returnedItem = list.MovePrevious();

            var testItem = list.Current;

            Assert.That(testItem, Is.EqualTo(returnedItem));
        }

        [Test]
        public void Current_FirstUse_ReturnsFirstElement()
        {
            var list = GetTestList(3);

            var testItem = list.Current;

            Assert.That(testItem, Is.EqualTo(0));
        }

        [Test]
        public void MoveNext_EndOfList_ReturnsFirstElement()
        {
            var list = GetTestList(3);
            list.MoveNext();
            list.MoveNext();
            list.MoveNext();

            var testItem = list.MoveNext();

            Assert.That(testItem, Is.EqualTo(0));
        }

        [Test]
        public void MoveNext_FirstUse_ReturnsFirstElement()
        {
            var list = GetTestList(3);

            var testItem = list.MoveNext();

            Assert.That(testItem, Is.EqualTo(0));
        }

        [Test]
        public void MoveNext_SecondUse_ReturnsSecondElement()
        {
            var list = GetTestList(3);
            list.MoveNext();

            var testItem = list.MoveNext();

            Assert.That(testItem, Is.EqualTo(1));
        }


        [Test]
        public void MovePrevious_FirstUse_ReturnsLastElement()
        {
            var list = GetTestList(3);

            var testItem = list.MovePrevious();

            Assert.That(testItem, Is.EqualTo(2));
        }

        [Test]
        public void MovePrevious_SecondUse_ReturnsSecondElement()
        {
            var list = GetTestList(3);
            list.MoveNext();

            var testItem = list.MoveNext();

            Assert.That(testItem, Is.EqualTo(1));
        }
    }
}