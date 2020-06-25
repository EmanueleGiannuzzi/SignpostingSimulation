using System.Collections.Generic;

public class VisibilityInfo {
    private readonly List<int> visibleBoards = new List<int>();

    public void AddVisibleBoard(int boardID) {
        visibleBoards.Add(boardID);
    }

    public List<int> GetVisibleBoards() {
        return visibleBoards;
    }
}
