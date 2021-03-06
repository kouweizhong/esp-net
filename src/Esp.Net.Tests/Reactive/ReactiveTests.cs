﻿#region copyright
// Copyright 2015 Dev Shop Limited
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Esp.Net.Reactive
{
    [TestFixture]
    public class ReactiveTests
    {
        public class TestModel
        {
        }

        private TestModel _model;

        private IEventContext _eventContext;

        private StubEventObservationRegistrar _eventObservationRegistrar;

        [SetUp]
        public void SetUp()
        {
            _model = new TestModel();
            _eventContext = new EventContext();
            _eventObservationRegistrar = new StubEventObservationRegistrar();
        }

        [Test]
        public void SubjectOnNextsItems()
        {
            var subject = new EventSubject<int, IEventContext, TestModel>(_eventObservationRegistrar);
            TestModel receivedModel = null;
            int receivedEvent = 0;
            IEventContext receivedContext = null;
            subject.Observe((e, c, m) =>
            {
                receivedModel = m;
                receivedEvent = e;
                receivedContext = c;
            });
            subject.OnNext(1, _eventContext, _model);
            receivedModel.ShouldBeSameAs(_model);
            receivedEvent.ShouldBe(1);
            receivedContext.ShouldBeSameAs(_eventContext);
        }

        [Test]
        public void SubjectRemovesSubscriptionOnDispose()
        {
            var subject = new EventSubject<int, IEventContext, TestModel>(_eventObservationRegistrar);
            int received = 0;
            var disposable = subject.Observe((e, c, m) => received = e);
            subject.OnNext(1, _eventContext, _model);
            Assert.AreEqual(1, received);
            disposable.Dispose();
            subject.OnNext(2, _eventContext, _model);
            Assert.AreEqual(1, received);
        }

        [Test]
        public void WhereFiltersWithProvidedPredicate()
        {
            var subject = new EventSubject<int, IEventContext, TestModel>(_eventObservationRegistrar);
            List<int> received = new List<int>();
            subject
                .Where((e, c, m) => e%2 == 0)
                .Observe((e, c, m) => received.Add(e));
            for (int i = 0; i < 10; i++) subject.OnNext(i, _eventContext, _model);
            Assert.IsTrue(received.SequenceEqual(new[] {0, 2, 4, 6, 8}));
        }

        [Test]
        public void WhereChainsSourceDisposableOnDispose()
        {
            var mockIEventObservable = new StubEventObservable<TestModel>();
            var disposable = mockIEventObservable
                .Where((e, c, m) => true)
                .Observe((e, c, m) => { });
            disposable.Dispose();
            Assert.IsTrue(mockIEventObservable.IsDisposed);
        }

        [Test]
        public void WhereChainsOnCompletedToSource()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void CanMergeEventStreams()
        {
            var subject1 = new EventSubject<int, IEventContext, TestModel>(_eventObservationRegistrar);
            var subject2 = new EventSubject<int, IEventContext, TestModel>(_eventObservationRegistrar);
            var stream = EventObservable.Merge(subject1, subject2);
            List<int> received = new List<int>();
            stream.Observe((e, c, m) => received.Add(e));
            subject1.OnNext(1, _eventContext, _model);
            subject2.OnNext(2, _eventContext, _model);
            subject1.OnNext(3, _eventContext, _model);
            subject2.OnNext(4, _eventContext, _model);
            Assert.IsTrue(received.SequenceEqual(new[] {1, 2, 3, 4}));
        }

        [Test]
        public void TakeOnlyTakesGivenNumberOfEvents()
        {
            List<int> received = new List<int>();
            var subject1 = new EventSubject<int, IEventContext, TestModel>(_eventObservationRegistrar);
            subject1.Take(3).Observe((e, c, m) => received.Add(e));
            subject1.OnNext(1, _eventContext, _model);
            subject1.OnNext(2, _eventContext, _model);
            subject1.OnNext(3, _eventContext, _model);
            subject1.OnNext(4, _eventContext, _model);
            Assert.IsTrue(received.SequenceEqual(new[] {1, 2, 3}));
        }

        [Test]
        public void TakeChainsSourceDisposableOnDispose()
        {
            var mockIEventObservable = new StubEventObservable<TestModel>();
            var disposable = mockIEventObservable.Take(3).Observe((e, c, m) => { });
            disposable.Dispose();
            Assert.IsTrue(mockIEventObservable.IsDisposed);
        }

        [Test]
        public void TakeChainsOnCompletedToSource()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void IncrementsObservationRegistrarOnObserve()
        {
            var subject1 = new EventSubject<int, IEventContext, TestModel>(_eventObservationRegistrar);
            subject1.Observe((e, c, m) => { });
            _eventObservationRegistrar.Register[typeof (int)].ShouldBe(1);
        }

        [Test]
        public void DecrementsObservationRegistrarOnObserve()
        {
            var subject1 = new EventSubject<int, IEventContext, TestModel>(_eventObservationRegistrar);
            IDisposable disposable = subject1.Observe((e, c, m) => { });
            disposable.Dispose();
            _eventObservationRegistrar.Register[typeof (int)].ShouldBe(0);
        }
    }
}