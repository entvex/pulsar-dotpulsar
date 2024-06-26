/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace DotPulsar.Extensions;

using DotPulsar.Abstractions;
using Microsoft.Extensions.ObjectPool;
using System.Runtime.CompilerServices;

/// <summary>
/// Extensions for IReceive.
/// </summary>
public static class ReceiveExtensions
{
    static ReceiveExtensions()
    {
        var policy = new DefaultPooledObjectPolicy<CancellationTokenSource>();
        _ctsPool = new DefaultObjectPool<CancellationTokenSource>(policy);
    }

    private static readonly ObjectPool<CancellationTokenSource> _ctsPool;

    /// <summary>
    /// Will return true (and a message) if a message is buffered or false otherwise.
    /// </summary>
    public static bool TryReceive<TMessage>(this IReceive<TMessage> receiver, out TMessage? message)
    {

        var cts = _ctsPool.Get();
        var messageTask = receiver.Receive(cts.Token);

        if (!messageTask.IsCompleted)
            cts.Cancel();
        else
            _ctsPool.Return(cts);

        if (messageTask.IsCompletedSuccessfully)
            message = messageTask.Result;
        else
            message = default;

        return messageTask.IsCompletedSuccessfully;
    }

    /// <summary>
    /// Get an IAsyncEnumerable for receiving messages.
    /// </summary>
    public static async IAsyncEnumerable<TMessage> Messages<TMessage>(this IReceive<TMessage> receiver, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
            yield return await receiver.Receive(cancellationToken).ConfigureAwait(false);
    }
}
