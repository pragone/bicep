// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import assert from "assert";

export class Deferred<T> {
  private _promise: Promise<T>;
  private _resolve?: (value: T) => void;
  private _reject?: (reason?: unknown) => void;

  constructor() {
    this._promise = new Promise((resolve, reject) => {
      this._resolve = resolve;
      this._reject = reject;
    });
  }

  public get promise(): Promise<T> {
    return this._promise;
  }

  public resolve(value: T): void {
    assert(this._resolve);
    this._resolve(value);
  }

  public reject(reason?: unknown): void {
    assert(this._reject);
    this._reject(reason);
  }
}
