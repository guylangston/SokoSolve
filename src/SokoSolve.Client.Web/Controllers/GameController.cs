using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SokoSolve.Client.Web.Logic;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using VectorInt;

namespace SokoSolve.Client.Web.Controllers
{
    public class GameController : Controller
    {
        private readonly LibraryComponent compLib;
        private readonly ISokobanSolutionRepository repSol;
        private readonly ServerSideStateComponent compState;

        public GameController(LibraryComponent compLib, ISokobanSolutionRepository repSol, ServerSideStateComponent compState)
        {
            this.compLib   = compLib;
            this.repSol    = repSol;
            this.compState = compState;
        }


        public IActionResult Start(string id)
        {
            var ident = PuzzleIdent.Parse(id);
            var p     = compLib.GetPuzzleWithCaching(ident);

            var game  = new ServerSideGame(p);
            var lease = compState.CreateLease(game);

            return RedirectToAction("Instance", new
            {
                id = lease.LeaseId,
                Reload = Url.Action("Start", new {id})
            });
        }
        
        
        public IActionResult Instance(int id, string reload)
        {
            if (compState.TryGetLease<ServerSideGame>(id, out var lease))
            {
                return View(lease);    
            }
            return Redirect(reload);
        }
        
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Update(int id, string verb, string key, int mouseX, int mouseY, int e)
        {
            var lease = compState.GetLease<ServerSideGame>(id);
            try
            {
                if (verb == "init")
                {
                    return Json(new
                    {
                        Text =$"[{id}-{e}]InitComplete",
                        Html = lease.State.Draw() 
                    });
                }
                else if (verb == "key")
                {
                    var res = MoveResult.Invalid;
                    if      (key == "ArrowUp") res         = lease.State.Move(VectorInt2.Up);
                    else if (key == "ArrowDown") res  = lease.State.Move(VectorInt2.Down);
                    else if (key == "ArrowLeft") res  = lease.State.Move(VectorInt2.Left);
                    else if (key == "ArrowRight") res = lease.State.Move(VectorInt2.Right);
                    else if (key == "KeyU") lease.State.UndoMove();
                    else if (key == "KeyR") lease.State.Reset();
                    else if (key == "KeyD")     // DEBUG
                    {
                        return Json(new
                        {
                            Text =$"[{id}-{e}]Key({key})+{res}",
                            Target = "serverside-debug",
                            Html = $"<pre>{GetDebug(lease.State)}</pre>" 
                        }); 
                    }
                    
                    return Json(new
                    {
                        Text =$"[{id}-{e}]Key({key})+{res}",
                        Html = lease.State.Draw() 
                    });       
                }
                else if (verb == "click")
                {
                    var res = lease.State.Click(new VectorInt2(mouseX, mouseY));
                    return Json(new
                    {
                        Text =$"[{id}-{e}]Click({mouseX})+{mouseY})={res}",
                        Html = lease.State.Draw()
                    });   
                }

                return Json(new {Error =$"[{id}-{e}]UnknownAction={verb}"});
            }
            catch (Exception exception)
            {
                return Json(new {Error =$"[{id}-{e}]Exception={exception.Message}"});
            }
            
           

           
        }
        
        private string GetDebug(ServerSideGame game)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Player: {game.Current.Player.Position}");
            sb.Append(game.Current.ToString());

            sb.AppendLine();
            sb.AppendLine("MoveMap");
            var map = SolverHelper.FloodFillUsingWallAndCrates(game.Current.ToMap(game.Current.Definition.Wall),
                game.Current.ToMap(game.Current.Definition.Crate),
                game.Current.Player.Position);
            sb.Append(map.ToString());

            return sb.ToString();
        }
    }
}