https://devblogs.microsoft.com/dotnet/introducing-winforms-async-apis/?utm_source=newsletter.csharpdigest.net&utm_medium=referral&utm_campaign=invoking-async-power

As .NET continues to evolve, so do the tools available to WinForms developers, enabling more efficient and responsive applications. With .NET 9, we’re excited to introduce a collection of new asynchronous APIs that significantly streamline UI management tasks. From updating controls to showing forms and dialogs, these additions bring the power of async programming to WinForms in new ways. In this post, we’ll dive into four key APIs, explaining how they work, where they shine, and how to start using them.

## Meet the New Async APIs

.NET 9 introduces several async APIs designed specifically for WinForms, making UI operations more intuitive and performant in asynchronous scenarios. The new additions include:

-   **Control.InvokeAsync** – Fully released in .NET 9, this API helps marshal calls to the UI thread asynchronously.
-   **Form.ShowAsync** and **Form.ShowDialogAsync** (Experimental) – These APIs let developers show forms asynchronously, making life easier in complex UI scenarios.
-   **TaskDialog.ShowDialogAsync** (Experimental) – This API provides a way to show Task-Dialog-based message box dialogs asynchronously, which is especially helpful for long-running, UI-bound operations.

Let’s break down each set of APIs, starting with `InvokeAsync`.

### Control.InvokeAsync: Seamless Asynchronous UI Thread Invocation

`InvokeAsync` offers a powerful way to marshal calls to the UI thread without blocking the calling thread. The method lets you execute both synchronous and asynchronous callbacks on the UI thread, offering flexibility while preventing accidental “fire-and-forget” behavior. It does that by queueing operations in the WinForms main message queue, ensuring they’re executed on the UI thread. This behavior is similar to `Control.Invoke`, which also marshals calls to the UI thread, but there’s an important difference: `InvokeAsync` doesn’t block the calling thread because it _posts_ the delegate to the message queue, rather than _sending_ it.

### Wait – Sending vs. Posting? Message Queue?

Let’s break down these concepts to clarify what they mean and why `InvokeAsync`‘s approach can help improve app responsiveness.

In WinForms, all UI operations happen on the main UI thread. To manage these operations, the UI thread runs a loop, known as the message loop, which continually processes messages—like button clicks, screen repaints, and other actions. This loop is the heart of how WinForms stays responsive to user actions while processing instructions. When you are working with modern APIs, the majority of your App’s code does not run on this UI thread. Ideally, the UI thread should only be used to do those things which are necessary to update your UI. There are situations when your code doesn’t end up on the UI Thread automatically. One example is when you spin-up a dedicated task to perform a compute-intense operation in parallel. In these cases, you need to “marshal” the code execution to the UI thread, so that the UI thread then can update the UI. Because otherwise it’s this:

[![Screenshot of a typical Cross-Thread-Exception in the Debugger](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/12/CrossThreadException1.png)](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/12/CrossThreadException1.png)

Let’s say I am not allowed to go into a certain room to get a glass of milk, but you are. In that case, there is only one option: Since I cannot become you, I can only ask you to get me that glass of milk. And that’s the same with thread marshalling. A worker thread cannot become the UI thread. But the execution of code (the getting of the glass of milk) can be marshalled. In other words: the worker thread can ask the UI Thread to execute some code on its behalf. And, simply put, that works by queuing the delegate of a method into the message queue.

And with that, lets address this _Sending_ and _Posting_ confusion: You have two main ways to queue up actions in this loop:

**Sending a Message (Blocking):** `Control.Invoke` uses this approach. When you call `Control.Invoke`, it synchronously sends the specified delegate to the UI thread’s message queue. This action is blocking, meaning the calling thread waits until the UI thread processes the delegate before continuing. This is useful when the calling code depends on an immediate result from the UI thread but can lead to UI freezes if overused, especially during long-running operations.

**Posting a Message (Non-Blocking):** `InvokeAsync` posts the delegate to the message queue, which is a non-blocking operation. This approach tells the UI thread to queue up the action and handle it as soon as it can, but the calling thread doesn’t wait around for it to finish. The method returns immediately, allowing the calling thread to continue its work. This distinction is particularly valuable in async scenarios, as it allows the app to handle other tasks without delay, minimizing UI thread bottlenecks.

Here’s a quick comparison:

| Operation | Method | Blocking | Description |
| --- | --- | --- | --- |
| **Send** | `Control.Invoke` | Yes | Calls the delegate on the UI thread and waits for it to complete. |
| **Post** | `Control.InvokeAsync` | No | Queues the delegate on the UI thread and returns immediately. |

### Why This Matters

By posting delegates with `InvokeAsync`, your code can now queue multiple updates to controls, perform background operations, or await other async tasks without halting the main UI thread. This approach not only helps prevent the dreaded “frozen UI” experience but also keeps the app responsive even when handling numerous UI-bound tasks.

In summary: while `Control.Invoke` waits for the UI thread to complete the delegate (blocking), `InvokeAsync` hands off the task to the UI thread and returns instantly (non-blocking). This difference makes `InvokeAsync` ideal for async scenarios, allowing developers to build smoother, more responsive WinForms applications.

Here’s how each `InvokeAsync` overload works:

```csharp
public async Task InvokeAsync(Action callback, CancellationToken cancellationToken = default) public async Task<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default) public async Task InvokeAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default) public async Task<T> InvokeAsync<T>(Func<CancellationToken, ValueTask<T>> callback, CancellationToken cancellationToken = default)
```

Each overload allows for different combinations of synchronous and asynchronous methods with or without return values:

`InvokeAsync(Action callback, CancellationToken cancellationToken = default)` is for synchronous operations with _no_ return value. If you want to update a control’s property on the UI thread—such as setting the `Text` property on a `Label`—this overload allows you to do so without waiting for a return value. The callback will be posted to the message queue and executed asynchronously, returning a `Task` that you can await if needed.

```csharp
await control.InvokeAsync(() => control.Text = "Updated Text");
```

`InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)` is for synchronous operations that _do_ return a result of type `T`. Use it when you want to retrieve a value computed on the UI thread, like getting the `SelectedItem` from a `ComboBox`. `InvokeAsync` posts the callback to the UI thread and returns a `Task<T>`, allowing you to await the result.

```csharp
int itemCount = await control.InvokeAsync(() => comboBox.Items.Count);
```

`InvokeAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default):` This overload is for _asynchronous_ operations that _don’t_ return a result. It’s ideal for a longer-running async operation that updates the UI, such as waiting for data to load before updating a control. The callback receives a _`CancellationToken`_ to support cancellation and need to return a _`ValueTask`_, which `InvokeAsync` will await (internally) for completion, keeping the UI responsive while the operation runs asynchronously. So, there are two “awaits happening”: `InvokeAsync` is awaited (or rather can be awaited), and internally the ValueTask that you passed is also awaited.

```csharp
await control.InvokeAsync(async (ct) => { await Task.Delay(1000, ct); // Simulating a delay control.Text = "Data Loaded"; });
```

`InvokeAsync<T>(Func<CancellationToken, ValueTask<T>> callback, CancellationToken cancellationToken = default)` is then finally the overload version for _asynchronous_ operations that _do_ return a result of type `T`. Use it when an async operation must complete on the UI thread and return a value, such as querying a control’s state after a delay or fetching data to update the UI. The callback receives a `CancellationToken` and returns a `ValueTask<T>`, which `InvokeAsync` will await to provide the result.

```csharp
var itemCount = await control.InvokeAsync(async (ct) => { await Task.Delay(500, ct); // Simulating data fetching delay return comboBox.Items.Count; });
```

### Quick decision lookup: Choosing the Right Overload

-   For no return value with synchronous operations, use `Action`.
-   For return values with synchronous operations, use `Func<T>`.
-   For async operations without a result, use `Func<CancellationToken, ValueTask>`.
-   For async operations with a result, use `Func<CancellationToken, ValueTask<T>>`.

Using the correct overload helps you handle UI tasks smoothly in async WinForms applications, avoiding main-thread bottlenecks and enhancing app responsiveness.

Here’s a quick example:

```csharp
var control = new Control(); // Sync action await control.InvokeAsync(() => control.Text = "Hello, async world!"); // Async function with return value var result = await control.InvokeAsync(async (ct) => { control.Text = "Loading..."; await Task.Delay(1000, ct); control.Text = "Done!"; return 42; });
```

### Mixing-up asynchronous and synchronous overloads happen – or do they?

With so many overload options, it’s possible to mistakenly pass an async method to a synchronous overload, which can lead to unintended “fire-and-forget” behavior. To prevent this, WinForms introduces for .NET 9 a WinForms-specific analyzer that detects when an asynchronous method (e.g., one returning `Task`) is passed to a synchronous overload of `InvokeAsync` without a `CancellationToken`. The analyzer will trigger a warning, helping you identify and correct potential issues before they cause runtime problems.

For example, passing an async method without `CancellationToken` support might generate a warning like:

```text
warning WFO2001: Task is being passed to InvokeAsync without a cancellation token.
```

This Analyzer ensures that async operations are handled correctly, maintaining reliable, responsive behavior across your WinForms applications.

## Experimental APIs

In addition to `InvokeAsync`, WinForms introduces experimental async options for .NET 9 for showing forms and dialogs. While still in experimental stages, these APIs provide developers with greater flexibility for asynchronous UI interactions, such as document management and form lifecycle control.

`Form.ShowAsync` and `Form.ShowDialogAsync` are new methods that allow forms to be shown asynchronously. They simplify the handling of multiple form instances and are especially useful in cases where you might need several instances of the same form type, such as when displaying different documents in separate windows.

Here’s a basic example of how to use `ShowAsync`:

```csharp
var myForm = new MyForm(); await myForm.ShowAsync();
```

And for modal dialogs, you can use `ShowDialogAsync`:

```csharp
var result = await myForm.ShowDialogAsync(); if (result == DialogResult.OK) { // Perform actions based on dialog result }
```

These methods streamline the management of asynchronous form displays and help you avoid blocking the UI thread while waiting for user interactions.

### TaskDialog.ShowDialogAsync

`TaskDialog.ShowDialogAsync` is another experimental API in .NET 9, aimed at improving the flexibility of dialog interactions. It provides a way to show task dialogs asynchronously, perfect for use cases where lengthy operations or multiple steps are involved.

Here’s how to display a `TaskDialog` asynchronously:

```csharp
var taskDialogPage = new TaskDialogPage { Heading = "Processing...", Text = "Please wait while we complete the task." }; var buttonClicked = await TaskDialog.ShowDialogAsync(taskDialogPage);
```

This API allows developers to asynchronously display dialogs, freeing the UI thread and providing a smoother user experience.

## Practical Applications of Async APIs

These async APIs unlock new capabilities for WinForms, particularly in multi-form applications, MVVM design patterns, and dependency injection scenarios. By leveraging async operations for forms and dialogs, you can:

-   **Simplify form lifecycle management** in async scenarios, especially when handling multiple instances of the same form.
-   **Support MVVM and DI workflows**, where async form handling is beneficial in ViewModel-driven architectures.
-   **Avoid UI-thread blocking**, enabling a more responsive interface even during intensive operations.

If you curious about how `Invoke.Async` can revolutionize AI-driven modernization of WinForms apps then check out [this .NET Conf 2024 talk](https://youtu.be/EBpJ99VriJk) to see these features come alive in real-world scenarios!

<iframe width="800" height="450" src="https://www.youtube.com/embed/EBpJ99VriJk" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen=""></iframe>

And that’s not all—don’t miss our deep dive into everything new in .NET 9 for WinForms [in another exciting talk](https://youtu.be/1ZjCGdmQl_g?si=43PRkdjm41Y4XEwp). Dive in and get inspired!

### How to Kick Off Something Async from Something Sync

In UI scenarios, it’s common to trigger async operations from synchronous contexts. Of course, we all know it’s best practice to avoid `async void` methods.

Why is this the case? When you use `async void`, the caller has no way to await or observe the completion of the method. This can lead to unhandled exceptions or unexpected behavior. `async void` methods are essentially fire-and-forget, and they operate outside the standard error-handling mechanisms provided by `Task`. This makes debugging and maintenance more challenging in most scenarios.

But! There is an exception, and that is event handlers or methods with “event handler characteristics.” Event handlers cannot return `Task` or `Task<T>`, so `async void` allows them to trigger async actions without blocking the UI thread. However, because `async void` methods aren’t awaitable, exceptions are difficult to catch. To address this, you can use error-handling constructs like `try-catch` around the awaited operations inside the event handler. This ensures that exceptions are properly handled even in these unique cases.

For example:

```csharp
private async void Button_Click(object sender, EventArgs e) { try { await PerformLongRunningOperationAsync(); } catch (Exception ex) { MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
```

Here, the `async void` is unavoidable due to the event handler signature, but by wrapping the awaited code in a try-catch, we can safely handle any exceptions that might occur during the async operation.

The following example uses a 7-Segment control named `SevenSegmentTimer` to display a timer in the typical 7-segment-style with the resolution of a 10th of a second. It has a few methods for updating and animating the content:

```csharp
public partial class TimerForm : Form { private SevenSegmentTimer _sevenSegmentTimer; private readonly CancellationTokenSource _formCloseCancellation = new(); public FrmMain() { InitializeComponent(); SetupTimerDisplay(); } [MemberNotNull(nameof(_sevenSegmentTimer))] private void SetupTimerDisplay() { _sevenSegmentTimer = new SevenSegmentTimer { Dock = DockStyle.Fill }; Controls.Add(_sevenSegmentTimer); } override async protected void OnLoad(EventArgs e) { base.OnLoad(e); await RunDisplayLoopAsyncV1(); } private async Task RunDisplayLoopAsyncV1() { // When we update the time, the method will also wait 75 ms asynchronously. _sevenSegmentTimer.UpdateDelay = 75; while (true) { // We update and then wait for the delay. // In the meantime, the Windows message loop can process other messages, // so the app remains responsive. await _sevenSegmentTimer.UpdateTimeAndDelayAsync( time: TimeOnly.FromDateTime(DateTime.Now)); } } }
```

When we run this, we see this timer in the Form on the screen:

WinForms App running a 7-segment stop watch in dark mode with green digits, image

The async method `UpdateTimeAndDelayAsync` does exactly what it says: It updates the time displayed in the control, and then waits the amount of time, which we’ve set with the `UpdateDelay` property the line before.

As you can see, this async method `RunDisplayLoopAsyncV1` is kicked-off in the Form’s `OnLoad`. And that’s the typical approach, how we initiate something async from a synchronous void method.

For the typical WinForms dev this may look a bit weird on first glance. After all, we’re calling another method from `OnLoad`, and that method never returns because it’s ending up in an endless loop. So, does `OnLoad` in this case ever finish? Aren’t we blocking the app here?

This is where async programming shines. Even though RunDisplayLoopAsyncV1 contains an infinite loop, it’s structured asynchronously. When the await keyword is encountered inside the loop (e.g., `await _sevenSegmentTimer.UpdateTimeAndDelayAsync()`), the method yields control back to the caller until the awaited task completes.

In the context of a WinForms app, this means the Windows message loop remains free to process events like repainting the UI, handling button clicks, or responding to keyboard input. The app stays responsive because `await` pauses the execution of `RunDisplayLoopAsyncV1` without blocking the UI thread.

When `OnLoad` is marked `async`, it completes as soon as it encounters its first `await` within `RunDisplayLoopAsyncV1`. After the awaited task completes, the runtime resumes execution of `RunDisplayLoopAsyncV1` from where it left off. This happens without blocking the UI thread, effectively allowing `OnLoad` to `return` immediately, even though the asynchronous operation continues in the background.

In the background? You can think of this as splitting the method into parts, like an imaginary `WaitAsync-Initiator`, which gets called after the first `await` is resolved. Which then kicks-off the `WaitAsync-Waiter` which runs in the background, until the wait period is over. Which then again triggers the `WaitAsync-Callback` which effectively asks the message loop to reentry the call and then complete everything which follows that async call.

So, the actual code path looks then something like this:

[![State diagram showing the asynchronous flow of OnLoad and RunDisplayLoopAsyncV1 in a WinForms app, keeping the UI responsive.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/12/AsyncFromSyncStateDiagram.svg)](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/12/AsyncFromSyncStateDiagram.svg)

And the best way to internalize this is to compare it to 2 mouse-click events, which have been processed in succession, where the first mouse-click kicks off `RunDisplayLoopAsyncV1`, and the second mouse-click corresponds to the `WaitAsync` call-back into “Part 3” of that method, when the delay is just ready waiting.

This process then repeats for each subsequent `await` in an async method. And this is why the app doesn’t freeze despite the infinite loop. In fact, technically, OnLoad actually finishes normally, but the part(s) after each await are called back by the message loop later in time.

Now, we’re still pretty much using the UI Thread exclusively here. (Technically speaking, the call-backs for a short moment run on a thread-pool thread, but let’s ignore that for now.) Yes, we’re async, but nothing so far is really happening in parallel. Up to now, this is more like a clever ochestrated relay race, where the baton is so seemlessly passed to the next respective runner, that there simply are no hangs or blocks.

But an async method can be called from a different thread at any time. And if we did this currently in our sample like this…

```c
private async Task RunDisplayLoopAsyncV2() { // When we update the time, the method will also wait 75 ms asynchronously. _sevenSegmentTimer.UpdateDelay = 75; // Let's kick-off a dedicated task for the loop. await Task.Run(ActualDisplayLoopAsync); // Local function, which represents the actual loop. async Task ActualDisplayLoopAsync() { while (true) { // We update and then wait for the delay. // In the meantime, the Windows message loop can process other messages, // so the app remains responsive. await _sevenSegmentTimer.UpdateTimeAndDelayAsync( time: TimeOnly.FromDateTime(DateTime.Now)); } } }
```

then…

[![Screenshot of a Cross-Thread-Exception in the demo-app's context](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/12/CrossThreadException2.png)](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/12/CrossThreadException2.png)

### The trickiness of InvokeAsync’s overload resolution

So, as we learned earlier, this is an easy one to resolve, right? We’re just using `InvokeAsync` to call our local function `ActualDisplayLoopAsync`, and then we’re good. So, let’s do that. Let’s get the `Task` that is returned by InvokeAsync and pass that to `Task.Run`. Easy-peasy.

[![Screenshot Errors and Warnings pointing to overload resolution issues](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/12/ErrorAndWarning.png)](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/12/ErrorAndWarning.png)

Well – that doesn’t look so good. We got 2 issues. First, as mentioned before, we’re trying to invoke a method returning a `Task` without a cancellation token. `InvokeAsync` is warning us that we are setting up a fire-and-forget in this case, which cannot be internally awaited. And the second issue is not only a warning, it is an error. `InvokeAsync` is returning a `Task`, and we of course cannot pass that to `Task.Run`. We can only pass an `Action` or a `Func` _returning_ a `Task`, but surely not a `Task` itself. But, what we can do, is just converting this line into another local function, so from this…

```c
// Doesn't work. InvokeAsync wants a cancellation token, and we can't pass Task.Run a task. var invokeTask = this.InvokeAsync(ActualDisplayLoopAsync); // Let's kick-off a dedicated task for the loop. await Task.Run(invokeTask); // Local function, which represents the actual loop. async Task ActualDisplayLoopAsync(CancellationToken cancellation)
```

to this:

```c
// This is a local function now, calling the actual loop on the UI Thread. Task InvokeTask() => this.InvokeAsync(ActualDisplayLoopAsync, CancellationToken.None); await Task.Run(InvokeTask); async ValueTask ActualDisplayLoopAsync(CancellationToken cancellation=default) ...
```

And that works like a charm now!

## Parallelizing for Performance or targeted code flow

Our 7-segment control has another neat trick up its sleeve: a fading animation for the separator columns. We can use this feature as follows:

```c
private async Task RunDisplayLoopAsyncV4() { while (true) { // We also have methods to fade the separators in and out! // Note: There is no need to invoke these methods on the UI thread, // because we can safely set the color for a label from any thread. await _sevenSegmentTimer.FadeSeparatorsInAsync().ConfigureAwait(false); await _sevenSegmentTimer.FadeSeparatorsOutAsync().ConfigureAwait(false); } }
```

When we run this, it looks like this:

WinForms App running showing the 7-segment control with separator animation, image

However, there’s a challenge: How can we set up our code flow so that the running clock and the fading separators are invoked in parallel, all within a continuous loop?

To achieve this, we can leverage Task-based parallelism. The idea is to:

1.  **Run both the clock update and the separator fading simultaneously:** We execute both tasks asynchronously and wait for them to complete.
2.  **Handle differing task durations gracefully:** Since the clock update and fading animations might take different amounts of time, we use `Task.WhenAny` to ensure the faster task doesn’t delay the slower one.
3.  **Reset completed tasks:** Once a task completes, we reset it to null so the next iteration can start it anew.

And the result is this:

```c
private async Task RunDisplayLoopAsyncV6() { Task? uiUpdateTask = null; Task? separatorFadingTask = null; while (true) { async Task FadeInFadeOutAsync(CancellationToken cancellation) { await _sevenSegmentTimer.FadeSeparatorsInAsync(cancellation).ConfigureAwait(false); await _sevenSegmentTimer.FadeSeparatorsOutAsync(cancellation).ConfigureAwait(false); } uiUpdateTask ??= _sevenSegmentTimer.UpdateTimeAndDelayAsync( time: TimeOnly.FromDateTime(DateTime.Now), cancellation: _formCloseCancellation.Token); separatorFadingTask ??= FadeInFadeOutAsync(_formCloseCancellation.Token); Task completedOrCancelledTask = await Task.WhenAny(separatorFadingTask, uiUpdateTask); if (completedOrCancelledTask.IsCanceled) { break; } if (completedOrCancelledTask == uiUpdateTask) { uiUpdateTask = null; } else { separatorFadingTask = null; } } } protected override void OnFormClosing(FormClosingEventArgs e) { base.OnFormClosing(e); _formCloseCancellation.Cancel(); }
```

And this. And you can see in this animated GIF, that the UI really stays responsive all the time, because the window can be smoothly dragged around with the mouse.

Final animated version of the 7-segment timer app, image

## Summary

With these new async APIs, .NET 9 brings advanced capabilities to WinForms, making it easier to work with asynchronous UI operations. While some APIs, like `Control.InvokeAsync`, are ready for production, experimental APIs for Form and Dialog management open up exciting possibilities for responsive UI development.

You can find the sample code of this blog post in our [Extensibility-Repo](https://github.com/microsoft/winforms-designer-extensibility) in the [respective Samples subfolder](https://github.com/microsoft/winforms-designer-extensibility/tree/main/Samples/NET%209/Async%20in%20NET%209).

Explore the potential of async programming in WinForms with .NET 9, and be sure to test out the experimental features in non-critical projects. As always, your feedback is invaluable, and we look forward to hearing how these new async capabilities enhance your development process!

And, as always: Happy Coding!