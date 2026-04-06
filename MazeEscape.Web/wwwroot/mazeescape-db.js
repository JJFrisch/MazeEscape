window.mazeEscapeDb = (() => {
  const dbName = "MazeEscapeWeb";
  const storeName = "saveDocuments";
  const version = 1;

  function openDb() {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(dbName, version);

      request.onupgradeneeded = (event) => {
        const db = event.target.result;
        if (!db.objectStoreNames.contains(storeName)) {
          db.createObjectStore(storeName, { keyPath: "playerId" });
        }
      };

      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error || new Error("Failed to open IndexedDB"));
    });
  }

  async function saveDocument(playerId, payloadJson) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(storeName, "readwrite");
      const store = tx.objectStore(storeName);
      store.put({ playerId, payloadJson });
      tx.oncomplete = () => resolve(true);
      tx.onerror = () => reject(tx.error || new Error("Failed to save document"));
    });
  }

  async function loadDocument(playerId) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(storeName, "readonly");
      const store = tx.objectStore(storeName);
      const request = store.get(playerId);
      request.onsuccess = () => resolve(request.result ? request.result.payloadJson : null);
      request.onerror = () => reject(request.error || new Error("Failed to load document"));
    });
  }

  return {
    saveDocument,
    loadDocument
  };
})();
