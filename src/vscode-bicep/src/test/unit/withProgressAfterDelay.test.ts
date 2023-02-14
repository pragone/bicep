// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ProgressLocation } from "vscode";
import { Deferred } from "../../utils/Deferred";
import { sleep } from "../../utils/time";
import { withProgressAfterDelay } from "../../utils/withProgressAfterDelay";
import { createWithProgressMock } from "../utils/vscodeMocks";

describe("withProgressAfterDelay", () => {
  it("should not show progress notification if task is short - default delay", async () => {
    const withProgressMock = createWithProgressMock<void>();
    let isDone = false;

    const result: string = await withProgressAfterDelay<string>(
      {
        location: ProgressLocation.Notification,
        inject: { withProgress: withProgressMock, synchronizationObject: {} },
      },
      async () => {
        await sleep(1);
        isDone = true;
        return "hi";
      }
    );

    expect(isDone).toBeTruthy();
    expect(withProgressMock).toHaveBeenCalledTimes(0);
    expect(result).toBe("hi");
  });

  it("should not show progress notification if task is short (using short delay)", async () => {
    const withProgressMock = createWithProgressMock<void>();
    let isDone = false;

    const result: string = await withProgressAfterDelay<string>(
      {
        location: ProgressLocation.Notification,
        delayBeforeShowingMs: 10,
        inject: { withProgress: withProgressMock, synchronizationObject: {} },
      },
      async () => {
        await sleep(1);
        isDone = true;
        return "hi";
      }
    );

    expect(isDone).toBeTruthy();
    expect(withProgressMock).toHaveBeenCalledTimes(0);
    expect(result).toBe("hi");
  });

  it("should show progress notification if task takes longer than delay", async () => {
    const withProgressMock = createWithProgressMock<void>();
    let isDone = false;

    const result: number = await withProgressAfterDelay(
      {
        location: ProgressLocation.Notification,
        delayBeforeShowingMs: 1,
        inject: { withProgress: withProgressMock, synchronizationObject: {} },
      },
      async () => {
        await sleep(10);
        isDone = true;
        return 123;
      }
    );

    expect(isDone).toBeTruthy();
    expect(withProgressMock).toHaveBeenCalledTimes(1);
    expect(result).toBe(123);
  });

  it("should handle throw before notification shows", async () => {
    const withProgressMock = createWithProgressMock<void>();

    const func = async () =>
      withProgressAfterDelay(
        {
          location: ProgressLocation.Notification,
          inject: { withProgress: withProgressMock, synchronizationObject: {} },
        },
        async () => {
          throw new Error("hah!");
        }
      );
    await expect(func).rejects.toThrow("hah!");

    expect(withProgressMock).toHaveBeenCalledTimes(0);
  });

  it("should handle throw after notification shows", async () => {
    const withProgressMock = createWithProgressMock<void>();

    const func = async () =>
      withProgressAfterDelay(
        {
          location: ProgressLocation.Notification,
          delayBeforeShowingMs: 1,
          inject: { withProgress: withProgressMock, synchronizationObject: {} },
        },
        async () => {
          await sleep(10);
          expect(withProgressMock).toHaveBeenCalledTimes(1);
          throw new Error("hah!");
        }
      );
    await expect(func).rejects.toThrow("hah!");

    expect(withProgressMock).toHaveBeenCalledTimes(1);
  });

  it("should not show multiple notifications at once", async () => {
    const withProgressMock = createWithProgressMock<void>();
    const synchronizationObject = {};

    let semaphore1 = false;
    let semaphore2 = false;

    const func1 = async () =>
      withProgressAfterDelay<number>(
        {
          location: ProgressLocation.Notification,
          delayBeforeShowingMs: 1,
          inject: { withProgress: withProgressMock, synchronizationObject },
        },
        async () => {
          expect(withProgressMock).toHaveBeenCalledTimes(0);

          while (!semaphore1) {
            await sleep(1);
          }

          return 1;
        }
      );

    const func2 = async () =>
      withProgressAfterDelay(
        {
          location: ProgressLocation.Notification,
          delayBeforeShowingMs: 1,
          inject: { withProgress: withProgressMock, synchronizationObject },
        },
        async () => {
          expect(withProgressMock).toHaveBeenCalledTimes(0);

          while (!semaphore2) {
            await sleep(1);
          }

          return 2;
        }
      );

    expect(withProgressMock).toHaveBeenCalledTimes(0);

    let result1: number | undefined;
    let result2: number | undefined;

    // Start up the functions (don't wait)
    (async () => {
      result1 = await func1();
    })();
    (async () => {
      result2 = await func2();
    })();

    // Wait long enough for progress to show
    await sleep(10);
    expect(withProgressMock).toHaveBeenCalledTimes(1);

    // Signal tasks to be done
    semaphore1 = true;
    semaphore2 = true;

    // Give them a chance to finish
    await sleep(2);

    expect(result1).toBe(1);
    expect(result2).toBe(2);

    expect(withProgressMock).toHaveBeenCalledTimes(1);
  });

  it("asdfg first call to withProgressAfterDelay should have its value available immediately after resolved", async () => {
    Create version of createWithProgressMock that allows me to know when the progress is done
    
    const withProgressMock = createWithProgressMock<void>();
    const synchronizationObject = {};
    const options = {
      location: ProgressLocation.Notification,
      delayBeforeShowingMs: 1,
      inject: { withProgress: withProgressMock, synchronizationObject },
    };

    const deferred1 = new Deferred<number>();
    const deferred2 = new Deferred<number>();

    const func1 = async () =>
      withProgressAfterDelay<number>(
        options,
        async () => await deferred1.promise
      );

    const func2 = async () =>
      withProgressAfterDelay<number>(
        options,
        async () => await deferred2.promise
      );

    deferred1.resolve(1);
    await expect(func1()).resolves.toBe(1);

    deferred2.resolve(1);
    await expect(func2()).resolves.toBe(1);
  });
});
