using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Model.DataModel;
using Path = System.IO.Path;

namespace SokoSolve.Core.Lib
{
    public class LibraryComponent
    {
        private readonly string basePath;
        Dictionary<string, Library> cacheLibs = new Dictionary<string, Library>();


        private readonly LibraryCollection collection; 

        public LibraryComponent(string basePath)
        {
            this.basePath = basePath;
            collection = new LibraryCollection
            {
                Items = new List<LibrarySummary>()
                {
                    new LibrarySummary()
                    {
                        Id = "SQ1",
                        Name =  "Sasquatch",
                        FileName =  "Sasquatch.ssx"
                    },
                    new LibrarySummary()
                    {
                        Id       = "SQ3",
                        Name     =  "Sasquatch III",
                        FileName =  "SasquatchIII.ssx"
                    },
                    new LibrarySummary()
                    {
                        Id       = "SQ4",
                        Name     =  "Sasquatch IV",
                        FileName =  "SasquatchIV.ssx"
                    },
                    new LibrarySummary()
                    {
                        Id       = "SQ5",
                        Name     =  "Sasquatch V",
                        FileName =  "SasquatchV.ssx"
                    },
                   
                    new LibrarySummary()
                    {
                        Id       = "MB",
                        Name     =  "Microban",
                        FileName =  "Microban.ssx"
                    },
                    new LibrarySummary()
                    {
                        Id       = "MB2",
                        Name     =  "Mas Microban",
                        FileName =  "Mas Microban.ssx"
                    },
                    new LibrarySummary()
                    {
                        Id       = "TR",
                        Name     =  "Thinking Rabbit Inc, Origonal",
                        FileName =  "Thinking Rabbit Inc, Origonal.ssx"
                    },
                }
            };
        }

        public string GetPathData(string rel)
        {
            return Path.Combine(basePath, rel);
        }

        public LibraryCollection GetDefaultLibraryCollection() => collection;

        public Library GetLibraryWithCaching(string id)
        {
            if (cacheLibs.TryGetValue(id, out var l)) return l;

            l = LoadLibrary(GetPathData( GetDefaultLibraryCollection().IdToFileName(id)));
            cacheLibs.Add(id, l);
            return l;
        }
        
        public LibraryPuzzle GetPuzzleWithCaching(PuzzleIdent ident)
        {
            var l = GetLibraryWithCaching(ident.Library);
            return l?.FirstOrDefault(x=>x.Ident.Puzzle == ident.Puzzle) ?? throw new ArgumentException($"NotFound: {ident}");
        }
        
        public Library LoadLibraryRel(string fileName)
        {
            return LoadLegacySokoSolve_SSX(Path.Combine(basePath, fileName));
        }

        public Library LoadLibrary(string fileName)
        {
            var lib = LoadLegacySokoSolve_SSX(fileName);
            
            // Upgrade
            foreach (var puz in lib)
            {
                puz.Rating = StaticAnalysis.CalculateRating2(puz.Puzzle);
            }
            return lib;
        }

        public void SaveLibrary(Library lib, string fileName)
        {
            throw new NotImplementedException();
        }

        // public Profile LoadProfile(string fileName)
        // {
        //     var profile = new Profile
        //     {
        //         FileName = fileName,
        //         Current = null,
        //         Statistics = new Statistics()
        //     };
        //     var pairs = TrivialNameValueFileFormat.Load(fileName);
        //     var binder = new TrivialNameValueFileFormat.WithBinder<Profile>();
        //     foreach (var pair in pairs)
        //     {
        //         binder.SetWhen(pair, profile, x => x.Name);
        //         binder.SetWhen(pair, profile, x => x.Created);
        //         binder.SetWhen(pair, profile, x => x.TimeInGame);
        //         binder.With(profile, x => x.Current)
        //             .SetWhen(pair, x => x.Library)
        //             .SetWhen(pair, x => x.Puzzle);
        //         binder.With(profile, x => x.Statistics)
        //             .SetWhen(pair, x => x.Pushes)
        //             .SetWhen(pair, x => x.Steps)
        //             .SetWhen(pair, x => x.Started)
        //             .SetWhen(pair, x => x.Undos)
        //             .SetWhen(pair, x => x.Restarts)
        //             ;
        //     }
        //
        //     return profile;
        // }


        // public TrivialNameValueFileFormat SaveProfile(Profile lib, string fileName)
        // {
        //     var txt = TrivialNameValueFileFormat.Serialise(lib);
        //     txt.Save(fileName);
        //     return txt;
        // }

        public Library LoadLegacySokoSolve_SSX(string fileName)
        {
            var ser = new XmlSerializer(typeof(SokobanLibrary));

            SokobanLibrary? xmlLib = null;
            using (var reader = File.OpenText(fileName))
            {
                xmlLib = (SokobanLibrary) ser.Deserialize(reader);
            }

            var lib = Convert(xmlLib);
            foreach (var puzzle in lib)
            {
                puzzle.Rating = StaticAnalysis.CalculateRating(puzzle.Puzzle);
            }

            return lib;
        }

        private Library Convert(SokobanLibrary xmlLib)
        {
            var lib = new Library()
            {
                Details = new AuthoredItem()
                {
                    Id = xmlLib.LibraryID
                }
            };
            foreach (var puzzle in xmlLib.Puzzles)
            {
                var map = puzzle.Maps.FirstOrDefault();
                if (map != null && map.Row != null)
                {
                    var lp = Convert(puzzle, map);
                    lp.Ident = new PuzzleIdent(lib.Details.Id, lp.Details.Id);
                    lib.Add(lp);
                }
            }

            return lib;
        }

        private LibraryPuzzle Convert(SokobanLibraryPuzzle xmlLib, SokobanLibraryPuzzleMap xp)
        {
            return new LibraryPuzzle(Puzzle.Builder.FromLines(xp.Row))
            {
                Name = xmlLib.PuzzleDescription != null ? xmlLib.PuzzleDescription.Name : null,
                Details = new AuthoredItem
                {
                    Id = xmlLib.PuzzleID,
                    Name = xmlLib.PuzzleDescription != null ? xmlLib.PuzzleDescription.Name : null
                }
            };
        }

        public List<LibraryPuzzle> LoadAllPuzzles(IEnumerable<PuzzleIdent> idents)
        {
            var libs = idents.Select(x => x.Library)
                .Distinct()
                .Select(
                    x => new Tuple<string, Library>(x, LoadLibrary(GetPathData(x))))
                .ToDictionary(x => x.Item1, x => x.Item2);

            var res = new List<LibraryPuzzle>();
            foreach (var ident in idents)
            {
                var p = libs[ident.Library][ident.Puzzle];
                if (p == null) throw new Exception("Ident not found:" + ident);
                res.Add(p);
            }

            return res;
        }

        public IEnumerable<LibraryPuzzle> GetPuzzlesWithCachingUsingRegex(string searchString)
        {
            if (searchString == "all")
            {
                foreach (var  sum in collection.Items)
                {
                    var sumLib = GetLibraryWithCaching(sum.Id);
                    foreach (var ll in sumLib)
                    {
                        yield return ll;
                    }
                }
            }
            
            if (searchString.Contains("~"))
            {
                // Try direct
                LibraryPuzzle p = null;
                try
                {
                     p = GetPuzzleWithCaching(PuzzleIdent.Parse(searchString));
                }
                catch (ArgumentException) { }
                catch (InvalidDataException) { }

                if (p != null)
                {
                    yield return p;
                    yield break;
                }
            }

            Library lib = null;
            try
            {
                lib = GetLibraryWithCaching(searchString);
            }
            catch (ArgumentException) { }
            catch (InvalidDataException) { }
            catch (FileNotFoundException) { }

            if (lib != null)
            {
                foreach (var  ll in lib)
                {
                    yield return ll;
                }

                yield break;
            }
            
            // TODO: Regex
            
            
        }
    }
}