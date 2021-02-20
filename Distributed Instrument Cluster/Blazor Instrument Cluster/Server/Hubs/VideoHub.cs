using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
/// Signal R Hubs
/// <author>Mikael Nilssen</author>
/// </summary>
namespace Blazor_Instrument_Cluster.Server.Hubs {

    public class VideoHub : Hub {
        protected bool isSendingFrames = true;

        public ChannelReader<string> Counter(int delay, CancellationToken cancellationToken) {
            var channel = Channel.CreateUnbounded<string>();

            // We don't want to await WriteItemsAsync, otherwise we'd end up waiting
            // for all the items to be written before returning the channel back to
            // the client.
            _ = WriteItemsAsync(channel.Writer, delay, cancellationToken);

            return channel.Reader;
        }

        private async Task WriteItemsAsync(ChannelWriter<string> writer, int delay, CancellationToken cancellationToken) {
            Exception localException = null;
            try {
                int i = 0;
                while (!cancellationToken.IsCancellationRequested) {
                    await writer.WriteAsync("Current int is "+i, cancellationToken);
                    i++;
                    // Use the cancellationToken in other APIs that accept cancellation
                    // tokens so the cancellation can flow down to them.
                    await Task.Delay(delay, cancellationToken);
                }
            } catch (Exception ex) {
                localException = ex;
            } finally {
                writer.Complete(localException);
            }
        }
    }
}