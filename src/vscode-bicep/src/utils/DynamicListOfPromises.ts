// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import assert from "assert";

export class DynamicListOfPromises {
  private promises: Promise<unknown>[] = [];

  private isComplete = false;
  private combinedPromise?: Promise<void>;

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

    if (!this.combinedPromise) {
      this.combinedPromise = this.createNewPromise();
    }

    return this.combinedPromise;
  }

  private createNewPromise(): Promise<void> {
    return new Promise<void>((resolve) => {
      DynamicListOfPromises.waitForNextPromise(this.promises, () => {
        assert(this.combinedPromise);
        this.combinedPromise = undefined;
        resolve();
      });
    });
  }

  private static waitForNextPromise(
    promises: Promise<unknown>[],
    resolve: (value: void) => void,
    previousPromiseToRemove?: Promise<unknown>
  ): void {
    if (previousPromiseToRemove) {
      // Remove the promise that just resolved. Can't remove it earlier because we
      //   don't want the count of promises to be 0 while we're waiting for an active promise
      assert(promises[0] === previousPromiseToRemove);
      promises.shift();
    }

    if (promises.length === 0) {
      resolve();
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
