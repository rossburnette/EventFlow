﻿// The MIT License (MIT)
//
// Copyright (c) 2015 Rasmus Mikkelsen
// https://github.com/rasmus/EventFlow
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.ReadStores;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Aggregates.Test;
using EventFlow.TestHelpers.Aggregates.Test.Events;
using Moq;
using NUnit.Framework;

namespace EventFlow.Tests.UnitTests.ReadStores
{
    public class ReadStoreManagerTests : TestsFor<SingleAggregateReadStoreManager<IReadModelStore<ReadStoreManagerTests.TestReadModel>, ReadStoreManagerTests.TestReadModel>>
    {
        public class TestReadModel : IReadModel,
            IAmReadModelFor<TestAggregate, TestId, PingEvent>
        {
            public void Apply(IReadModelContext context, IDomainEvent<TestAggregate, TestId, PingEvent> e)
            {
            }
        }

        private Mock<IReadModelStore<TestReadModel>> _readModelStoreMock;

        [SetUp]
        public void SetUp()
        {
            _readModelStoreMock = InjectMock<IReadModelStore<TestReadModel>>();
        }

        [Test]
        public async Task ReadStoreIsUpdatedWithRelevantEvents()
        {
            // Arrange
            var events = new []
                {
                    ToDomainEvent(A<PingEvent>()),
                    ToDomainEvent(A<DomainErrorAfterFirstEvent>()),
                };

            // Act
            await Sut.UpdateReadStoresAsync(events, CancellationToken.None).ConfigureAwait(false);

            // Assert
            _readModelStoreMock.Verify(
                s => s.UpdateAsync(
                    It.Is<IReadOnlyCollection<ReadModelUpdate>>(l => l.Count == 1),
                    It.IsAny<IReadModelContext>(),
                    It.IsAny<Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TestReadModel>, CancellationToken, Task<ReadModelEnvelope<TestReadModel>>>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
