﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AcmeCorporation.Raffle.Domain;
using AcmeCorporation.Raffle.Domain.Interfaces;
using AcmeCorporation.Raffle.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace AcmeCorporation.Raffle.Tests.Integration.Services
{
    [TestFixture]
    public class DrawSubmissionServiceTests : DbTestFixture
    {
        private IDrawSubmissionService sut;
        
        public override void DoSetup()
        {
            sut = new DrawSubmissionService(Context);
        }

        public override void DoTeardown()
        {
            Context.RaffleSubmissions.RemoveRange(Context.RaffleSubmissions);
            Context.SerialNumbers.RemoveRange(Context.SerialNumbers);
            Context.SaveChanges();
        }

        [Test]
        public async Task Submit_SerialNumberIsNull_Throws()
        {
            Assert.ThrowsAsync<ArgumentNullException>(()
                => sut.Submit(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                    new EmailAddress("aid@email.com"), null));
        }

        [Test]
        public async Task Submit_Succeeds()
        {
            // Arrange
            var serial = Guid.NewGuid().ToString();
            var serialNumber = SerialNumber.CreateNewSerialNumber(serial);
            
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var emailAddress = $"{Guid.NewGuid().ToString()}@{Guid.NewGuid().ToString()}.com";

            // Act
            await sut.Submit(firstName, lastName, new EmailAddress(emailAddress), serialNumber);
            await Context.SaveChangesAsync();
            
            
            // Assert
            var submission = await Context.RaffleSubmissions
                .Include(x => x.SerialNumber) // Loads the adjacent entity to only roundtrip the database once
                .SingleOrDefaultAsync();
            Assert.NotNull(submission);
            
            Assert.That(submission.FirstName, Is.EqualTo(firstName));
            Assert.That(submission.LastName, Is.EqualTo(lastName));
            Assert.That(submission.EmailAddress.Value, Is.EqualTo(emailAddress));
            
            Assert.That(submission.SerialNumber.UsageCount, Is.EqualTo(1));
        }
        
        /// <remarks>
        /// Assuming the pages size is equal to 10!
        /// This is just an assumption but should be configurable
        /// </remarks>
        [TestCase(1, 1)]
        [TestCase(15, 2)]
        [TestCase(21, 3)]
        public async Task GetSubmissions_WithNumberOfItems_GetsCorrectPageNumber(int numberOfItems, int expectedPages)
        {
            var submissions = Enumerable.Range(0, numberOfItems)
                .Select(indexer => new DrawSubmission(
                    firstName: Guid.NewGuid().ToString(),
                    lastName: Guid.NewGuid().ToString(),
                    emailAddress: new EmailAddress($"{Guid.NewGuid().ToString()}@{Guid.NewGuid().ToString()}.com"),
                    serialNumber: SerialNumber.CreateNewSerialNumber(Guid.NewGuid().ToString())));

            await Context.RaffleSubmissions.AddRangeAsync(submissions);
            await Context.SaveChangesAsync();
            
            var result = await sut.GetSubmissions(1);
            
            Assert.That(result.NumberOfPages, Is.EqualTo(expectedPages));
        }
    }
}