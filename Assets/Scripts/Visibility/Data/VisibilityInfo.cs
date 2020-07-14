using System.Collections.Generic;
using UnityEngine;

public class VisibilityInfo {
    public readonly Vector3 cachedWorldPos;
    private readonly List<int> visibleBoards = new List<int>();

    public VisibilityInfo(Vector3 worldPos) {
        this.cachedWorldPos = worldPos;
    }

    public void AddVisibleBoard(int boardID) {
        visibleBoards.Add(boardID);
    }

    public List<int> GetVisibleBoards() {
        return visibleBoards;
    }
}
