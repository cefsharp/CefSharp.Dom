using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CefSharp.Dom
{
    /// <summary>
    /// <see cref="JSHandle"/> and <see cref="ElementHandle"/> Extensions.
    /// </summary>
    public static class PuppeteerHandleExtensions
    {
        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="elementHandleTask"/> as the first argument
        /// </summary>
        /// <param name="elementHandleTask">A task that returns an <see cref="ElementHandle"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task</returns>
        /// <exception cref="SelectorException">If <paramref name="elementHandleTask"/> resolves to <c>null</c></exception>
        public static Task EvaluateFunctionAsync(this Task<ElementHandle> elementHandleTask, string pageFunction, params object[] args)
            => elementHandleTask.EvaluateFunctionAsync<object>(pageFunction, args);

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="elementHandleTask"/> as the first argument
        /// </summary>
        /// <typeparam name="T">The type of the response</typeparam>
        /// <param name="elementHandleTask">A task that returns an <see cref="ElementHandle"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        /// <exception cref="SelectorException">If <paramref name="elementHandleTask"/> resolves to <c>null</c></exception>
        public static async Task<T> EvaluateFunctionAsync<T>(this Task<ElementHandle> elementHandleTask, string pageFunction, params object[] args)
        {
            if (elementHandleTask == null)
            {
                throw new ArgumentNullException(nameof(elementHandleTask));
            }

            var elementHandle = await elementHandleTask.ConfigureAwait(false);
            if (elementHandle == null)
            {
                throw new SelectorException("Error: failed to find element matching selector");
            }

            return await elementHandle.EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome the <paramref name="elementHandle"/> as the first argument
        /// </summary>
        /// <typeparam name="T">The type of the response</typeparam>
        /// <param name="elementHandle">An <see cref="ElementHandle"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        /// <exception cref="SelectorException">If <paramref name="elementHandle"/> is <c>null</c></exception>
        public static async Task<T> EvaluateFunctionAsync<T>(this ElementHandle elementHandle, string pageFunction, params object[] args)
        {
            if (elementHandle == null)
            {
                throw new SelectorException("Error: failed to find element matching selector");
            }

            var result = await elementHandle.EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(false);
            await elementHandle.DisposeAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="arrayHandleTask"/> as the first argument. Use only after <see cref="IDevToolsContext.QuerySelectorAllHandleAsync(string)"/>
        /// </summary>
        /// <param name="arrayHandleTask">A task that returns an <see cref="JSHandle"/> that represents an array of <see cref="ElementHandle"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task</returns>
        public static Task EvaluateFunctionAsync(this Task<JSHandle> arrayHandleTask, string pageFunction, params object[] args)
            => arrayHandleTask.EvaluateFunctionAsync<object>(pageFunction, args);

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="arrayHandleTask"/> as the first argument. Use only after <see cref="IDevToolsContext.QuerySelectorAllHandleAsync(string)"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="arrayHandleTask">A task that returns an <see cref="JSHandle"/> that represents an array of <see cref="ElementHandle"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        public static async Task<T> EvaluateFunctionAsync<T>(this Task<JSHandle> arrayHandleTask, string pageFunction, params object[] args)
        {
            if (arrayHandleTask == null)
            {
                throw new ArgumentNullException(nameof(arrayHandleTask));
            }

            return await (await arrayHandleTask.ConfigureAwait(false)).EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs <paramref name="pageFunction"/> within the frame and passes it the outcome of <paramref name="arrayHandle"/> as the first argument. Use only after <see cref="IDevToolsContext.QuerySelectorAllHandleAsync(string)"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="arrayHandle">An <see cref="JSHandle"/> that represents an array of <see cref="ElementHandle"/> that will be used as the first argument in <paramref name="pageFunction"/></param>
        /// <param name="pageFunction">Function to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to <c>pageFunction</c></param>
        /// <returns>Task which resolves to the return value of <c>pageFunction</c></returns>
        public static async Task<T> EvaluateFunctionAsync<T>(this JSHandle arrayHandle, string pageFunction, params object[] args)
        {
            if (arrayHandle == null)
            {
                throw new ArgumentNullException(nameof(arrayHandle));
            }

            var result = await arrayHandle.EvaluateFunctionAsync<T>(pageFunction, args).ConfigureAwait(false);
            await arrayHandle.DisposeAsync().ConfigureAwait(false);
            return result;
        }

        internal static async IAsyncEnumerable<ElementHandle> TransposeIterableHandleAsync(this JSHandle handle)
        {
            var iterator = await handle.EvaluateFunctionHandleAsync(@"iterable => {
                return (async function* () {
                    yield* iterable;
                })();
            }").ConfigureAwait(false);

            await foreach (var item in iterator.TransposeIteratorHandleAsync())
            {
                yield return item;
            }
        }

        internal static async IAsyncEnumerable<ElementHandle> TransposeIteratorHandleAsync(this JSHandle iterator)
        {
            try
            {
                IEnumerable<ElementHandle> result;
                do
                {
                    result = await iterator.FastTransposeIteratorHandleAsync().ConfigureAwait(false);
                    foreach (var item in result)
                    {
                        yield return item;
                    }
                }
                while (result.Any());
            }
            finally
            {
                await iterator.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal static async Task<IEnumerable<ElementHandle>> FastTransposeIteratorHandleAsync(this JSHandle handle)
        {
            var array = await handle.EvaluateFunctionHandleAsync(
                @"async (iterator, size) =>
                {
                    const results = [];
                    while (results.length < size)
                    {
                        const result = await iterator.next();
                        if (result.done)
                        {
                            break;
                        }
                        results.push(result.value);
                    }
                    return results;
                }",
                20).ConfigureAwait(false);

            var properties = await array.GetPropertiesAsync().ConfigureAwait(false);

            await array.DisposeAsync().ConfigureAwait(false);
            return properties.Values.Where(handle => handle is ElementHandle).Cast<ElementHandle>();
        }
    }
}
