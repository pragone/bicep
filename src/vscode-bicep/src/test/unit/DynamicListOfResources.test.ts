// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/* eslint-disable jest/max-expects */

import { Deferred } from "../../utils/Deferred";
import { DynamicListOfPromises } from "../../utils/DynamicListOfPromises";
import { sleep } from "../../utils/time";

describe("dynamicListOfResources", () => {
  it("should resolve immediately if list empty", async () => {
    const list = new DynamicListOfPromises();

    const promise1 = list.getPromise();
    await expect(promise1).resolves.toBeUndefined();

    const promise2 = list.getPromise();
    await expect(promise2).resolves.toBeUndefined();
  });

  it("should wait for single promise", async () => {
    const list = new DynamicListOfPromises();
    const deferred = new Deferred<number>();
    let counter = 0;

    const taskPromise = new Promise((resolve, reject) => {
      deferred.promise.then(() => {
        counter++;
        resolve(123);
      }, reject);
    });
    list.add(taskPromise);

    const listPromise = list.getPromise();

    await sleep(1);
    expect(counter).toBe(0);

    deferred.resolve(123);
    await expect(listPromise).resolves.toBeUndefined();

    expect(counter).toBe(1);
    await expect(taskPromise).resolves.toBe(123);
  });

  it("should wait for single rejected promise", async () => {
    const list = new DynamicListOfPromises();
    const deferred = new Deferred<number>();
    let counter = 0;

    const taskPromise = new Promise((resolve, reject) => {
      deferred.promise.then(() => {
        counter++;
        resolve(123);
      }, reject);
    });
    list.add(taskPromise);

    const listPromise = list.getPromise();

    await sleep(1);
    expect(counter).toBe(0);

    deferred.reject("whoops");
    await expect(taskPromise).rejects.toBe("whoops"); // task promise should reject
    await expect(listPromise).resolves.toBeUndefined(); // list promise should resolve
    expect(counter).toBe(0);
  });

  it("should resolve after all promises are resolved or rejected", async () => {
    const list = new DynamicListOfPromises();
    const deferred1 = new Deferred<number>();
    const deferred2 = new Deferred<number>();
    const deferred3 = new Deferred<number>();
    let finished = false;

    list.add(deferred1.promise);
    list.add(deferred2.promise);
    list.add(deferred3.promise);

    list.getPromise().then(() => (finished = true));

    await sleep(1);
    expect(finished).toBe(false);

    deferred1.resolve(123);
    await sleep(1);
    expect(finished).toBe(false);

    deferred2.reject("whoops");
    await sleep(1);
    expect(finished).toBe(false);

    deferred3.resolve(321);
    await sleep(1);
    expect(finished).toBe(true);
  });

  it("should handle new promises added after some aleady resolved", async () => {
    const list = new DynamicListOfPromises();
    const deferred1 = new Deferred<number>();
    const deferred2 = new Deferred<number>();
    const deferred3 = new Deferred<number>();
    let finished = false;

    list.add(deferred1.promise);
    list.add(deferred2.promise);
    list.add(deferred3.promise);

    list.getPromise().then(() => (finished = true));

    await sleep(1);
    expect(finished).toBe(false);

    deferred1.resolve(123);
    deferred2.reject("whoops");
    await sleep(1);
    expect(finished).toBe(false);

    const deferred4 = new Deferred<string>();
    const deferred5 = new Deferred<string>();
    list.add(deferred4.promise);
    list.add(deferred5.promise);

    deferred4.reject("whoops2");

    const deferred6 = new Deferred<void>();
    list.add(deferred6.promise);

    deferred5.resolve("yay");
    deferred6.resolve();

    expect(finished).toBe(false);

    deferred3.resolve(3);

    await list.getPromise();
    await sleep(1);
    expect(finished).toBe(true);
  });

  it("should return same promise when multiple promises are activate", async () => {
    const list = new DynamicListOfPromises();

    const deferred1 = new Deferred<number>();
    list.add(deferred1.promise);

    const promise1 = list.getPromise();

    const deferred2 = new Deferred<number>();
    list.add(deferred2.promise);
    const promise2 = list.getPromise();
    expect(promise2).toBe(promise1);

    const deferred3 = new Deferred<number>();
    list.add(deferred3.promise);
    const promise3 = list.getPromise();
    expect(promise3).toBe(promise1);

    // Resolve all promises (don't bother awaiting them)
    await sleep(1);
    deferred1.resolve(1);
    deferred2.resolve(2);
    deferred3.resolve(3);

    await sleep(1);

    // New promise should be different from previous (empty, so it resolves immediately)
    const emptyPromise = list.getPromise();
    expect(emptyPromise).not.toBe(promise1);
    await expect(emptyPromise).resolves.toBeUndefined();

    // Add more task promises, they should all return the same new list promise
    const deferred4 = new Deferred<number>();
    list.add(deferred4.promise);
    const promise4 = list.getPromise();
    expect(promise4).not.toBe(promise1);
    expect(promise4).not.toBe(emptyPromise);

    const deferred5 = new Deferred<number>();
    list.add(deferred5.promise);
    const promise5 = list.getPromise();
    expect(promise5).not.toBe(promise1);
    expect(promise5).not.toBe(emptyPromise);
    expect(promise5).toBe(promise4);

    deferred4.resolve(4);
    deferred5.reject("whoops");
    await sleep(1);
    await expect(promise3).resolves.toBeUndefined();

    // Empty again
    await expect(list.getPromise()).resolves.toBeUndefined();
  });
});
