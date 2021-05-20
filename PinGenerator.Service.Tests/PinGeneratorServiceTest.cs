using Moq;
using NUnit.Framework;
using PinGenerator.Data.Interfaces;
using PinGenerator.Model.Entities;
using PinGenerator.Service.Services;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PinGenerator.Service.Tests
{
    [TestFixture]
    public class PinGeneratorServiceTest
    {
        private Mock<IUnitOfWork> unitOfWorkMock;
        private PinGeneratorService pinGeneratorService;

        [SetUp]
        public void Setup()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
            pinGeneratorService = new PinGeneratorService(unitOfWorkMock.Object);
        }

        [Test]
        public async Task TestAlreadyInitialized()
        {
            unitOfWorkMock.Setup(mock => mock.Pins.IsInitialized()).Returns(Task.FromResult(true));

            bool isInitialized = await pinGeneratorService.InitializePins();

            Assert.IsTrue(isInitialized);
        }

        [Test]
        public async Task TestNotInitialized()
        {
            unitOfWorkMock.Setup(mock => mock.Pins.IsInitialized()).Returns(Task.FromResult(false));

            unitOfWorkMock.Setup(mock => mock.Pins.AddPins(It.IsAny<DataTable>())).Returns(Task.FromResult(true));

            bool isInitialized = await pinGeneratorService.InitializePins();

            Assert.IsTrue(isInitialized);
        }

        [Test]
        public async Task TestNotInitializedFailure()
        {
            unitOfWorkMock.Setup(mock => mock.Pins.IsInitialized()).Returns(Task.FromResult(false));

            unitOfWorkMock.Setup(mock => mock.Pins.AddPins(It.IsAny<DataTable>())).Returns(Task.FromResult(false));

            bool isInitialized = await pinGeneratorService.InitializePins();

            Assert.IsFalse(isInitialized);
        }

        [Test]
        public async Task TestGetPinsNoRollover()
        {
            int requested = 5;
            IReadOnlyList<PIN> pins = new List<PIN>( new PIN[requested] );
            unitOfWorkMock.Setup(mock => mock.Pins.GetPINs(requested)).Returns(Task.FromResult(pins));

            var result = await pinGeneratorService.GetPINs(requested);

            Assert.AreEqual(result.Count, requested);
        }

        [Test]
        public async Task TestGetPinsWithRollover()
        {
            int requested = 9;
            int available = 5;
            IReadOnlyList<PIN> pins = new List<PIN>(new PIN[available]);
            IReadOnlyList<PIN> pinsAfterReset = new List<PIN>(new PIN[requested-available]);
            unitOfWorkMock.Setup(mock => mock.Pins.GetPINs(requested)).Returns(Task.FromResult(pins));
            unitOfWorkMock.Setup(mock => mock.Pins.GetPINs(requested-available)).Returns(Task.FromResult(pinsAfterReset));

            var result = await pinGeneratorService.GetPINs(requested);

            Assert.AreEqual(result.Count, requested);
        }
    }
}