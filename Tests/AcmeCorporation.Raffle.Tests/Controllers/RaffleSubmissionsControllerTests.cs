﻿using System;
using System.Threading.Tasks;
using AcmeCorporation.Raffle.Domain;
using AcmeCorporation.Raffle.Domain.Interfaces;
using AcmeCorporation.Raffle.WebApi.Controllers;
using AcmeCorporation.Raffle.WebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AcmeCorporation.Raffle.Tests.Controllers
{
    [TestFixture]
    public class RaffleSubmissionsControllerTests
    {
        private RaffleSubmissionsController sut;
        private Mock<IRaffleSubmissionService> drawSubmissionsServiceMock;
        private Mock<ISerialNumberRepository> serialNumberRepositoryMock;

        [SetUp]
        public void Setup()
        {
            drawSubmissionsServiceMock = new Mock<IRaffleSubmissionService>();
            serialNumberRepositoryMock = new Mock<ISerialNumberRepository>();
            sut = new RaffleSubmissionsController(drawSubmissionsServiceMock.Object, serialNumberRepositoryMock.Object);
        }

        [Test]
        public async Task SubmitDraw_Succeeds_ReturnsSuccess()
        {
            // Arrange
            var serialNumber = SerialNumber.CreateNewSerialNumber(Guid.NewGuid().ToString());
            var emailStr = $"{Guid.NewGuid().ToString()}@{Guid.NewGuid().ToString()}.com";
            var dto = new SubmitDrawRequest
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                EmailAddress = emailStr,
                SerialNumber = serialNumber.Serial,
            };

            var email = new EmailAddress(emailStr);
            serialNumberRepositoryMock.Setup(x => x.GetSerialNumber(serialNumber.Serial))
                .ReturnsAsync(() => serialNumber);

            drawSubmissionsServiceMock.Setup(x =>x.Submit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailAddress>(), It.IsAny<SerialNumber>()))
                .ReturnsAsync(() => new RaffleSubmission(dto.FirstName, dto.LastName, email, serialNumber));
            
            // Act
            var response = await sut.SubmitDraw(dto);
            
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(response);
            
            var payload = (response as OkObjectResult).Value;
            var typedPayLoad = payload as DrawSubmissionListingDto;
            Assert.That(typedPayLoad.FirstName, Is.EqualTo(dto.FirstName));
            Assert.That(typedPayLoad.LastName, Is.EqualTo(dto.LastName));
            Assert.That(typedPayLoad.SerialNumber, Is.EqualTo(dto.SerialNumber));
            Assert.That(typedPayLoad.EmailAddress, Is.EqualTo(dto.EmailAddress));
        }
    }
}