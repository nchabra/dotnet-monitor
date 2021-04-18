# Trace - Get Custom

Captures a diagnostic trace of a process using the given set of event providers specified in the request body.

## HTTP Route

```http
POST /trace/{pid}?durationSeconds={durationSeconds} HTTP/1.1
```

or 

```http
POST /trace/{uid}?durationSeconds={durationSeconds} HTTP/1.1
```

or

```http
POST /trace?durationSeconds={durationSeconds} HTTP/1.1
```

> **NOTE:** Process information (IDs, names, environment, etc) may change between invocations of these APIs. Processes may start or stop between API invocations, causing this information to change.

## URI Parameters

| Name | In | Required | Type | Description |
|---|---|---|---|---|
| `pid` | path | false | int | The ID of the process. |
| `uid` | path | false | guid | A value that uniquely identifies a runtime instance within a process. |
| `durationSeconds` | query | false | int | The duration of the trace operation in seconds. Default is `30`. Min is `-1` (indefinite duration). Max is `2147483647`. |

See [ProcessIdentifier](definitions.md#ProcessIdentifier) for more details about these parameters.

If neither `pid` nor `uid` are specified, a trace of the [default process](defaultprocess.md) will be captured. Attempting to capture a trace of the default process when the default process cannot be resolved will fail.

## Authentication

Allowed schemes:
- `MonitorApiKey`
- `Negotiate` (Windows only, running as unelevated)

See [Authentication](./../authentication.md) for further information.

## Request Body

A request body of type [EventProvidersConfiguration](definitions.md#EventProvidersConfiguration) is required.

The expected content type is `application/json`.

## Responses

| Name | Type | Description | Content Type |
|---|---|---|---|
| 200 OK | stream | A trace of the process. | `application/octet-stream` |
| 400 Bad Request | [ValidationProblemDetails](definitions.md#ValidationProblemDetails) | An error occurred due to invalid input. The response body describes the specific problem(s). | `application/problem+json` |
| 401 Unauthorized | | Authentication is required to complete the request. See [Authentication](./../authentication.md) for further information. | |
| 429 Too Many Requests | | There are too many trace requests at this time. Try to request a trace at a later time. | |

> **NOTE:** After the expiration of the trace duration, completing the request may take a long time (up to several minutes) for large applications if `EventProvidersConfiguration.RequestRundown` is set to `true`. The runtime needs to send over the type cache for all managed code that was captured in the trace, known as rundown events. Thus, the length of time of the request may take significantly longer than the requested duration.

## Examples

### Sample Request

```http
POST /trace/21632?durationSeconds=60 HTTP/1.1
Host: localhost:52323
Authorization: MonitorApiKey QmFzZTY0RW5jb2RlZERvdG5ldE1vbml0b3JBcGlLZXk=
Content-Type: application/json

{
    "Providers": [{
        "Name": "Microsoft-DotNETCore-SampleProfiler",
        "EventLevel": "Informational"
    },{
        "Name": "Microsoft-Windows-DotNETRuntime",
        "EventLevel": "Informational",
        "Keywords": "0x14C14FCCBD"
    }],
    "BufferSizeInMB": 1024
}
```

or

```http
POST /trace/cd4da319-fa9e-4987-ac4e-e57b2aac248b?durationSeconds=60 HTTP/1.1
Host: localhost:52323
Authorization: MonitorApiKey QmFzZTY0RW5jb2RlZERvdG5ldE1vbml0b3JBcGlLZXk=
Content-Type: application/json

{
    "Providers": [{
        "Name": "Microsoft-DotNETCore-SampleProfiler",
        "EventLevel": "Informational"
    },{
        "Name": "Microsoft-Windows-DotNETRuntime",
        "EventLevel": "Informational",
        "Keywords": "0x14C14FCCBD"
    }],
    "BufferSizeInMB": 1024
}
```

### Sample Response

The 1 minute trace with CPU information, chunk encoded, is returned as the response body.

```http
HTTP/1.1 200 OK
Content-Type: application/octet-stream
Transfer-Encoding: chunked
```

## Supported Runtimes

| Operating System | Runtime Version |
|---|---|
| Windows | .NET Core 3.1, .NET 5+ |
| Linux | .NET Core 3.1, .NET 5+ |
| MacOS | .NET Core 3.1, .NET 5+ |

## Additional Notes

### When to use `pid` vs `uid`

See [Process ID `pid` vs Unique ID `uid`](pidvsuid.md) for clarification on when it is best to use either parameter.

### View the collected `.nettrace` file

On Windows, `.nettrace` files can be viewed in [PerfView](https://github.com/microsoft/perfview) for analysis or in Visual Studio. 

A `.nettrace` files can be converted to another format (e.g. SpeedScope or Chromium) using the [dotnet-trace](https://docs.microsoft.com/dotnet/core/diagnostics/dotnet-trace) tool.