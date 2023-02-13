// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { DynamicListOfPromises } from "../../utils/DynamicListOfPromises";

describe("dynamicListOfResources", () => {
  it("should resolve immediately if list empty", async () => {
    const list = new DynamicListOfPromises();
    const now = Date.now();

    await list.getPromise();

    const elapsed = Date.now() - now;
    expect(elapsed).toBeLessThanOrEqual(1);
  });
});
