using Moq;
using NUnit.Framework;
using PinGenerator.Data.Interfaces;
using PinGenerator.Model.Entities;
using PinGenerator.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinGenerator.Service.Tests
{
    [TestFixture]
    public class PinGeneratorHelperTest
    {
        private Mock<IUnitOfWork> unitOfWorkMock;
        private int validPins = 9580;
        private int possiblePins = 10000;

        [SetUp]
        public void Setup()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
        }

        [Test]
        public async Task TestHandlePINRetrievalNoRollover()
        {
            int requested = 5;
            IReadOnlyList<PIN> pins = new List<PIN>(new PIN[requested]);
            unitOfWorkMock.Setup(mock => mock.Pins.GetPINs(requested)).Returns(Task.FromResult(pins));

            var result = await PinGeneratorHelper.HandlePINRetrieval(unitOfWorkMock.Object, requested, new List<PIN>());

            Assert.AreEqual(result.Count, requested);
        }

        [Test]
        public async Task TestGetPinsWithRollover()
        {
            int requested = 9;
            int available = 5;
            IReadOnlyList<PIN> pins = new List<PIN>(new PIN[available]);
            IReadOnlyList<PIN> pinsAfterReset = new List<PIN>(new PIN[requested - available]);
            unitOfWorkMock.Setup(mock => mock.Pins.GetPINs(requested)).Returns(Task.FromResult(pins));
            unitOfWorkMock.Setup(mock => mock.Pins.GetPINs(requested - available)).Returns(Task.FromResult(pinsAfterReset));

            var result = await PinGeneratorHelper.HandlePINRetrieval(unitOfWorkMock.Object, requested, new List<PIN>());

            Assert.AreEqual(result.Count, requested);
        }

        [Test]
        public async Task TestAddPINsWithoutInitialList()
        {
            int requested = 5;
            IReadOnlyList<PIN> pins = new List<PIN>(new PIN[requested]);
            unitOfWorkMock.Setup(mock => mock.Pins.GetPINs(requested)).Returns(Task.FromResult(pins));

            var result = await PinGeneratorHelper.AddPINsFromDataStore(unitOfWorkMock.Object, requested, new List<PIN>());

            Assert.AreEqual(result.Count, requested);
        }

        [Test]
        public async Task TestAddPINsWithInitialList()
        {
            int requested = 5;
            IReadOnlyList<PIN> pins = new List<PIN>(new PIN[requested]);
            unitOfWorkMock.Setup(mock => mock.Pins.GetPINs(requested)).Returns(Task.FromResult(pins));

            var result = await PinGeneratorHelper.AddPINsFromDataStore(unitOfWorkMock.Object, requested, pins);

            Assert.AreEqual(result.Count, requested*2);
        }

        [Test]
        public void TestConvertPinListToDataTable()
        {
            int requested = 5;
            List<PIN> pins = new List<PIN>(new PIN[requested]);

            var result = PinGeneratorHelper.ConvertToDataTable(pins);

            Assert.AreEqual(pins.Count, result.Rows.Count);
        }

        [Test]
        public async Task TestFirstTimeGeneration()
        {
            unitOfWorkMock.Setup(mock => mock.Pins.AddPins(It.IsAny<DataTable>())).Returns(Task.FromResult(true));

            var result = await PinGeneratorHelper.FirstTimePinGenerationAsync(unitOfWorkMock.Object);

            Assert.IsTrue(result);
        }

        [Test]
        public void TestAllPINsCreation()
        {
            int expectedReturnPins = validPins;
            var result = PinGeneratorHelper.GetAllPINs();

            Assert.AreEqual(result.Count, expectedReturnPins);
        }

        [Test]
        public void TestAllPossiblePINs()
        {
            int expectedReturnPins = possiblePins;
            var result = PinGeneratorHelper.GetAllPossiblePins();

            Assert.AreEqual(result.Count(), expectedReturnPins);
        }

        [Test]
        public void TestAllValidPINs()
        {
            int expectedReturnPins = validPins;

            var possiblePins = PinGeneratorHelper.GetAllPossiblePins();

            var result = PinGeneratorHelper.GetAllValidPins(possiblePins);

            Assert.AreEqual(result.Count(), expectedReturnPins);
        }

        [Test]
        public void TestMapPINStringsToObjectList()
        {
            List<string> pinStrings = new List<string>{ "0000", "1111", "5453" }; 
            var result = PinGeneratorHelper.MapToPINList(pinStrings);

            Assert.AreEqual(result.Count(), pinStrings.Count());
            Assert.AreEqual(result.First().Allocated, false);
            Assert.AreEqual(result.First().PinString, pinStrings.First());
            Assert.AreEqual(result.Last().PinString, pinStrings.Last());
        }

        [Test]
        public void TestRequestNotFulfilledCheckTrue()
        {
            int requested = 5;
            var result = PinGeneratorHelper.RequestNotFulfilled(requested+1, new List<PIN>(new PIN[requested]));

            Assert.IsTrue(result);
        }

        [Test]
        public void TestRequestNotFulfilledCheckFalse()
        {
            int requested = 5;
            var result = PinGeneratorHelper.RequestNotFulfilled(requested, new List<PIN>(new PIN[requested]));

            Assert.IsFalse(result);
        }

        [Test]
        public void TestIsValidPINFlagTrue()
        {
            var result = PinGeneratorHelper.IsValidPin("6442");

            Assert.IsTrue(result);
        }

        [Test]
        public void TestIsValidPINFlagFalse()
        {
            var result = PinGeneratorHelper.IsValidPin("5555");

            Assert.IsFalse(result);
        }

        [Test]
        public void TestPairFlagTrue()
        {
            var result = PinGeneratorHelper.ContainsPairs("1122");

            Assert.IsTrue(result);
        }

        [Test]
        public void TestPairFlagFalse()
        {
            var result = PinGeneratorHelper.ContainsPairs("1342");

            Assert.IsFalse(result);
        }

        [Test]
        public void TestRepeatFlagTrue()
        {
            var result = PinGeneratorHelper.ContainsRepeat("4343");

            Assert.IsTrue(result);
        }

        [Test]
        public void TestRepeatFlagFalse()
        {
            var result = PinGeneratorHelper.ContainsRepeat("5364");

            Assert.IsFalse(result);
        }

        [Test]
        public void TestYearFlagTrue()
        {
            var result = PinGeneratorHelper.IsLogicalYear("1986");

            Assert.IsTrue(result);
        }

        [Test]
        public void TestYearFlagFalse()
        {
            var result = PinGeneratorHelper.IsLogicalYear("5434");

            Assert.IsFalse(result);
        }

        [Test]
        public void TestPalindromeFlagTrue()
        {
            var result = PinGeneratorHelper.IsPalindromic("9779");

            Assert.IsTrue(result);
        }

        [Test]
        public void TestPalindromeFlagFalse()
        {
            var result = PinGeneratorHelper.IsPalindromic("9775");

            Assert.IsFalse(result);
        }


        [Test]
        public void TestSequentialFlagTrue()
        {
            var result = PinGeneratorHelper.IsSequential("4567");
            Assert.IsTrue(result);

            result = PinGeneratorHelper.IsSequential("7654");
            Assert.IsTrue(result);
        }

        [Test]
        public void TestSequentialFlagFalse()
        {
            var result = PinGeneratorHelper.IsSequential("4576");
            Assert.IsFalse(result);

            result = PinGeneratorHelper.IsSequential("7645");
            Assert.IsFalse(result);
        }
    }
}
