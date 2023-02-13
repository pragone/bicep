// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import assert from "assert";

export class DynamicListOfPromises {
  private promises: Promise<unknown>[] = [];

  private isComplete = false;
  private combinedPromise = new Promise<void>((resolve) => {
    DynamicListOfPromises.waitForNextPromise(this.promises, resolve);
  });

  public add(p: Promise<unknown>): void {
    if (this.isComplete) {
      throw new Error(
        "Can't add new promises after the current promises have all completed"
      );
    }
    this.promises.push(p);
  }

  public getPromise(): Promise<void> {
    if (this.promises.length === 0) {
      return new Promise((resolve) => resolve());
    }

    return this.combinedPromise;
  }

  private static waitForNextPromise(
    promises: Promise<unknown>[],
    resolve: (value: void) => void,
    previousPromiseToRemove?: Promise<unknown>
  ): void {
    if (previousPromiseToRemove) {
      assert(promises[0] === previousPromiseToRemove);
    }

    if (promises.length === 0) {
      return;
    } else {
      // On resolve or reject, remove the promise from the list and wait for the next one
      const currentPromise = promises[0];
      currentPromise.then(
        () =>
          DynamicListOfPromises.waitForNextPromise(
            promises,
            resolve,
            currentPromise
          ),
        () =>
          DynamicListOfPromises.waitForNextPromise(
            promises,
            resolve,
            currentPromise
          )
      );
    }
  }
}
