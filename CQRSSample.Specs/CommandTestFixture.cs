using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Core;
using CommonDomain;
using CQRSSample.CommandHandlers;
using CQRSSample.Commands;
using CQRSSample.Domain;
using CQRSSample.Events;
using NUnit.Framework;

namespace CQRSSample.Specs
{
    [TestFixture]
    public abstract class CommandTestFixture<TCommand, TCommandHandler, TAggregateRoot>
        where TCommand : Command
        where TCommandHandler : class, Handles<TCommand>
        where TAggregateRoot : IAggregate, new()
    {
        protected TAggregateRoot AggregateRoot;
        protected Handles<TCommand> CommandHandler;
        protected Exception CaughtException;
        protected ICollection PublishedEvents;
        protected FakeRepository Repository;
        protected virtual void SetupDependencies() { }

        protected virtual IEnumerable<DomainEvent> Given()
        {
            return new List<DomainEvent>();
        }

        protected virtual void Finally() { }

        protected abstract TCommand When();

        [SetUp]
        public void Setup()
        {
            Repository = new FakeRepository();
            CaughtException = new ThereWasNoExceptionButOneWasExpectedException();
            AggregateRoot = new TAggregateRoot();
            Given().ForEach(x => AggregateRoot.ApplyEvent(x));

            CommandHandler = BuildCommandHandler();
            SetupDependencies();
            try
            {
                CommandHandler.Handle(When());
                if (Repository.SavedAggregate == null)
                    PublishedEvents = AggregateRoot.GetUncommittedEvents();
                else
                    PublishedEvents = Repository.SavedAggregate.GetUncommittedEvents();
            }
            catch (Exception exception)
            {
                CaughtException = exception;
            }
            finally
            {
                Finally();
            }
        }

        private Handles<TCommand> BuildCommandHandler()
        {
            return Activator.CreateInstance(typeof(TCommandHandler), Repository) as TCommandHandler;
        }        
    }

    public class ThereWasNoExceptionButOneWasExpectedException : Exception { }
}