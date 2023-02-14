// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { CancellationToken, Progress, ProgressOptions, window } from "vscode";
import { DynamicListOfPromises } from "./DynamicListOfPromises";

const defaultMsBeforeShowing = 1000;

// Used to ensure that only one progress notification is shown at a time. Needed because can't be global when unit testing.
type SynchronizationObject = {
  dynamicListOfPromises?: DynamicListOfPromises;
};

const globalSynchronizationObject: SynchronizationObject = {};

export type WithProgress<TResult> = (
  options: ProgressOptions,
  task: (
    progress: Progress<{ message?: string; increment?: number }>,
    token: CancellationToken
  ) => Thenable<TResult>
) => Thenable<TResult>;

/**
 * Executes a task, and displays a progress notification only if the action takes longer than a given amount of time
 */
export async function withProgressAfterDelay<T>(
  options: ProgressOptions & {
    delayBeforeShowingMs?: number;
    inject?: {
      withProgress?: WithProgress<void>;
      synchronizationObject?: object;
    };
  },
  task: () => Promise<T>
): Promise<T> {
  const withProgress = options.inject?.withProgress ?? window.withProgress;
  const synchronizationObject: SynchronizationObject =
    options.inject?.synchronizationObject ?? globalSynchronizationObject;

  const delayBeforeShowingMs =
    options.delayBeforeShowingMs ?? defaultMsBeforeShowing;

  if (synchronizationObject.dynamicListOfPromises) {
    // We're already showing a progress notification, so just add the task to the list of promises
    const taskPromise = task();
    synchronizationObject.dynamicListOfPromises.add(taskPromise);

    // Return its value when its done
    return await taskPromise;
  } else {
    // New list of promises to hold any additional promises that might come through while we're waiting for the task to complete
    // These will keep the progress notification open until all of them are complete.
    synchronizationObject.dynamicListOfPromises = new DynamicListOfPromises();
    const taskPromise = task();
    synchronizationObject.dynamicListOfPromises.add(taskPromise);

    // Don't wait, it won't resolve until all tasks are done and the popup is closed
    startTimerForTaskList(
      synchronizationObject.dynamicListOfPromises,
      withProgress,
      options,
      delayBeforeShowingMs
    ).then(() => {
      // All tasks done
      synchronizationObject.dynamicListOfPromises = undefined; //asdfg
    });

    // Return this task's value when its done
    return await taskPromise;
  }
}

async function startTimerForTaskList(
  dynamicListOfPromises: DynamicListOfPromises,
  withProgress: WithProgress<void>,
  options: ProgressOptions,
  delayBeforeShowingMs: number
): Promise<void> {
  // Start the first task without showing a progress notification asdfg
  const taskListPromise = dynamicListOfPromises.getPromise();

  // Start a timer to show the progress notification if not cleared first
  let allTasksDone = false;
  async function onTimerDone(): Promise<void> {
    if (!allTasksDone) {
      // Timer fired and we still have some tasks running, so show the progress notification
      // No need to await this, since we're already waiting for the list of promises to complete
      withProgress(options, async () => await taskListPromise).then(
        () => {
          // All tasks are done, nothing else to do
        },
        () => {
          /* ignore (will be handled in catch below, but shouldn't actually happen) */
        }
      );
    }
  }
  const timeoutHandle = setTimeout(onTimerDone, delayBeforeShowingMs);

  // Wait for the list of promises to complete (which can get added to dynamically)
  try {
    await taskListPromise;
    allTasksDone = true;
  } finally {
    clearInterval(timeoutHandle);
  }
}
