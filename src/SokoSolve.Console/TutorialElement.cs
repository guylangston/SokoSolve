using SokoSolve.Core.Game;

namespace SokoSolve.Console
{
    public class TutorialElement : GameElement
    {
        private float elapsed;
        private float showingMessage;
        private int   curr;


        public string[] Tutorial = new[]
        {
            "",
            "Get all the crates onto the goals",
            "Push but never pull",
            "Push one at a time",
            "Push into a free space",
            "Solve a puzzle to unlock Move Movement",
            "Corners are dangerous",
            "Take your time",
            "Puzzle get harder as you get better",
            "Soboban is a Japanese Game",
            "Soboban means Warehouse Keeper",
            "You can use the mouse to move"
        };

        public string CurrentMessageText { get; set; }
        
        public override void Step(float elapsedSec)
        {
            elapsed += elapsedSec;
            if (elapsed > 2)
            {
                if (CurrentMessageText == null)
                {
                    curr++;
                    if (curr >= Tutorial.Length) curr = 0;
                    CurrentMessageText = Tutorial[curr];
                    elapsed            = 0;    
                }
                else
                {
                    CurrentMessageText = null;
                    elapsed = 0;
                }
                
            }
            base.Step(elapsedSec);
        }
    }
}