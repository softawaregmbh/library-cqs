using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public class CancellationTokenTest : TestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            this.RegisterPublicDecoratorsAndVerifyContainer();
        }

        [Test]
        public async Task DoNotCancelLongRunningCommand()
        {
            await this.commandProcessor.ExecuteAsync(new LongRunningCommand());
        }

        [Test]
        public void CancelLongRunningCommand()
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            // start task
            var task = this.commandProcessor.ExecuteAsync(new LongRunningCommand(), cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);            
        }

        [Test]
        public async Task DoNotCancelLongRunningQuery()
        {
            await this.queryProcessor.ExecuteAsync(new LongRunningQuery());
        }

        [Test]
        public void CancelLongRunningQuery()
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            // start task
            var task = this.queryProcessor.ExecuteAsync(new LongRunningQuery(), cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }
    }
}
