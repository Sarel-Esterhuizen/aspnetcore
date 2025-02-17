// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc.Infrastructure;

/// <summary>
/// A <see cref="IActionResultExecutor{FileContentResult}"/>
/// </summary>
public class FileContentResultExecutor : FileResultExecutorBase, IActionResultExecutor<FileContentResult>
{
    /// <summary>
    /// Intializes a new <see cref="FileContentResultExecutor"/>.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers.</param>
    public FileContentResultExecutor(ILoggerFactory loggerFactory)
        : base(CreateLogger<FileContentResultExecutor>(loggerFactory))
    {
    }

    /// <inheritdoc />
    public virtual Task ExecuteAsync(ActionContext context, FileContentResult result)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        Logger.ExecutingFileResult(result);

        var (range, rangeLength, serveBody) = SetHeadersAndLog(
            context,
            result,
            result.FileContents.Length,
            result.EnableRangeProcessing,
            result.LastModified,
            result.EntityTag);

        if (!serveBody)
        {
            return Task.CompletedTask;
        }

        return WriteFileAsync(context, result, range, rangeLength);
    }

    /// <summary>
    /// Writes the file content.
    /// </summary>
    /// <param name="context">The action context.</param>
    /// <param name="result">The <see cref="FileContentResult"/>.</param>
    /// <param name="range">The <see cref="RangeItemHeaderValue"/>.</param>
    /// <param name="rangeLength">The length of the range.</param>
    protected virtual Task WriteFileAsync(ActionContext context, FileContentResult result, RangeItemHeaderValue? range, long rangeLength)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (range != null && rangeLength == 0)
        {
            return Task.CompletedTask;
        }

        if (range != null)
        {
            Logger.WritingRangeToBody();
        }

        var fileContentStream = new MemoryStream(result.FileContents);
        return WriteFileAsync(context.HttpContext, fileContentStream, range, rangeLength);
    }
}
