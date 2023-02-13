// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Deferred } from "../../utils/Deferred";
import { sleep } from "../../utils/time";

describe("deferred", () => {
  it("should resolve when deferred is resolved", async () => {
    const deferred = new Deferred<number>();

    let finished = false;
    const funcPromise = (async () => {
      await deferred.promise;
      finished = true;
    })();

    await sleep(2);
    expect(finished).toBe(false);

    deferred.resolve(123);

    await expect(funcPromise).resolves.toBeUndefined();
    await expect(deferred.promise).resolves.toBe(123);
    expect(finished).toBe(true);
  });

  it("should reject when deferred is rejected", async () => {
    const deferred = new Deferred<void>();

    let finished = false;
    const funcPromise = (async () => {
      await deferred.promise;
      finished = true;
    })();

    await sleep(2);
    expect(finished).toBe(false);

    deferred.reject("Deferred was rejected");

    await expect(funcPromise).rejects.toBe("Deferred was rejected");
    await expect(deferred.promise).rejects.toBe("Deferred was rejected");
    expect(finished).toBe(false);
  });
});
