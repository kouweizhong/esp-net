﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Esp.Net
{
    public partial class RouterTests
    {
        public class RouterHalting : RouterTests
        {
            [Test]
            public void ShouldHaltAndRethrowIfAPreProcessorErrors()
            {
                _model1EventProcessor.Event1Details.NormalStage.RegisterAction((m, e) =>
                {
                    throw new Exception("Boom");
                });
                AssertPublishEventThrows();
            }

            [Test]
            public void ShouldHaltAndRethrowIfAnEventProcessorErrors()
            {
                _model1PreEventProcessor.RegisterAction(m =>
                {
                    throw new Exception("Boom");
                });
                AssertPublishEventThrows();
            }

            [Test]
            public void ShouldHaltAndRethrowIfAnPostProcessorErrors()
            {
                _model1PostEventProcessor.RegisterAction(m =>
                {
                    throw new Exception("Boom");
                });
                AssertPublishEventThrows();
            }

            [Test]
            public void ShouldHaltAndRethrowIfAModelObserverErrors()
            {
                _model1Controller.RegisterAction(m =>
                {
                    throw new Exception("Boom");
                });
                AssertPublishEventThrows();
            }

            [Test]
            public void TerminalErrorHandlerGetInvokedOnHaltingException()
            {
                var exception = new Exception("Boom");
                _model1Controller.RegisterAction(m =>
                {
                    throw exception;
                });
                AssertPublishEventThrows();
                _terminalErrorHandler.Errors.Count.ShouldBe(2);
                _terminalErrorHandler.Errors[0].ShouldBe(exception);
                _terminalErrorHandler.Errors[1].InnerException.ShouldBe(exception);
            }

            private void AssertPublishEventThrows()
            {
                _router.PublishEvent(_model1.Id, new Event1());
                _terminalErrorHandler.Errors.Count.ShouldBe(1);
                _terminalErrorHandler.Errors[0].ShouldBeOfType<Exception>();
                _terminalErrorHandler.Errors[0].Message.ShouldBe("Boom");

                _router.PublishEvent(_model1.Id, new Event2());

                _terminalErrorHandler.Errors.Count.ShouldBe(2);
                _terminalErrorHandler.Errors[1].ShouldBeOfType<Exception>();
                _terminalErrorHandler.Errors[1].Message.ShouldBe("Router halted due to previous error");
            }

            public abstract class HaltedTestBase : RouterTests
            {
                protected void HaltRouter()
                {
                    var exceptionThrow = false;
                    _model1PreEventProcessor.RegisterAction(m =>
                    {
                        if (!exceptionThrow)
                        {
                            exceptionThrow = true;
                            throw new Exception("Boom");
                        }
                    });
                    try
                    {
                        _router.PublishEvent(_model1.Id, new Event1());
                    }
                    catch
                    {
                    }
                }

                [Test]
                public void ShouldThrowOnAddModel()
                {
                    DoAssert(() => _router.AddModel(Guid.NewGuid(), _model1));
                    DoAssert(() => _router.AddModel(Guid.NewGuid(), _model1, (IPreEventProcessor<TestModel>)new StubModelProcessor()));
                    DoAssert(() => _router.AddModel(Guid.NewGuid(), _model1, (IPostEventProcessor<TestModel>)new StubModelProcessor()));
                    DoAssert(() => _router.AddModel(Guid.NewGuid(), _model1, new StubModelProcessor(), new StubModelProcessor()));
                }

                [Test]
                public void ShouldThrowOnGetModelObservable()
                {
                    DoAssert(() => _router.GetModelObservable<TestModel>(_model1.Id));
                }

                [Test]
                public void ShouldThrowOnGetEventObservable()
                {
                    DoAssert(() => _router.GetEventObservable<Event1, TestModel>(_model1.Id));
                }

                [Test]
                public void ShouldThrowOnPublishEvent()
                {
                    DoAssert(() => _router.PublishEvent(_model1.Id, new Event2()));
                    DoAssert(() => _router.PublishEvent(_model1.Id, (object)new Event2()));
                }

                [Test]
                public void ShouldThrowOnExecuteEvent()
                {
                    Assert.Inconclusive();
                    // AssertRethrows(() => _router.ExecuteEvent(_model1.Id, new Event2()));
                }

                [Test]
                public void ShouldThrowOnBroadcastEvent()
                {
                    DoAssert(() => _router.BroadcastEvent(new Event2()));
                    DoAssert(() => _router.BroadcastEvent((object)new Event2()));
                }

                protected abstract void DoAssert(TestDelegate test);
            }

            public class WhenHaltedWithTerminalErrorHandler : HaltedTestBase
            {
                public override void SetUp()
                {
                    base.SetUp();
                    HaltRouter();
                }

                protected override void DoAssert(TestDelegate action)
                {
                    var previousErrorCount = _terminalErrorHandler.Errors.Count;
                    action();
                    _terminalErrorHandler.Errors.Count.ShouldBe(previousErrorCount + 1);
                    Exception ex = _terminalErrorHandler.Errors.Last();
                    ex.Message.ShouldBe("Router halted due to previous error");
                    ex.InnerException.Message.ShouldBe("Boom");
                }
            }

            public class WhenHaltedWithoutTerminalErrorHandler : HaltedTestBase
            {
                [SetUp]
                public override void SetUp()
                {
                    _routerDispatcher = new StubRouterDispatcher();
                    _router = new Router(_routerDispatcher);
                    AddModel1();
                    HaltRouter();
                }

                protected override void DoAssert(TestDelegate action)
                {
                    Exception ex = Assert.Throws<Exception>(action);
                    ex.Message.ShouldBe("Router halted due to previous error");
                    ex.InnerException.Message.ShouldBe("Boom");
                }
            }
        }
    }
}